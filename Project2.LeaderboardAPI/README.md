# Project 2: Real-Time Leaderboard System

A high-performance gaming leaderboard API built with ASP.NET Core and Redis Sorted Sets.

## 🎯 Features

- **Score Submission**: Submit player scores with atomic operations
- **Top N Players**: Retrieve top players in real-time
- **Rank Calculation**: Get any player's current rank
- **Player Statistics**: Track games played, highest score, average score
- **Range Queries**: Get players in specific rank ranges
- **Atomic Operations**: Thread-safe score updates using Redis transactions

## 🛠️ Technologies

- **ASP.NET Core 10.0**: Web API framework
- **Redis**: In-memory data store for leaderboard
- **StackExchange.Redis**: Redis client library
- **Sorted Sets**: Redis data structure for efficient ranking

## 📁 Project Structure

```
Project2.LeaderboardAPI/
├── Controllers/
│   └── LeaderboardController.cs    # API endpoints
├── Models/
│   └── PlayerScore.cs               # Data models
├── Services/
│   ├── ILeaderboardService.cs       # Service interface
│   └── LeaderboardService.cs        # Redis operations
├── Program.cs                        # App configuration
├── appsettings.json                 # Configuration
└── README.md                        # This file
```

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 SDK
- Redis Server (running on localhost:6379)

### Installation

1. **Start Redis** (if not already running):
```bash
# Using Docker
docker run -d -p 6379:6379 redis:7-alpine

# Or using local Redis
redis-server
```

2. **Run the application**:
```bash
cd Project2.LeaderboardAPI
dotnet run
```

The API will start at `http://localhost:5000` (or `https://localhost:5001`)

## 📡 API Endpoints

### 1. Submit Score
```http
POST /api/leaderboard/score
Content-Type: application/json

{
  "playerId": "player123",
  "score": 1500
}
```

**Response:**
```json
{
  "message": "Score submitted successfully",
  "playerId": "player123",
  "score": 1500,
  "rank": 5
}
```

### 2. Get Top Players
```http
GET /api/leaderboard/top/10
```

**Response:**
```json
[
  {
    "playerId": "player456",
    "score": 2500,
    "rank": 1
  },
  {
    "playerId": "player789",
    "score": 2300,
    "rank": 2
  }
]
```

### 3. Get Player Rank
```http
GET /api/leaderboard/rank/player123
```

**Response:**
```json
{
  "playerId": "player123",
  "rank": 5,
  "score": 1500,
  "totalPlayers": 100,
  "percentile": 95.0
}
```

### 4. Get Player Statistics
```http
GET /api/leaderboard/player/player123
```

**Response:**
```json
{
  "playerId": "player123",
  "currentScore": 1500,
  "rank": 5,
  "totalGames": 25,
  "highestScore": 1800,
  "averageScore": 1350.5,
  "lastPlayed": "2024-01-15T10:30:00Z"
}
```

### 5. Get Rank Range
```http
GET /api/leaderboard/range/10/20
```

**Response:**
```json
[
  {
    "playerId": "player101",
    "score": 1200,
    "rank": 10
  },
  ...
]
```

### 6. Get Leaderboard Statistics
```http
GET /api/leaderboard/stats
```

**Response:**
```json
{
  "totalPlayers": 1000,
  "topPlayer": "player456",
  "topScore": 2500
}
```

## 🔧 Redis Data Structures

### Sorted Set (Leaderboard)
```
Key: "leaderboard:global"
Type: Sorted Set (ZSET)
Members: Player IDs
Scores: Player scores

Example:
ZADD leaderboard:global 1500 "player123"
ZREVRANK leaderboard:global "player123"  # Get rank
ZREVRANGE leaderboard:global 0 9 WITHSCORES  # Top 10
```

### Hash (Player Statistics)
```
Key: "player:stats:{playerId}"
Type: Hash
Fields:
  - totalGames: Number of games played
  - totalScore: Sum of all scores
  - highestScore: Best score ever
  - averageScore: Average score
  - currentScore: Latest score
  - lastPlayed: Timestamp

Example:
HSET player:stats:player123 totalGames 25
HGET player:stats:player123 highestScore
HGETALL player:stats:player123
```

## 💡 Key Concepts

### 1. Redis Sorted Sets
- Members are unique (one player = one entry)
- Scores can be updated (ZADD is idempotent)
- O(log N) operations for add/update/rank
- Range queries in O(log N + M) where M is result size

### 2. Atomic Transactions
```csharp
var tran = _db.CreateTransaction();
tran.SortedSetAddAsync(leaderboardKey, playerId, score);
tran.HashIncrementAsync(statsKey, "totalGames", 1);
await tran.ExecuteAsync();
```

### 3. Rank Calculation
- Redis ranks are 0-based
- `ZREVRANK` for descending order (highest score = rank 0)
- Add 1 to convert to human-readable ranks

### 4. Performance Optimization
- Connection pooling via `IConnectionMultiplexer` (singleton)
- Async operations throughout
- Batch operations where possible
- No N+1 query problems

## 🧪 Testing

### Using curl

```bash
# Submit scores
curl -X POST http://localhost:5000/api/leaderboard/score \
  -H "Content-Type: application/json" \
  -d '{"playerId":"alice","score":1500}'

curl -X POST http://localhost:5000/api/leaderboard/score \
  -H "Content-Type: application/json" \
  -d '{"playerId":"bob","score":2000}'

# Get top 10
curl http://localhost:5000/api/leaderboard/top/10

# Get player rank
curl http://localhost:5000/api/leaderboard/rank/alice

# Get player stats
curl http://localhost:5000/api/leaderboard/player/alice
```

### Using Redis CLI

Monitor Redis commands in real-time:
```bash
redis-cli monitor
```

Check leaderboard data:
```bash
redis-cli

# View all players and scores
ZREVRANGE leaderboard:global 0 -1 WITHSCORES

# Get specific player's score
ZSCORE leaderboard:global alice

# Get player's rank
ZREVRANK leaderboard:global alice

# Count total players
ZCARD leaderboard:global

# View player stats
HGETALL player:stats:alice
```

## 📊 Performance Characteristics

| Operation | Time Complexity | Notes |
|-----------|----------------|-------|
| Submit Score | O(log N) | Sorted set insertion |
| Get Top N | O(log N + N) | Range query |
| Get Rank | O(log N) | Binary search in sorted set |
| Get Stats | O(1) | Hash lookup |
| Range Query | O(log N + M) | M = result size |

**Expected Performance:**
- 10,000+ writes per second
- 50,000+ reads per second
- Sub-millisecond response times
- Linear scalability with Redis Cluster

## 🔒 Best Practices

1. **Connection Management**: Use singleton `IConnectionMultiplexer`
2. **Error Handling**: Graceful degradation if Redis is down
3. **Validation**: Input validation before Redis operations
4. **Logging**: Log score submissions and rank changes
5. **Monitoring**: Track Redis connection health

## 📈 Scaling Considerations

### Vertical Scaling
- Increase Redis memory
- Use Redis persistence (AOF/RDB)
- Enable compression for large datasets

### Horizontal Scaling
- Multiple leaderboards (per region, per game mode)
- Redis Cluster for sharding
- Read replicas for heavy read workloads

### Example: Multiple Leaderboards
```csharp
// Daily leaderboard
const string DailyKey = "leaderboard:daily:2024-01-15";

// Regional leaderboards
const string USKey = "leaderboard:region:us";
const string EUKey = "leaderboard:region:eu";

// Game mode leaderboards
const string CasualKey = "leaderboard:mode:casual";
const string RankedKey = "leaderboard:mode:ranked";
```

## 🚀 Production Deployment

### Configuration

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-server:6380,password=secret,ssl=True,abortConnect=False"
  }
}
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY publish/ .
ENV ConnectionStrings__Redis="redis:6379"
EXPOSE 80
ENTRYPOINT ["dotnet", "Project2.LeaderboardAPI.dll"]
```

### Health Checks

Add Redis health checks:
```csharp
builder.Services.AddHealthChecks()
    .AddRedis(redisConnection);

app.MapHealthChecks("/health");
```

## 📚 Learning Outcomes

After completing this project, you understand:

- ✅ Redis Sorted Sets for ranking systems
- ✅ Atomic transactions with MULTI/EXEC
- ✅ Hash data structures for metadata
- ✅ ConnectionMultiplexer pattern
- ✅ Async/await with Redis operations
- ✅ Time complexity and performance optimization
- ✅ Real-world leaderboard architecture

## 🎓 Extensions

Ideas to enhance the project:

1. **Time-based Leaderboards**: Daily/weekly/monthly rankings
2. **Multiple Leaderboards**: Per game mode, region, or level
3. **Score History**: Track score progression over time
4. **Achievements**: Award badges at score milestones
5. **Friends Leaderboard**: Compare with friends only
6. **Decay System**: Reduce scores over time for inactive players
7. **Anti-cheat**: Detect and flag suspicious score submissions
8. **Real-time Updates**: WebSocket/SignalR for live rank changes

## 🐛 Troubleshooting

**Redis connection failed:**
- Verify Redis is running: `redis-cli ping`
- Check connection string in appsettings.json
- Ensure firewall allows port 6379

**Scores not updating:**
- Check Redis logs: `redis-cli info`
- Monitor commands: `redis-cli monitor`
- Verify transaction execution

**Slow performance:**
- Check Redis memory usage: `redis-cli info memory`
- Enable Redis persistence for safety
- Consider Redis Cluster for scale

## 📖 References

- [Redis Sorted Sets Documentation](https://redis.io/docs/data-types/sorted-sets/)
- [StackExchange.Redis Guide](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Transactions](https://redis.io/docs/manual/transactions/)

---

**Status**: ✅ Complete and Production-Ready

**Complexity**: Intermediate

**Time to Complete**: 1-2 hours
