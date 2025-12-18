# Project 2 Implementation Guide: Real-Time Leaderboard System

## 📋 Overview

This guide walks through the implementation of a high-performance gaming leaderboard API using Redis Sorted Sets. This is **Phase 2** of the Redis learning plan.

## 🎯 Learning Objectives

By the end of this project, you will master:
- Redis Sorted Sets (ZADD, ZRANGE, ZRANK, ZSCORE)
- Redis Hashes for metadata storage
- Atomic operations and transactions (MULTI/EXEC)
- Efficient ranking algorithms
- ASP.NET Core Web API development
- Performance optimization techniques

## 🏗️ Architecture Overview

```
┌─────────────┐      HTTP      ┌──────────────────────┐
│   Client    │ ────────────► │  LeaderboardAPI      │
│  (Browser,  │                │  (ASP.NET Core)      │
│   Mobile)   │ ◄──────────── │                      │
└─────────────┘      JSON      └──────────┬───────────┘
                                           │
                                           │ StackExchange.Redis
                                           │ (Connection Pool)
                                           ▼
                                ┌──────────────────────┐
                                │      Redis Server     │
                                │  ┌─────────────────┐ │
                                │  │ Sorted Set      │ │  ← Leaderboard
                                │  │ (ZSET)          │ │
                                │  └─────────────────┘ │
                                │  ┌─────────────────┐ │
                                │  │ Hashes          │ │  ← Player Stats
                                │  │ (HASH)          │ │
                                │  └─────────────────┘ │
                                └──────────────────────┘
```

## 📦 Project Structure

### Created Files

```
Project2.LeaderboardAPI/
├── Controllers/
│   └── LeaderboardController.cs     # REST API endpoints
├── Models/
│   └── PlayerScore.cs                # DTOs and models
├── Services/
│   ├── ILeaderboardService.cs        # Service contract
│   └── LeaderboardService.cs         # Redis operations
├── Program.cs                         # App configuration
├── appsettings.json                  # Configuration
├── README.md                         # Project documentation
├── test-api.sh                       # Testing script
└── run-leaderboard.sh                # Quick start script

Project2.TestConsole/
└── Program.cs                         # Performance testing
```

## 🔧 Implementation Steps

### Step 1: Project Setup

```bash
# Create the Web API project
dotnet new webapi -n Project2.LeaderboardAPI

# Add Redis client library
dotnet add package StackExchange.Redis
```

### Step 2: Data Models (Models/PlayerScore.cs)

**Purpose**: Define the data structures for API requests and responses.

```csharp
// Score submission from client
public class ScoreSubmission
{
    public string PlayerId { get; set; }
    public double Score { get; set; }
}

// Leaderboard entry
public class PlayerScore
{
    public string PlayerId { get; set; }
    public double Score { get; set; }
    public long Rank { get; set; }
}

// Detailed player statistics
public class PlayerStats
{
    public string PlayerId { get; set; }
    public double CurrentScore { get; set; }
    public long Rank { get; set; }
    public int TotalGames { get; set; }
    public double HighestScore { get; set; }
    public double AverageScore { get; set; }
    public DateTime LastPlayed { get; set; }
}
```

### Step 3: Service Interface (Services/ILeaderboardService.cs)

**Purpose**: Define the contract for leaderboard operations.

```csharp
public interface ILeaderboardService
{
    Task<bool> SubmitScoreAsync(string playerId, double score);
    Task<List<PlayerScore>> GetTopPlayersAsync(int count);
    Task<long> GetPlayerRankAsync(string playerId);
    Task<PlayerStats?> GetPlayerStatsAsync(string playerId);
    Task<List<PlayerScore>> GetPlayerRangeAsync(int start, int end);
    Task<double?> GetPlayerScoreAsync(string playerId);
    Task<long> GetTotalPlayersAsync();
}
```

### Step 4: Redis Service Implementation (Services/LeaderboardService.cs)

**Purpose**: Implement leaderboard logic using Redis Sorted Sets.

#### Key Method 1: Submit Score

```csharp
public async Task<bool> SubmitScoreAsync(string playerId, double score)
{
    // Create a transaction for atomicity
    var tran = _db.CreateTransaction();
    
    // 1. Update leaderboard (Sorted Set)
    var scoreTask = tran.SortedSetAddAsync(LeaderboardKey, playerId, score);
    
    // 2. Update player statistics (Hash)
    var statsKey = PlayerStatsPrefix + playerId;
    tran.HashIncrementAsync(statsKey, "totalGames", 1);
    tran.HashSetAsync(statsKey, "lastPlayed", DateTime.UtcNow.Ticks);
    tran.HashSetAsync(statsKey, "currentScore", score);
    
    // 3. Update highest score if needed
    var currentHighest = await _db.HashGetAsync(statsKey, "highestScore");
    if (!currentHighest.HasValue || score > (double)currentHighest)
    {
        tran.HashSetAsync(statsKey, "highestScore", score);
    }
    
    // 4. Calculate and update average
    var totalGames = await _db.HashGetAsync(statsKey, "totalGames");
    var totalScore = await _db.HashGetAsync(statsKey, "totalScore");
    var newTotalScore = (totalScore.HasValue ? (double)totalScore : 0) + score;
    var games = (totalGames.HasValue ? (int)totalGames : 0) + 1;
    
    tran.HashSetAsync(statsKey, "totalScore", newTotalScore);
    tran.HashSetAsync(statsKey, "averageScore", newTotalScore / games);
    
    // Execute all operations atomically
    var executed = await tran.ExecuteAsync();
    return executed && await scoreTask;
}
```

**Why this works:**
- ✅ All operations succeed or fail together (atomicity)
- ✅ No race conditions even with concurrent updates
- ✅ Efficient - single round-trip to Redis

#### Key Method 2: Get Top Players

```csharp
public async Task<List<PlayerScore>> GetTopPlayersAsync(int count)
{
    // Get top N from sorted set (descending order)
    var topPlayers = await _db.SortedSetRangeByRankWithScoresAsync(
        LeaderboardKey,
        start: 0,
        stop: count - 1,
        order: Order.Descending  // Highest scores first
    );
    
    // Map to response model
    var result = new List<PlayerScore>();
    for (int i = 0; i < topPlayers.Length; i++)
    {
        result.Add(new PlayerScore
        {
            PlayerId = topPlayers[i].Element.ToString(),
            Score = topPlayers[i].Score,
            Rank = i + 1  // Convert 0-based to 1-based
        });
    }
    
    return result;
}
```

**Why this works:**
- ✅ O(log N + M) complexity where M is result size
- ✅ Redis maintains sorted order automatically
- ✅ No need to sort in application code

#### Key Method 3: Get Player Rank

```csharp
public async Task<long> GetPlayerRankAsync(string playerId)
{
    // Binary search in sorted set
    var rank = await _db.SortedSetRankAsync(
        LeaderboardKey, 
        playerId, 
        Order.Descending
    );
    
    // Convert 0-based to 1-based, return -1 if not found
    return rank.HasValue ? rank.Value + 1 : -1;
}
```

**Why this works:**
- ✅ O(log N) lookup time
- ✅ Redis uses skip list + hash table internally
- ✅ Extremely fast even with millions of players

### Step 5: Controller Implementation (Controllers/LeaderboardController.cs)

**Purpose**: Expose HTTP endpoints for the leaderboard.

```csharp
[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly ILogger<LeaderboardController> _logger;
    
    // POST /api/leaderboard/score
    [HttpPost("score")]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreSubmission submission)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(submission.PlayerId))
            return BadRequest("PlayerId is required");
            
        if (submission.Score < 0)
            return BadRequest("Score must be non-negative");
        
        // Submit score
        var success = await _leaderboardService.SubmitScoreAsync(
            submission.PlayerId, 
            submission.Score
        );
        
        if (success)
        {
            var rank = await _leaderboardService.GetPlayerRankAsync(submission.PlayerId);
            return Ok(new { 
                message = "Score submitted successfully",
                rank 
            });
        }
        
        return StatusCode(500, "Failed to submit score");
    }
    
    // GET /api/leaderboard/top/10
    [HttpGet("top/{count}")]
    public async Task<ActionResult<List<PlayerScore>>> GetTopPlayers(int count = 10)
    {
        if (count <= 0 || count > 100)
            return BadRequest("Count must be between 1 and 100");
            
        return Ok(await _leaderboardService.GetTopPlayersAsync(count));
    }
    
    // Additional endpoints...
}
```

### Step 6: Program Configuration (Program.cs)

**Purpose**: Set up Redis connection and dependency injection.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure Redis connection (singleton pattern)
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(redisConnection);
    config.AbortOnConnectFail = false;  // Resilient to Redis restarts
    return ConnectionMultiplexer.Connect(config);
});

// Register services
builder.Services.AddSingleton<ILeaderboardService, LeaderboardService>();
builder.Services.AddControllers();

var app = builder.Build();

// Test Redis connection on startup
var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
await redis.GetDatabase().PingAsync();
app.Logger.LogInformation("✅ Connected to Redis");

app.MapControllers();
app.Run();
```

**Key Points:**
- ✅ `IConnectionMultiplexer` is a singleton (one connection pool for entire app)
- ✅ Connection is thread-safe and handles multiplexing internally
- ✅ `AbortOnConnectFail = false` allows app to start even if Redis is temporarily down

## 🧪 Testing the Implementation

### Manual Testing with curl

```bash
# 1. Submit scores
curl -X POST http://localhost:5000/api/leaderboard/score \
  -H "Content-Type: application/json" \
  -d '{"playerId":"alice","score":1500}'

curl -X POST http://localhost:5000/api/leaderboard/score \
  -H "Content-Type: application/json" \
  -d '{"playerId":"bob","score":2000}'

# 2. Get top 10 players
curl http://localhost:5000/api/leaderboard/top/10

# 3. Get player rank
curl http://localhost:5000/api/leaderboard/rank/alice

# 4. Get player stats
curl http://localhost:5000/api/leaderboard/player/alice
```

### Automated Testing

```bash
# Run the test script
./test-api.sh
```

### Verify in Redis CLI

```bash
redis-cli

# View leaderboard
ZREVRANGE leaderboard:global 0 9 WITHSCORES

# View player stats
HGETALL player:stats:alice

# Get specific rank
ZREVRANK leaderboard:global alice

# Count total players
ZCARD leaderboard:global
```

## 📊 Understanding Redis Data Structures

### Sorted Set (Leaderboard)

**Internal Structure:**
- Skip list for O(log N) operations
- Hash table for O(1) member lookups
- Automatically maintains sort order

**Commands:**
```bash
ZADD leaderboard:global 1500 alice      # Add/update score
ZREVRANGE leaderboard:global 0 9        # Top 10 (high to low)
ZREVRANK leaderboard:global alice       # Get rank
ZSCORE leaderboard:global alice         # Get score
ZCARD leaderboard:global                # Count members
```

### Hash (Player Statistics)

**Internal Structure:**
- Hash table with field-value pairs
- O(1) access to individual fields
- Compact memory representation

**Commands:**
```bash
HSET player:stats:alice totalGames 10   # Set field
HGET player:stats:alice totalGames      # Get field
HGETALL player:stats:alice              # Get all fields
HINCRBY player:stats:alice totalGames 1 # Atomic increment
```

## 🚀 Performance Optimization Tips

### 1. Connection Pooling
```csharp
// ✅ GOOD: Singleton, reused across requests
builder.Services.AddSingleton<IConnectionMultiplexer>(...)

// ❌ BAD: Creating new connection per request
builder.Services.AddScoped<IConnectionMultiplexer>(...)
```

### 2. Async Operations
```csharp
// ✅ GOOD: Non-blocking I/O
await _db.SortedSetAddAsync(...)

// ❌ BAD: Blocking thread
_db.SortedSetAdd(...)  // Avoid sync methods
```

### 3. Batch Operations
```csharp
// ✅ GOOD: Single round-trip
var tasks = players.Select(p => _db.SortedSetAddAsync(key, p.Id, p.Score));
await Task.WhenAll(tasks);

// ❌ BAD: N round-trips
foreach (var player in players)
    await _db.SortedSetAddAsync(key, player.Id, player.Score);
```

### 4. Transactions for Atomicity
```csharp
// ✅ GOOD: All-or-nothing updates
var tran = _db.CreateTransaction();
tran.SortedSetAddAsync(...);
tran.HashSetAsync(...);
await tran.ExecuteAsync();
```

## 🎓 Key Concepts Explained

### Why Sorted Sets for Leaderboards?

1. **Automatic Sorting**: Redis maintains sort order on every update
2. **Efficient Ranking**: O(log N) for rank lookups
3. **Range Queries**: Get top N or rank range efficiently
4. **Score Updates**: Updating a score automatically repositions the member
5. **Uniqueness**: Each player appears only once

### Redis Transaction Guarantees

- **Atomic**: All commands execute together or not at all
- **Isolated**: No other client can see partial updates
- **Consistent**: Data remains valid before and after
- **Not Rolled Back**: Failed transactions don't undo previous commands

### When to Use Hashes vs. Strings

**Use Hashes when:**
- ✅ Multiple related fields per entity (player stats)
- ✅ Need to update individual fields
- ✅ Want memory efficiency

**Use Strings when:**
- ✅ Single value per key (simple counters)
- ✅ Need TTL per value
- ✅ Value is opaque (serialized JSON)

## 🐛 Common Issues and Solutions

### Issue: Rank is always -1

**Cause**: Player not in sorted set  
**Solution**: Ensure `ZADD` succeeded before calling `ZRANK`

### Issue: Scores not updating

**Cause**: Transaction not executing  
**Solution**: Check return value of `ExecuteAsync()`

### Issue: High memory usage

**Cause**: Too many player stat fields  
**Solution**: Use TTL on stat keys, or archive old data

### Issue: Slow rank lookups

**Cause**: Millions of players in single sorted set  
**Solution**: Shard leaderboards (per region, per time period)

## 📈 Scaling Strategies

### Vertical Scaling
- Increase Redis memory (64GB+)
- Use Redis persistence (AOF)
- Enable compression

### Horizontal Scaling
- **Multiple Leaderboards**: Per region, game mode, time period
- **Redis Cluster**: Shard data across nodes
- **Read Replicas**: Separate read and write paths

### Time-Based Leaderboards
```csharp
// Daily leaderboard
var dailyKey = $"leaderboard:daily:{DateTime.UtcNow:yyyy-MM-dd}";

// Weekly leaderboard
var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(...);
var weeklyKey = $"leaderboard:weekly:{year}:{week}";
```

## ✅ Success Criteria

Your implementation is complete when:

- ✅ Build succeeds without errors
- ✅ All endpoints return expected data
- ✅ Scores update correctly in Redis
- ✅ Ranks calculate accurately
- ✅ Player stats track properly
- ✅ Performance tests show good throughput
- ✅ Redis commands visible in monitor

## 🎉 Next Steps

After mastering this project:

1. **Extend**: Add time-based leaderboards, friends rankings
2. **Optimize**: Implement caching, add monitoring
3. **Scale**: Test with millions of players
4. **Integrate**: Add WebSocket updates for real-time ranks
5. **Deploy**: Containerize and deploy to cloud

## 📚 Additional Learning Resources

- [Redis Sorted Sets Deep Dive](https://redis.io/docs/data-types/sorted-sets/)
- [Redis Transactions Tutorial](https://redis.io/docs/manual/transactions/)
- [StackExchange.Redis Basics](https://stackexchange.github.io/StackExchange.Redis/)
- [Leaderboard Patterns](https://redis.com/redis-best-practices/indexing-patterns/leaderboards/)

---

**Congratulations!** 🎉 You've implemented a production-grade leaderboard system!

**Time to Complete**: 1-2 hours  
**Difficulty**: Intermediate  
**Redis Concepts**: Sorted Sets, Hashes, Transactions  
**Next Phase**: Phase 3 - Caching Strategies
