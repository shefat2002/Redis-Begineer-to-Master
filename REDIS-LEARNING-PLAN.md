# Redis Learning Plan: Beginner to Advanced
## ASP.NET Core with SignalR Focus

---

## 📚 Learning Path Overview

This plan progresses through 5 phases with hands-on projects, focusing on Redis integration with ASP.NET Core and SignalR for real-time applications.

---

## Phase 1: Redis Fundamentals (Week 1-2)

### Theory
- What is Redis and why use it?
- Redis data structures (Strings, Lists, Sets, Sorted Sets, Hashes, Streams)
- Redis architecture and persistence (RDB, AOF)
- Installing Redis (Docker recommended)

### Project 1: Simple Key-Value Store API
**Goal**: Build a basic CRUD API using Redis as primary storage

**Features**:
- Store and retrieve user profiles
- String operations (SET, GET, DEL, EXPIRE)
- Hash operations for structured data
- Basic TTL (Time To Live) management

**NuGet Packages**:
```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
```

**Endpoints**:
- POST `/api/users` - Create user profile
- GET `/api/users/{id}` - Get user profile
- PUT `/api/users/{id}` - Update user profile
- DELETE `/api/users/{id}` - Delete user profile
- GET `/api/users/{id}/ttl` - Check remaining TTL

**Learning Outcomes**:
- Connect to Redis using StackExchange.Redis
- Understand ConnectionMultiplexer pattern
- Work with IDatabase interface
- Implement serialization/deserialization

---

## Phase 2: Advanced Redis Data Structures (Week 3-4)

### Theory
- Lists, Sets, and Sorted Sets use cases
- Redis Pub/Sub mechanism
- Redis Streams for event sourcing
- Lua scripting basics

### Project 2: Real-Time Leaderboard System
**Goal**: Build a gaming leaderboard with real-time updates

**Features**:
- Store player scores using Sorted Sets
- Top N players retrieval
- Rank calculation
- Score updates with atomic operations
- Player statistics using Hashes

**NuGet Packages**:
```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
```

**Endpoints**:
- POST `/api/leaderboard/score` - Submit score
- GET `/api/leaderboard/top/{n}` - Get top N players
- GET `/api/leaderboard/rank/{playerId}` - Get player rank
- GET `/api/leaderboard/player/{playerId}` - Get player stats
- GET `/api/leaderboard/range/{start}/{end}` - Get rank range

**Learning Outcomes**:
- Master Sorted Sets (ZADD, ZRANGE, ZRANK, ZSCORE)
- Atomic operations
- Redis transactions (MULTI/EXEC)
- Performance optimization with batch operations

---

## Phase 3: Redis Caching Strategies (Week 5-6)

### Theory
- Cache-aside pattern
- Write-through and Write-behind patterns
- Cache invalidation strategies
- Distributed caching concepts
- IDistributedCache abstraction

### Project 3: E-Commerce Product Catalog with Caching
**Goal**: Build a product catalog API with multi-level caching

**Features**:
- Product CRUD with database (SQL Server/PostgreSQL)
- Redis distributed cache implementation
- Cache-aside pattern implementation
- Cache warming strategies
- Cache invalidation on updates
- Tag-based cache invalidation

**NuGet Packages**:
```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
```

**Endpoints**:
- GET `/api/products` - List products (cached)
- GET `/api/products/{id}` - Get product (cached)
- GET `/api/products/category/{category}` - Get by category (cached)
- POST `/api/products` - Create product (invalidate cache)
- PUT `/api/products/{id}` - Update product (invalidate cache)
- DELETE `/api/products/{id}` - Delete product (invalidate cache)
- POST `/api/cache/clear` - Manual cache clear

**Learning Outcomes**:
- Implement IDistributedCache interface
- Cache key design patterns
- Serialization strategies (JSON, MessagePack)
- Cache stampede prevention
- Monitoring cache hit/miss ratios

---

## Phase 4: SignalR with Redis Backplane (Week 7-8)

### Theory
- SignalR architecture and concepts
- Hubs and connections
- Groups and user management
- Scale-out with Redis backplane
- Connection management at scale

### Project 4: Real-Time Chat Application
**Goal**: Build a scalable chat app with Redis backplane

**Features**:
- Real-time messaging
- Chat rooms (SignalR groups)
- Online user tracking
- Message history (Redis Streams)
- Typing indicators
- Read receipts
- Multiple server instances support

**NuGet Packages**:
```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="10.0.0" />
```

**Hub Methods**:
- `SendMessage(roomId, message)` - Send message to room
- `JoinRoom(roomId)` - Join chat room
- `LeaveRoom(roomId)` - Leave chat room
- `GetOnlineUsers(roomId)` - Get active users
- `StartTyping(roomId)` - Broadcast typing indicator
- `StopTyping(roomId)` - Stop typing indicator

**Client Events**:
- `ReceiveMessage` - New message received
- `UserJoined` - User joined room
- `UserLeft` - User left room
- `UserTyping` - User is typing
- `MessageRead` - Message read confirmation

**Learning Outcomes**:
- Configure SignalR Redis backplane
- Implement Hub lifecycle events
- Manage SignalR groups in Redis
- Handle connection state
- Scale horizontally with multiple servers
- Message persistence with Redis Streams

---

## Phase 5: Advanced SignalR + Redis Patterns (Week 9-10)

### Theory
- Redis caching for SignalR state
- Presence management
- Session storage in Redis
- Rate limiting with Redis
- Advanced Pub/Sub patterns
- Performance monitoring

### Project 5: Real-Time Collaborative Dashboard
**Goal**: Build a collaborative analytics dashboard with live updates

**Features**:
- Real-time data visualization
- Multi-user collaboration
- User presence tracking with Redis
- Cursor position sharing
- Document locking mechanism
- Live comments and annotations
- Session management with Redis
- Rate limiting for API calls
- Real-time notifications

**NuGet Packages**:
```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.0" />
```

**Architecture Components**:
1. **Presence Service** (Redis Sets + TTL)
   - Track online users per dashboard
   - Heartbeat mechanism
   - Auto-cleanup disconnected users

2. **Lock Service** (Redis Distributed Locks)
   - Document/widget editing locks
   - RedLock algorithm implementation
   - Lock expiration and renewal

3. **Session Service** (Redis Hashes)
   - User session data
   - Distributed session storage
   - Session sharing across instances

4. **Rate Limiter** (Redis Sliding Window)
   - API rate limiting
   - Per-user quotas
   - Burst handling

5. **Notification Hub** (SignalR + Redis Backplane)
   - Real-time notifications
   - Targeted messaging
   - Broadcast updates

**Hub Methods**:
- `JoinDashboard(dashboardId)` - Join dashboard session
- `LeaveDashboard(dashboardId)` - Leave dashboard
- `UpdateCursorPosition(x, y)` - Share cursor position
- `LockWidget(widgetId)` - Lock for editing
- `UnlockWidget(widgetId)` - Release lock
- `UpdateWidget(widgetId, data)` - Update widget data
- `AddComment(widgetId, comment)` - Add comment
- `SendNotification(userId, message)` - Send notification

**API Endpoints**:
- GET `/api/dashboard/{id}/users` - Get active users (cached)
- POST `/api/dashboard/{id}/lock/{widgetId}` - Acquire lock
- DELETE `/api/dashboard/{id}/lock/{widgetId}` - Release lock
- GET `/api/dashboard/{id}/data` - Get dashboard data (cached)
- GET `/api/user/session` - Get session info
- GET `/api/ratelimit/status` - Check rate limit status

**Learning Outcomes**:
- Implement distributed locks with Redis
- Build presence system with TTL and Sets
- Create sliding window rate limiter
- Combine SignalR with Redis caching
- Handle concurrent operations
- Monitor and debug distributed systems
- Performance tuning for high concurrency

---

## Phase 6: Production-Ready Patterns (Week 11-12)

### Theory
- Redis Sentinel for high availability
- Redis Cluster architecture
- Monitoring and observability
- Security best practices
- Performance optimization
- Disaster recovery

### Project 6: Production-Ready Microservices Architecture
**Goal**: Build enterprise-grade system with Redis

**Features**:
- Multi-service architecture
- Shared session management
- Distributed tracing
- Health checks and monitoring
- Circuit breaker pattern
- Graceful degradation
- Redis Sentinel setup
- Backup and restore strategies

**NuGet Packages**:
```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.0" />
<PackageReference Include="Polly" Version="8.5.0" />
<PackageReference Include="OpenTelemetry" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.StackExchangeRedis" Version="1.0.0-rc9.15" />
```

**Architecture**:
```
├── Gateway Service (YARP/Ocelot)
├── Auth Service (JWT + Redis Session)
├── Notification Service (SignalR + Redis)
├── Analytics Service (Redis Streams)
├── Cache Service (Redis Cluster)
└── Monitoring Service (Prometheus + Grafana)
```

**Key Implementations**:
1. **Session Management**
   - JWT token caching
   - Session replication
   - Token revocation list

2. **Circuit Breaker**
   - Redis connection failure handling
   - Fallback strategies
   - Health monitoring

3. **Observability**
   - Redis metrics collection
   - Performance counters
   - Distributed tracing
   - Logging integration

4. **Security**
   - TLS/SSL connections
   - ACL configuration
   - Secret management
   - Network isolation

**Learning Outcomes**:
- Deploy Redis in production
- Configure high availability
- Implement monitoring and alerting
- Security hardening
- Performance tuning at scale
- Disaster recovery procedures

---

## 🛠️ Development Environment Setup

### Prerequisites
```bash
# Install Docker Desktop
https://www.docker.com/products/docker-desktop

# Install .NET 10 SDK
https://dotnet.microsoft.com/download

# Install Redis CLI tools
brew install redis (macOS)
choco install redis (Windows)
```

### Docker Compose Setup
```yaml
version: '3.8'
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes
  
  redis-commander:
    image: rediscommander/redis-commander
    ports:
      - "8081:8081"
    environment:
      - REDIS_HOSTS=local:redis:6379
    depends_on:
      - redis

volumes:
  redis-data:
```

---

## 📊 Progress Tracking

### Phase Completion Checklist

#### Phase 1: ☐
- [ ] Redis installed and running
- [ ] Basic CRUD API completed
- [ ] Understanding of connection management
- [ ] Serialization working correctly

#### Phase 2: ☐
- [ ] Leaderboard system functional
- [ ] Sorted Sets mastered
- [ ] Atomic operations implemented
- [ ] Performance tests passed

#### Phase 3: ☐
- [ ] Caching patterns implemented
- [ ] Cache invalidation working
- [ ] Hit/miss ratio monitoring
- [ ] Database integration complete

#### Phase 4: ☐
- [ ] SignalR chat application working
- [ ] Redis backplane configured
- [ ] Multiple instances tested
- [ ] Message history implemented

#### Phase 5: ☐
- [ ] Collaborative dashboard complete
- [ ] Presence system working
- [ ] Distributed locks functional
- [ ] Rate limiting implemented

#### Phase 6: ☐
- [ ] Production deployment ready
- [ ] High availability configured
- [ ] Monitoring dashboards setup
- [ ] Security audit completed

---

## 📚 Recommended Resources

### Documentation
- [StackExchange.Redis Documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Official Documentation](https://redis.io/docs/)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr/)
- [ASP.NET Core Caching](https://docs.microsoft.com/aspnet/core/performance/caching/)

### Books
- "Redis in Action" by Josiah L. Carlson
- "Redis Essentials" by Maxwell Dayvson Da Silva
- "Mastering Redis" by Jeremy Nelson

### Online Courses
- Redis University (free courses)
- Pluralsight: Redis Fundamentals
- Udemy: Redis with ASP.NET Core

### Tools
- RedisInsight - GUI for Redis
- Redis Commander - Web-based management
- Azure Cache for Redis - Managed Redis service
- AWS ElastiCache - Redis hosting

---

## 🎯 Key Concepts to Master

### Redis Concepts
- ✅ Connection multiplexing
- ✅ Pipelining and batching
- ✅ Transactions (MULTI/EXEC)
- ✅ Lua scripting
- ✅ Pub/Sub patterns
- ✅ Streams and consumer groups
- ✅ Persistence strategies
- ✅ Replication and clustering

### SignalR Concepts
- ✅ Hub architecture
- ✅ Connection lifecycle
- ✅ Groups and users
- ✅ Backplane configuration
- ✅ Scale-out strategies
- ✅ Authentication and authorization
- ✅ Protocol fallback
- ✅ Reconnection handling

### ASP.NET Core Integration
- ✅ IDistributedCache abstraction
- ✅ Dependency injection
- ✅ Configuration management
- ✅ Middleware patterns
- ✅ Health checks
- ✅ Background services
- ✅ Hosted services

---

## 💡 Pro Tips

1. **Start Simple**: Don't jump to SignalR immediately. Master basic Redis operations first.

2. **Use Docker**: Simplifies Redis setup and allows testing clustering scenarios.

3. **Monitor Everything**: Track cache hit rates, connection pools, and memory usage from day one.

4. **Test Failures**: Simulate Redis downtime to ensure graceful degradation.

5. **Key Naming**: Establish a clear key naming convention early (e.g., `app:entity:id`).

6. **Serialization**: Choose between JSON (readable) and MessagePack (performance) based on needs.

7. **Connection Pooling**: Always use ConnectionMultiplexer as a singleton.

8. **TTL Strategy**: Set appropriate TTLs to prevent memory bloat.

9. **Pub/Sub vs Streams**: Use Streams when you need message history and guaranteed delivery.

10. **Security First**: Always use passwords and TLS in production, even in internal networks.

---

## 🚀 Next Steps After Completion

1. **Contribute to Open Source**: Contribute to Redis or SignalR related projects
2. **Build Production System**: Deploy a real application using these patterns
3. **Explore Advanced Topics**: 
   - Redis Modules (RedisJSON, RedisSearch, RedisGraph)
   - Redis Functions (Redis 7+)
   - Redis Gears for programmability
4. **Cloud Certifications**: Azure Redis Cache or AWS ElastiCache certifications
5. **Architecture Design**: Design distributed systems using Redis patterns

---

## 📞 Getting Help

- Stack Overflow: [redis] and [signalr] tags
- Redis Discord Community
- ASP.NET Core GitHub Discussions
- Reddit: r/redis, r/dotnet

---

**Good luck on your Redis learning journey! 🎓**

*Remember: Build all projects, don't just read. Practical experience is key!*
