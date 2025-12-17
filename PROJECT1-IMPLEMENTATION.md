# Project 1: Simple Key-Value Store API

## 📋 Overview

This project implements a RESTful API for managing user profiles using Redis as the primary data store. It demonstrates fundamental Redis operations including string operations, TTL (Time To Live) management, and the proper use of the StackExchange.Redis library in ASP.NET Core.

---

## 🎯 Learning Objectives

By completing this project, you will learn:

1. **Redis Fundamentals**
   - String data type operations (SET, GET, DEL)
   - Key expiration and TTL management
   - Key naming conventions and best practices

2. **StackExchange.Redis Library**
   - ConnectionMultiplexer pattern (singleton connection)
   - IDatabase interface usage
   - Async operations in Redis

3. **ASP.NET Core Integration**
   - Dependency injection with Redis
   - Service layer architecture
   - Configuration management

4. **Data Serialization**
   - JSON serialization for complex objects
   - Converting C# objects to Redis strings

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      Client (HTTP)                      │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│              UsersController (API Layer)                │
│  • POST /api/users                                      │
│  • GET /api/users/{id}                                  │
│  • PUT /api/users/{id}                                  │
│  • DELETE /api/users/{id}                               │
│  • GET /api/users/{id}/ttl                              │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│            RedisService (Business Logic)                │
│  • CreateUserAsync()                                    │
│  • GetUserAsync()                                       │
│  • UpdateUserAsync()                                    │
│  • DeleteUserAsync()                                    │
│  • GetTtlAsync()                                        │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│          StackExchange.Redis (IDatabase)                │
│  • StringSetAsync()                                     │
│  • StringGetAsync()                                     │
│  • KeyDeleteAsync()                                     │
│  • KeyTimeToLiveAsync()                                 │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                    Redis Server                         │
│                  (Key-Value Store)                      │
└─────────────────────────────────────────────────────────┘
```

---

## 📦 Project Structure

```
Project1.KeyValueStoreAPI/
├── Controllers/
│   └── UsersController.cs          # API endpoints
├── Models/
│   └── UserProfile.cs               # User data model
├── Services/
│   ├── IRedisService.cs             # Service interface
│   └── RedisService.cs              # Redis implementation
├── Program.cs                       # App configuration
├── appsettings.json                 # Configuration
├── appsettings.Development.json     # Dev configuration
└── docker-compose.yml               # Redis container setup
```

---

## 🔧 Key Components Explained

### 1. **ConnectionMultiplexer Pattern**

```csharp
// In Program.cs - Registered as SINGLETON
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});
```

**Why Singleton?**
- ConnectionMultiplexer is designed to be shared and reused
- It manages connection pooling internally
- Creating multiple instances wastes resources
- Thread-safe by design

**Important Settings:**
- `AbortOnConnectFail = false`: Retries connection instead of throwing exception
- Connection string format: `"localhost:6379"` or `"host:port,password=secret"`

---

### 2. **RedisService Implementation**

#### Key Naming Convention
```csharp
private string _keyPrefix = "user:";
private string GetKey(string userId) => $"{_keyPrefix}{userId}";
```

**Best Practice:** Always use prefixes for keys
- Example: `user:123`, `session:abc`, `cart:xyz`
- Helps organize data in Redis
- Prevents key collisions
- Makes debugging easier

#### Create User with Conditional SET
```csharp
public async Task<bool> CreateUserAsync(UserProfile user, TimeSpan? expiry = null)
{
    var key = GetKey(user.Id);
    var json = JsonSerializer.Serialize(user);
    
    // SET with NX (Not eXists) - only set if key doesn't exist
    var created = await _db.StringSetAsync(key, json, expiry, When.NotExists);
    return created;
}
```

**Redis Command Equivalent:**
```bash
SET user:123 "{\"id\":\"123\",\"name\":\"John\"}" NX EX 3600
```

**Explanation:**
- `When.NotExists`: Ensures we don't overwrite existing users
- Returns `true` if created, `false` if key already exists
- Optional `expiry` parameter sets TTL

#### Get User with Deserialization
```csharp
public async Task<UserProfile?> GetUserAsync(string userId)
{
    var key = GetKey(userId);
    var value = await _db.StringGetAsync(key);
    
    if (value.IsNullOrEmpty)
        return null;

    return JsonSerializer.Deserialize<UserProfile>(value!);
}
```

**Redis Command Equivalent:**
```bash
GET user:123
```

**Explanation:**
- Retrieves string value from Redis
- Returns null if key doesn't exist
- Deserializes JSON back to C# object

#### Update User with Existence Check
```csharp
public async Task<bool> UpdateUserAsync(UserProfile user, TimeSpan? expiry = null)
{
    var key = GetKey(user.Id);
    
    // Check existence first
    if (!await _db.KeyExistsAsync(key))
        return false;

    user.UpdatedAt = DateTime.UtcNow;
    var json = JsonSerializer.Serialize(user);
    
    // SET with XX (eXists) - only set if key exists
    await _db.StringSetAsync(key, json, expiry, When.Exists);
    return true;
}
```

**Redis Command Equivalent:**
```bash
SET user:123 "{\"id\":\"123\",\"name\":\"Jane\"}" XX
```

**Explanation:**
- `When.Exists`: Only updates if key already exists
- Updates `UpdatedAt` timestamp automatically
- Can optionally reset TTL on update

#### TTL Management
```csharp
public async Task<TimeSpan?> GetTtlAsync(string userId)
{
    var key = GetKey(userId);
    return await _db.KeyTimeToLiveAsync(key);
}
```

**Redis Command Equivalent:**
```bash
TTL user:123
```

**Return Values:**
- `TimeSpan`: Remaining time until expiration
- `null`: Key has no expiration (persistent)
- `TimeSpan(-1)`: Key doesn't exist

---

### 3. **Controller Layer**

#### Create User Endpoint
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(
    [FromBody] UserProfile user, 
    [FromQuery] int? ttlMinutes = null)
{
    if (string.IsNullOrWhiteSpace(user.Id))
    {
        user.Id = Guid.NewGuid().ToString(); // Auto-generate ID
    }

    TimeSpan? expiry = ttlMinutes.HasValue 
        ? TimeSpan.FromMinutes(ttlMinutes.Value) 
        : null;
    
    var created = await _redisService.CreateUserAsync(user, expiry);
    
    if (!created)
    {
        return Conflict(new { message = $"User with ID {user.Id} already exists" });
    }

    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}
```

**Usage Examples:**

Create user without TTL (persistent):
```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "dateOfBirth": "1990-01-01",
    "phoneNumber": "+1234567890",
    "address": "123 Main St"
  }'
```

Create user with 60-minute TTL:
```bash
curl -X POST "http://localhost:5000/api/users?ttlMinutes=60" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "user123",
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane@example.com",
    "dateOfBirth": "1985-05-15",
    "phoneNumber": "+0987654321",
    "address": "456 Oak Ave"
  }'
```

---

## 🚀 Getting Started

### Prerequisites

1. **.NET 10 SDK**
   ```bash
   dotnet --version  # Should be 10.0.x
   ```

2. **Docker Desktop** (for Redis)
   ```bash
   docker --version
   ```

### Step 1: Start Redis

Navigate to the project directory and start Redis using Docker Compose:

```bash
cd Project1.KeyValueStoreAPI
docker-compose up -d
```

**What this does:**
- Starts Redis server on port 6379
- Starts Redis Commander (GUI) on port 8081
- Creates persistent volume for data
- Enables AOF (Append Only File) persistence

**Verify Redis is running:**
```bash
docker ps
# Should show redis and redis-commander containers

# Test Redis connection
docker exec -it project1-redis redis-cli ping
# Should return: PONG
```

**Access Redis Commander:**
Open browser to `http://localhost:8081` to view Redis data graphically.

### Step 2: Build and Run the API

```bash
dotnet build
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

**OpenAPI/Swagger:**
In development mode, OpenAPI endpoint is available at:
- `http://localhost:5000/openapi/v1.json`

---

## 📖 API Endpoints

### 1. Create User
**Endpoint:** `POST /api/users`

**Query Parameters:**
- `ttlMinutes` (optional): Expiration time in minutes

**Request Body:**
```json
{
  "id": "user123",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "phoneNumber": "+1234567890",
  "address": "123 Main Street, City, State"
}
```

**Response (201 Created):**
```json
{
  "id": "user123",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "phoneNumber": "+1234567890",
  "address": "123 Main Street, City, State",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null
}
```

**Response (409 Conflict):**
```json
{
  "message": "User with ID user123 already exists"
}
```

---

### 2. Get User
**Endpoint:** `GET /api/users/{id}`

**Response (200 OK):**
```json
{
  "id": "user123",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "phoneNumber": "+1234567890",
  "address": "123 Main Street, City, State",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null
}
```

**Response (404 Not Found):**
```json
{
  "message": "User with ID user123 not found"
}
```

---

### 3. Update User
**Endpoint:** `PUT /api/users/{id}`

**Query Parameters:**
- `ttlMinutes` (optional): Reset expiration time

**Request Body:**
```json
{
  "id": "user123",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "phoneNumber": "+1234567890",
  "address": "456 New Avenue"
}
```

**Response (200 OK):**
```json
{
  "id": "user123",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "phoneNumber": "+1234567890",
  "address": "456 New Avenue",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T11:30:00Z"
}
```

---

### 4. Delete User
**Endpoint:** `DELETE /api/users/{id}`

**Response (204 No Content):** Empty body

**Response (404 Not Found):**
```json
{
  "message": "User with ID user123 not found"
}
```

---

### 5. Check TTL
**Endpoint:** `GET /api/users/{id}/ttl`

**Response (200 OK) - With TTL:**
```json
{
  "userId": "user123",
  "ttl": 2547,
  "message": "Key expires in 2547 seconds"
}
```

**Response (200 OK) - No TTL:**
```json
{
  "userId": "user123",
  "ttl": -1,
  "message": "Key has no expiration (persistent)"
}
```

---

### 6. Check User Existence
**Endpoint:** `GET /api/users/{id}/exists`

**Response (200 OK):**
```json
{
  "userId": "user123",
  "exists": true
}
```

---

## 🧪 Testing the API

### Using cURL

```bash
# Create a user
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Alice",
    "lastName": "Johnson",
    "email": "alice@example.com",
    "dateOfBirth": "1992-03-20",
    "phoneNumber": "+1122334455",
    "address": "789 Pine Road"
  }'

# Get user (replace {id} with actual ID from create response)
curl http://localhost:5000/api/users/{id}

# Update user
curl -X PUT http://localhost:5000/api/users/{id} \
  -H "Content-Type: application/json" \
  -d '{
    "id": "{id}",
    "firstName": "Alice",
    "lastName": "Williams",
    "email": "alice.williams@example.com",
    "dateOfBirth": "1992-03-20",
    "phoneNumber": "+1122334455",
    "address": "999 New Street"
  }'

# Check TTL
curl http://localhost:5000/api/users/{id}/ttl

# Delete user
curl -X DELETE http://localhost:5000/api/users/{id}
```

### Using Redis CLI

Monitor Redis operations in real-time:

```bash
# Connect to Redis container
docker exec -it project1-redis redis-cli

# Monitor all commands
MONITOR

# In another terminal, make API calls and watch the Redis commands execute
```

**View keys:**
```bash
# List all user keys
KEYS user:*

# Get specific user
GET user:123

# Check TTL
TTL user:123

# Delete key
DEL user:123
```

---

## 🔍 Understanding Redis Commands

### String Operations

| Operation | Redis Command | StackExchange.Redis |
|-----------|---------------|---------------------|
| Set value | `SET key value` | `StringSetAsync(key, value)` |
| Get value | `GET key` | `StringGetAsync(key)` |
| Set with NX | `SET key value NX` | `StringSetAsync(key, value, when: When.NotExists)` |
| Set with XX | `SET key value XX` | `StringSetAsync(key, value, when: When.Exists)` |
| Set with expiry | `SET key value EX 60` | `StringSetAsync(key, value, expiry: TimeSpan.FromSeconds(60))` |

### Key Operations

| Operation | Redis Command | StackExchange.Redis |
|-----------|---------------|---------------------|
| Check existence | `EXISTS key` | `KeyExistsAsync(key)` |
| Delete key | `DEL key` | `KeyDeleteAsync(key)` |
| Get TTL | `TTL key` | `KeyTimeToLiveAsync(key)` |
| Set expiry | `EXPIRE key 60` | `KeyExpireAsync(key, TimeSpan.FromSeconds(60))` |

---

## 💡 Key Concepts Explained

### 1. **Why Use Redis as Primary Storage?**

**Advantages:**
- ⚡ **Ultra-fast**: All data in memory, sub-millisecond latency
- 📊 **Simple data model**: Key-value pairs are intuitive
- 🔄 **Persistence options**: Can persist data to disk (RDB, AOF)
- 🌍 **Distributed**: Easy to scale horizontally

**Considerations:**
- 💰 **Memory cost**: More expensive than disk storage
- 📉 **Data size**: Best for datasets that fit in memory
- 🔄 **Persistence trade-offs**: Synchronous writes impact performance

**Use Cases:**
- Session storage
- Real-time analytics
- Caching layers
- Rate limiting
- Temporary data storage

---

### 2. **TTL (Time To Live) Patterns**

#### Session-like Data
```csharp
// 30-minute expiration for user sessions
await _redisService.CreateUserAsync(user, TimeSpan.FromMinutes(30));
```

#### Cache with Refresh
```csharp
// Reset TTL on every read
var user = await _redisService.GetUserAsync(userId);
if (user != null)
{
    await _redisService.UpdateUserAsync(user, TimeSpan.FromHours(1));
}
```

#### No Expiration (Persistent)
```csharp
// Null expiry = never expires
await _redisService.CreateUserAsync(user, expiry: null);
```

---

### 3. **Serialization Strategies**

#### JSON (Current Implementation)
```csharp
var json = JsonSerializer.Serialize(user);
await _db.StringSetAsync(key, json);
```

**Pros:**
- Human-readable
- Easy to debug
- Compatible with Redis Commander

**Cons:**
- Larger payload size
- Slower serialization

#### Alternative: MessagePack (For Production)
```csharp
// Install: MessagePack
var bytes = MessagePackSerializer.Serialize(user);
await _db.StringSetAsync(key, bytes);
```

**Pros:**
- Smaller payload (30-50% reduction)
- Faster serialization
- Binary format

**Cons:**
- Not human-readable
- Requires extra library

---

## 🐛 Common Issues and Solutions

### Issue 1: Connection Failures

**Error:**
```
StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s)
```

**Solutions:**
1. Check Redis is running:
   ```bash
   docker ps | grep redis
   ```

2. Verify connection string in `appsettings.json`

3. Check firewall settings

4. Use `AbortOnConnectFail = false` for retry logic

---

### Issue 2: Serialization Errors

**Error:**
```
System.Text.Json.JsonException: The JSON value could not be converted
```

**Solutions:**
1. Ensure all properties have getters and setters
2. Handle nullable types properly
3. Use `[JsonIgnore]` for properties you don't want to serialize

---

### Issue 3: Key Not Found

**Error:** Always getting 404 responses

**Solutions:**
1. Check key naming convention matches
2. Verify TTL hasn't expired
3. Use Redis Commander to inspect keys
4. Check Redis database number (default is 0)

---

## 📊 Monitoring and Debugging

### Using Redis Commander

1. Open `http://localhost:8081`
2. Browse keys by pattern (`user:*`)
3. View key values and TTL
4. Manually edit or delete keys

### Using Redis CLI

```bash
# Connect to Redis
docker exec -it project1-redis redis-cli

# Get server info
INFO

# Memory usage
INFO memory

# Check connected clients
CLIENT LIST

# Monitor commands in real-time
MONITOR
```

### Application Logging

The application logs Redis operations:

```csharp
_logger.LogInformation("Created user {UserId} with TTL: {TTL}", user.Id, expiry?.ToString() ?? "None");
```

View logs in console output.

---

## 🎓 Learning Exercises

### Exercise 1: Add Batch Operations
Implement a method to create multiple users at once using Redis transactions.

**Hint:** Use `ITransaction` interface:
```csharp
var transaction = _db.CreateTransaction();
// Add multiple operations
await transaction.ExecuteAsync();
```

### Exercise 2: Implement Search by Email
Add an endpoint to find users by email using Redis Sets or Hashes.

### Exercise 3: Add Refresh TTL Endpoint
Create `POST /api/users/{id}/refresh-ttl` to extend expiration time.

### Exercise 4: Implement Soft Delete
Instead of deleting keys, add an `IsDeleted` flag and filter deleted users.

### Exercise 5: Add Connection Health Check
Implement ASP.NET Core health checks for Redis connection.

**Hint:**
```csharp
builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString);
```

---

## 🚀 Production Considerations

### 1. **Connection Resilience**
```csharp
var configuration = ConfigurationOptions.Parse(redisConnectionString);
configuration.AbortOnConnectFail = false;
configuration.ConnectRetry = 3;
configuration.ConnectTimeout = 5000;
configuration.SyncTimeout = 1000;
```

### 2. **Error Handling**
Always wrap Redis operations in try-catch blocks and log errors.

### 3. **Key Expiration Strategy**
- Set default TTL for all keys to prevent unbounded memory growth
- Use appropriate expiration times based on use case
- Monitor memory usage with Redis INFO command

### 4. **Security**
```csharp
// Production connection string with password
"localhost:6379,password=your_secure_password,ssl=true"
```

### 5. **Monitoring**
- Track command execution time
- Monitor memory usage
- Set up alerts for connection failures
- Log slow commands

---

## 📚 Additional Resources

### StackExchange.Redis Documentation
- [GitHub Repository](https://github.com/StackExchange/StackExchange.Redis)
- [API Documentation](https://stackexchange.github.io/StackExchange.Redis/)

### Redis Documentation
- [Redis Commands](https://redis.io/commands)
- [Redis Data Types](https://redis.io/topics/data-types)
- [Redis Persistence](https://redis.io/topics/persistence)

### Best Practices
- [Redis Best Practices](https://redis.io/docs/manual/patterns/)
- [Key Naming Conventions](https://redis.io/docs/manual/patterns/naming/)

---

## ✅ Project Checklist

- [x] Redis installed and running in Docker
- [x] ConnectionMultiplexer configured as singleton
- [x] CRUD operations implemented
- [x] TTL management working
- [x] Proper error handling
- [x] Logging configured
- [x] JSON serialization working
- [ ] Tested all endpoints with curl
- [ ] Viewed data in Redis Commander
- [ ] Monitored Redis commands with CLI
- [ ] Understood key naming conventions
- [ ] Learned When.NotExists vs When.Exists

---

## 🎯 Summary

You've successfully built a Redis-backed API that demonstrates:

1. ✅ **Connection Management**: Singleton ConnectionMultiplexer pattern
2. ✅ **String Operations**: SET, GET, DEL with conditional operations
3. ✅ **TTL Management**: Setting and checking key expiration
4. ✅ **Serialization**: JSON serialization for complex objects
5. ✅ **Service Architecture**: Clean separation of concerns
6. ✅ **Error Handling**: Proper error responses and logging

**Next Steps:**
- Move to **Project 2: Leaderboard System** to learn about Sorted Sets
- Explore Redis Hashes for more efficient storage
- Implement caching patterns in Project 3

**Congratulations!** You've completed Phase 1 of the Redis learning journey! 🎉
