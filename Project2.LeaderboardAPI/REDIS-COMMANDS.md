# Project 2: Redis Commands Quick Reference

## 🎮 Sorted Sets (ZSET) - Leaderboard Operations

### Add/Update Score
```bash
ZADD leaderboard:global 1500 player1
# Returns: 1 if new, 0 if updated
# O(log N) complexity

# Add multiple players
ZADD leaderboard:global 2000 player2 1800 player3 2500 player4
```

### Get Top Players (Descending)
```bash
# Top 10 players (highest scores first)
ZREVRANGE leaderboard:global 0 9 WITHSCORES
# Returns: player4 2500 player2 2000 player3 1800 player1 1500

# Top 5
ZREVRANGE leaderboard:global 0 4 WITHSCORES
```

### Get Player Rank
```bash
# Get rank (0-based, 0 = highest score)
ZREVRANK leaderboard:global player1
# Returns: 3 (4th place)

# Get rank (ascending order)
ZRANK leaderboard:global player1
```

### Get Player Score
```bash
ZSCORE leaderboard:global player1
# Returns: 1500
```

### Get Players in Rank Range
```bash
# Get ranks 10-20 (0-based indices 9-19)
ZREVRANGE leaderboard:global 9 19 WITHSCORES
```

### Count Total Players
```bash
ZCARD leaderboard:global
# Returns: 4
```

### Get Score Range
```bash
# Get players with scores between 1500-2000
ZRANGEBYSCORE leaderboard:global 1500 2000 WITHSCORES

# Descending order
ZREVRANGEBYSCORE leaderboard:global 2000 1500 WITHSCORES
```

### Remove Player
```bash
ZREM leaderboard:global player1
# Returns: 1 if removed, 0 if not found
```

### Increment Score
```bash
# Add 100 points to player's score
ZINCRBY leaderboard:global 100 player1
# Returns: new score
```

### Get Count in Range
```bash
# Count players with scores between 1500-2000
ZCOUNT leaderboard:global 1500 2000
```

## 📊 Hashes - Player Statistics

### Set Player Stats
```bash
# Set individual field
HSET player:stats:player1 totalGames 10
HSET player:stats:player1 highestScore 1800
HSET player:stats:player1 averageScore 1400.5

# Set multiple fields at once
HSET player:stats:player1 totalGames 10 highestScore 1800 averageScore 1400.5
```

### Get Player Stats
```bash
# Get single field
HGET player:stats:player1 totalGames
# Returns: 10

# Get all fields
HGETALL player:stats:player1
# Returns: totalGames 10 highestScore 1800 averageScore 1400.5

# Get multiple fields
HMGET player:stats:player1 totalGames highestScore
```

### Increment Counter
```bash
# Increment games played by 1
HINCRBY player:stats:player1 totalGames 1

# Increment with float
HINCRBYFLOAT player:stats:player1 averageScore 10.5
```

### Check if Field Exists
```bash
HEXISTS player:stats:player1 totalGames
# Returns: 1 if exists, 0 if not
```

### Delete Field
```bash
HDEL player:stats:player1 temporaryField
```

### Get All Field Names
```bash
HKEYS player:stats:player1
# Returns: totalGames highestScore averageScore
```

### Get All Values
```bash
HVALS player:stats:player1
# Returns: 10 1800 1400.5
```

### Count Fields
```bash
HLEN player:stats:player1
# Returns: 3
```

## 🔄 Transactions

### Basic Transaction
```bash
MULTI
ZADD leaderboard:global 1600 player1
HSET player:stats:player1 currentScore 1600
HINCRBY player:stats:player1 totalGames 1
EXEC
# All commands execute atomically
```

### Transaction with Watch (Optimistic Locking)
```bash
WATCH leaderboard:global
# ... read current value ...
MULTI
ZADD leaderboard:global 1700 player1
EXEC
# Executes only if key wasn't modified
```

### Discard Transaction
```bash
MULTI
ZADD leaderboard:global 1500 player1
DISCARD
# Cancel transaction
```

## 🔍 Monitoring & Debugging

### Monitor All Commands
```bash
redis-cli monitor
# Shows all commands in real-time
```

### View Key Information
```bash
# Check key type
TYPE leaderboard:global
# Returns: zset

# Check key TTL
TTL leaderboard:global
# Returns: -1 (no expiry)

# Check memory usage
MEMORY USAGE leaderboard:global
```

### Scan Keys
```bash
# Find all player stat keys
SCAN 0 MATCH player:stats:* COUNT 100

# Find all leaderboard keys
KEYS leaderboard:*
# ⚠️ Don't use KEYS in production (blocks server)
```

## 📈 Performance Testing

### Benchmark Commands
```bash
# Test ZADD performance
redis-benchmark -t zadd -n 100000 -q

# Test ZRANGE performance
redis-benchmark -t zrange -n 100000 -q

# Test HSET performance
redis-benchmark -t hset -n 100000 -q
```

### Check Info
```bash
# Server stats
INFO stats

# Memory info
INFO memory

# CPU info
INFO cpu

# All info
INFO all
```

## 🧹 Maintenance

### Clear Test Data
```bash
# Delete leaderboard
DEL leaderboard:global

# Delete all player stats (use with caution!)
redis-cli --scan --pattern "player:stats:*" | xargs redis-cli DEL

# Flush database (deletes everything!)
FLUSHDB

# Flush all databases
FLUSHALL
```

### Backup & Restore
```bash
# Force save to disk
SAVE
# or background save
BGSAVE

# Check last save time
LASTSAVE
```

## 🎯 Common Patterns

### Daily Leaderboard Reset
```bash
# Copy current to daily archive
ZUNIONSTORE leaderboard:daily:2024-01-15 1 leaderboard:global

# Clear current
DEL leaderboard:global

# Set expiry on archive (30 days)
EXPIRE leaderboard:daily:2024-01-15 2592000
```

### Get Player's Neighbors in Rank
```bash
# Get player rank
ZREVRANK leaderboard:global player1
# Returns: 5

# Get players ranked 4-6 (neighbors)
ZREVRANGE leaderboard:global 4 6 WITHSCORES
```

### Batch Score Update
```bash
MULTI
ZADD leaderboard:global 1600 player1
ZADD leaderboard:global 2100 player2
ZADD leaderboard:global 1900 player3
EXEC
```

### Get Top Player
```bash
ZREVRANGE leaderboard:global 0 0 WITHSCORES
# Returns top player and score
```

### Check if Player Exists
```bash
ZSCORE leaderboard:global player1
# Returns score if exists, nil if not
```

## 📊 Time Complexity Reference

| Operation | Command | Complexity |
|-----------|---------|------------|
| Add Score | ZADD | O(log N) |
| Get Top N | ZREVRANGE | O(log N + M) |
| Get Rank | ZREVRANK | O(log N) |
| Get Score | ZSCORE | O(1) |
| Count | ZCARD | O(1) |
| Remove | ZREM | O(log N) |
| Set Hash Field | HSET | O(1) |
| Get Hash Field | HGET | O(1) |
| Get All Fields | HGETALL | O(N) |
| Increment | HINCRBY | O(1) |

## 🔗 Useful Links

- [Redis Sorted Sets Docs](https://redis.io/docs/data-types/sorted-sets/)
- [Redis Hashes Docs](https://redis.io/docs/data-types/hashes/)
- [Redis Transactions](https://redis.io/docs/manual/transactions/)
- [Redis Commands Reference](https://redis.io/commands/)

---

**Quick Copy-Paste Examples**

```bash
# Setup test data
ZADD leaderboard:global 2500 alice 2300 bob 1800 charlie 1500 david
HSET player:stats:alice totalGames 25 highestScore 2600 averageScore 2400

# View top 10
ZREVRANGE leaderboard:global 0 9 WITHSCORES

# Get alice's rank
ZREVRANK leaderboard:global alice

# Get alice's stats
HGETALL player:stats:alice

# Update alice's score
ZADD leaderboard:global 2700 alice

# Monitor commands
redis-cli monitor

# Clean up
DEL leaderboard:global
redis-cli --scan --pattern "player:stats:*" | xargs redis-cli DEL
```
