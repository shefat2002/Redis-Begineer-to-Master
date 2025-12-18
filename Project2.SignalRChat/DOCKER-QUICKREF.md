# 🐳 Docker Quick Reference

## One-Command Start

```bash
./docker-start.sh
```

This starts:
- ✅ Redis on port 6379
- ✅ Redis Commander on port 8081

Then run the app:
```bash
dotnet run
```

## Access URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Chat App | http://localhost:5000 | SignalR chat interface |
| Redis Commander | http://localhost:8081 | Redis monitoring GUI |
| Redis | localhost:6379 | Redis server |

## Common Commands

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Restart Redis
docker-compose restart redis

# Check status
docker-compose ps

# Access Redis CLI
docker-compose exec redis redis-cli
```

## Redis Commander Tips

1. **Monitor Real-Time Activity**
   - Go to CLI tab
   - Run: `MONITOR`
   - Send chat messages
   - Watch Redis commands appear!

2. **View Active Channels**
   - CLI tab → `PUBSUB CHANNELS`
   - See: `Microsoft.AspNetCore.SignalR.Redis:*`

3. **Check Connections**
   - CLI tab → `CLIENT LIST`
   - See all connected SignalR servers

4. **View Stats**
   - CLI tab → `INFO stats`
   - Memory usage, commands/sec, etc.

## Testing Workflow

```bash
# Terminal 1: Start Redis
./docker-start.sh

# Terminal 2: Start App Instance 1
dotnet run --urls "http://localhost:5000"

# Terminal 3: Start App Instance 2
dotnet run --urls "http://localhost:5001"

# Browser 1: Chat
http://localhost:5000

# Browser 2: Chat
http://localhost:5001

# Browser 3: Monitor Redis
http://localhost:8081
```

Send message from Browser 1 → Appears in Browser 2 → Watch in Redis Commander!

## Troubleshooting

**Redis won't start?**
```bash
# Check if port is in use
lsof -i :6379

# View logs
docker-compose logs redis
```

**Can't access Redis Commander?**
```bash
# Check if running
docker-compose ps

# Restart
docker-compose restart redis-commander
```

**App can't connect?**
- Verify Redis is running: `docker-compose ps`
- Test connection: `docker-compose exec redis redis-cli ping`
- Should return: `PONG`

## Clean Up

```bash
# Stop and keep data
docker-compose down

# Stop and remove all data
docker-compose down -v
```

---

For more details, see [DOCKER.md](DOCKER.md)
