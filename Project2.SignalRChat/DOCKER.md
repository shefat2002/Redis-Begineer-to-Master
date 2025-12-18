# Docker Compose Setup

## 🐳 What's Included

This Docker Compose configuration provides a complete Redis environment with monitoring:

### Services

1. **Redis** (port 6379)
   - Redis 7 Alpine (lightweight)
   - Persistent storage with volumes
   - Health checks enabled
   - AOF (Append Only File) persistence

2. **Redis Commander** (port 8081)
   - Web-based Redis management UI
   - Real-time monitoring
   - Key/value browser
   - Command execution

3. **SignalR App** (optional, commented out)
   - Can run app in Docker or locally
   - Default: Run locally for easier development

## 🚀 Quick Start

### Option 1: Run Everything in Docker

```bash
# Start Redis + Redis Commander
./docker-start.sh
```

This starts Redis and Redis Commander. The app runs locally:

```bash
# In another terminal
dotnet run
```

### Option 2: Include App in Docker

Uncomment the `signalr-app` service in `docker-compose.yml`, then:

```bash
docker-compose up --build
```

## 🌐 Access Points

| Service | URL | Description |
|---------|-----|-------------|
| SignalR Chat | http://localhost:5000 | Chat application |
| Redis Commander | http://localhost:8081 | Redis GUI |
| Redis Server | localhost:6379 | Direct Redis connection |

## 🔍 Redis Commander Features

### What You Can Do

1. **Browse Keys**
   - View all Redis keys
   - See SignalR channels in real-time
   - Example: `Microsoft.AspNetCore.SignalR.Redis:*`

2. **Monitor Commands**
   - Watch PUBLISH/SUBSCRIBE in real-time
   - See message flow between servers
   - Track connection activity

3. **Execute Commands**
   - Run Redis CLI commands from browser
   - Test PUBSUB manually
   - Check server info

4. **View Statistics**
   - Memory usage
   - Connected clients
   - Commands per second
   - Pub/Sub channels

### Using Redis Commander

1. **Start services**: `./docker-start.sh`
2. **Open Redis Commander**: http://localhost:8081
3. **Start the chat app**: `dotnet run` (in another terminal)
4. **Open chat**: http://localhost:5000
5. **Send messages** and watch Redis Commander update in real-time!

### What to Watch in Redis Commander

When you send a chat message, you'll see:

- **Keys tab**: SignalR internal keys appear
- **Pub/Sub tab**: Message broadcasts
- **CLI tab**: Execute `PUBSUB CHANNELS` to see active channels

## 📋 Docker Commands

### Start Services
```bash
# Start all services
docker-compose up -d

# Start and rebuild
docker-compose up -d --build

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f redis
docker-compose logs -f redis-commander
```

### Stop Services
```bash
# Stop services (keeps data)
docker-compose down

# Stop and remove volumes (deletes data)
docker-compose down -v

# Stop specific service
docker-compose stop redis
```

### Manage Services
```bash
# Restart a service
docker-compose restart redis

# View running containers
docker-compose ps

# Execute command in container
docker-compose exec redis redis-cli ping
```

## 🔧 Configuration

### Redis Configuration

Edit `docker-compose.yml` to customize Redis:

```yaml
redis:
  command: redis-server --maxmemory 256mb --maxmemory-policy allkeys-lru
```

### Redis Commander Configuration

Environment variables:

```yaml
redis-commander:
  environment:
    - REDIS_HOSTS=local:redis:6379
    - HTTP_USER=admin
    - HTTP_PASSWORD=secret
```

### Connect App to Docker Redis

The app automatically connects to `localhost:6379` when running locally.

For Docker app (if uncommented):
```yaml
environment:
  - Redis__ConnectionString=redis:6379
```

## 🧪 Testing with Docker

### Test 1: Single Instance
```bash
# Start Redis
docker-compose up -d redis redis-commander

# Run app locally
dotnet run

# Open browser
open http://localhost:5000
open http://localhost:8081
```

### Test 2: Multiple Instances (Scaling)
```bash
# Start Redis
docker-compose up -d redis redis-commander

# Terminal 1
dotnet run --urls "http://localhost:5000"

# Terminal 2
dotnet run --urls "http://localhost:5001"

# Terminal 3
dotnet run --urls "http://localhost:5002"

# Watch Redis Commander
# See messages distributed across instances!
```

### Test 3: Monitor Redis Activity

1. Open Redis Commander: http://localhost:8081
2. Go to "CLI" tab
3. Run: `MONITOR`
4. Send chat messages
5. Watch Redis commands in real-time!

Or use CLI directly:
```bash
docker-compose exec redis redis-cli monitor
```

## 📊 Redis Commander Screenshots Explained

### Keys Tab
Shows all keys in Redis:
- SignalR internal state keys
- Connection mappings
- Group memberships

### Pub/Sub Tab
Shows active channels:
```
Microsoft.AspNetCore.SignalR.Redis:SignalRChat:messages:all
Microsoft.AspNetCore.SignalR.Redis:SignalRChat:internal:*
```

### CLI Tab
Execute commands:
```bash
> PUBSUB CHANNELS
> PUBSUB NUMSUB channel_name
> CLIENT LIST
> INFO stats
```

### Configuration Tab
View Redis server configuration:
- Memory settings
- Persistence options
- Connection limits

## 🐛 Troubleshooting

### Port Already in Use

```bash
# Check what's using port 6379
lsof -i :6379

# Use different port
docker-compose stop redis
```

Edit `docker-compose.yml`:
```yaml
ports:
  - "6380:6379"
```

Update `appsettings.json`:
```json
{
  "Redis": {
    "ConnectionString": "localhost:6380"
  }
}
```

### Redis Commander Not Loading

```bash
# Check logs
docker-compose logs redis-commander

# Restart service
docker-compose restart redis-commander
```

### App Can't Connect to Redis

```bash
# Check Redis is running
docker-compose ps

# Test connection
docker-compose exec redis redis-cli ping

# Check app config in appsettings.json
# Should be: "localhost:6379" when running app locally
```

### Data Persistence

Redis data is stored in Docker volume `redis-data`.

```bash
# View volumes
docker volume ls

# Inspect volume
docker volume inspect project2signalrchat_redis-data

# Backup data
docker-compose exec redis redis-cli --rdb /data/backup.rdb

# Remove all data
docker-compose down -v
```

## 🎯 Production Considerations

### Use Redis with Password

```yaml
redis:
  command: redis-server --requirepass yourpassword
  
redis-commander:
  environment:
    - REDIS_HOSTS=local:redis:6379:0:yourpassword
```

App configuration:
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379,password=yourpassword"
  }
}
```

### Redis Cluster (High Availability)

For production, use Redis Cluster or managed services:

```yaml
services:
  redis-1:
    image: redis:7-alpine
    command: redis-server --cluster-enabled yes
    
  redis-2:
    image: redis:7-alpine
    command: redis-server --cluster-enabled yes
    
  redis-3:
    image: redis:7-alpine
    command: redis-server --cluster-enabled yes
```

### Monitoring in Production

Consider:
- **Redis Insights** - RedisLabs' official GUI
- **Prometheus + Grafana** - Metrics and dashboards
- **Redis Enterprise** - Managed Redis with monitoring

## 📚 Next Steps

1. ✅ Start services: `./docker-start.sh`
2. ✅ Open Redis Commander: http://localhost:8081
3. ✅ Run app: `dotnet run`
4. ✅ Send messages and watch Redis in action!
5. ✅ Try scaling with multiple app instances
6. ✅ Experiment with Redis commands in Commander

## 🔗 Resources

- [Redis Commander GitHub](https://github.com/joeferner/redis-commander)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Redis Docker Image](https://hub.docker.com/_/redis)
- [Redis Commands Reference](https://redis.io/commands/)

---

**Happy Monitoring! 🚀**

The Redis Commander makes it easy to visualize what's happening in Redis as your SignalR messages flow through the backplane!
