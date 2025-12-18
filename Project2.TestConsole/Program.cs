using StackExchange.Redis;
using System.Diagnostics;

Console.WriteLine("🎮 Leaderboard Performance Test\n");

// Connect to Redis
var redis = ConnectionMultiplexer.Connect("localhost:6379");
var db = redis.GetDatabase();

// Test configuration
const string leaderboardKey = "leaderboard:perf-test";
const int playerCount = 1000;
const int operationsCount = 10000;

Console.WriteLine($"Configuration:");
Console.WriteLine($"  - Players: {playerCount:N0}");
Console.WriteLine($"  - Operations: {operationsCount:N0}");
Console.WriteLine($"  - Redis: {redis.GetEndPoints()[0]}\n");

// Cleanup previous test data
Console.WriteLine("Cleaning up previous test data...");
await db.KeyDeleteAsync(leaderboardKey);

// Test 1: Bulk Score Insertion
Console.WriteLine("\n📊 Test 1: Bulk Score Insertion");
var sw = Stopwatch.StartNew();
var tasks = new List<Task>();

for (int i = 0; i < playerCount; i++)
{
    var playerId = $"player{i:D6}";
    var score = Random.Shared.Next(100, 10000);
    tasks.Add(db.SortedSetAddAsync(leaderboardKey, playerId, score));
}

await Task.WhenAll(tasks);
sw.Stop();

Console.WriteLine($"✅ Inserted {playerCount:N0} players");
Console.WriteLine($"   Time: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Rate: {playerCount / sw.Elapsed.TotalSeconds:N0} ops/sec");

// Test 2: Random Score Updates
Console.WriteLine("\n📊 Test 2: Random Score Updates");
sw.Restart();
tasks.Clear();

for (int i = 0; i < operationsCount; i++)
{
    var playerId = $"player{Random.Shared.Next(playerCount):D6}";
    var score = Random.Shared.Next(100, 10000);
    tasks.Add(db.SortedSetAddAsync(leaderboardKey, playerId, score));
}

await Task.WhenAll(tasks);
sw.Stop();

Console.WriteLine($"✅ Completed {operationsCount:N0} score updates");
Console.WriteLine($"   Time: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Rate: {operationsCount / sw.Elapsed.TotalSeconds:N0} ops/sec");

// Test 3: Top 100 Retrieval
Console.WriteLine("\n📊 Test 3: Top 100 Players Retrieval");
sw.Restart();
var retrievals = 1000;

for (int i = 0; i < retrievals; i++)
{
    await db.SortedSetRangeByRankWithScoresAsync(leaderboardKey, 0, 99, Order.Descending);
}

sw.Stop();
Console.WriteLine($"✅ Retrieved top 100 players {retrievals:N0} times");
Console.WriteLine($"   Time: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Avg: {sw.Elapsed.TotalMilliseconds / retrievals:F2}ms per query");
Console.WriteLine($"   Rate: {retrievals / sw.Elapsed.TotalSeconds:N0} ops/sec");

// Test 4: Rank Lookups
Console.WriteLine("\n📊 Test 4: Rank Lookups");
sw.Restart();
var rankLookups = 10000;

for (int i = 0; i < rankLookups; i++)
{
    var playerId = $"player{Random.Shared.Next(playerCount):D6}";
    await db.SortedSetRankAsync(leaderboardKey, playerId, Order.Descending);
}

sw.Stop();
Console.WriteLine($"✅ Completed {rankLookups:N0} rank lookups");
Console.WriteLine($"   Time: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Avg: {sw.Elapsed.TotalMilliseconds / rankLookups:F3}ms per query");
Console.WriteLine($"   Rate: {rankLookups / sw.Elapsed.TotalSeconds:N0} ops/sec");

// Test 5: Score Lookups
Console.WriteLine("\n📊 Test 5: Score Lookups");
sw.Restart();
var scoreLookups = 10000;

for (int i = 0; i < scoreLookups; i++)
{
    var playerId = $"player{Random.Shared.Next(playerCount):D6}";
    await db.SortedSetScoreAsync(leaderboardKey, playerId);
}

sw.Stop();
Console.WriteLine($"✅ Completed {scoreLookups:N0} score lookups");
Console.WriteLine($"   Time: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Avg: {sw.Elapsed.TotalMilliseconds / scoreLookups:F3}ms per query");
Console.WriteLine($"   Rate: {scoreLookups / sw.Elapsed.TotalSeconds:N0} ops/sec");

// Test 6: Mixed Workload (Read-Heavy)
Console.WriteLine("\n📊 Test 6: Mixed Workload (80% reads, 20% writes)");
sw.Restart();
var mixedOps = 10000;
tasks.Clear();

for (int i = 0; i < mixedOps; i++)
{
    if (Random.Shared.Next(100) < 80)
    {
        // Read operation (80%)
        var playerId = $"player{Random.Shared.Next(playerCount):D6}";
        tasks.Add(db.SortedSetRankAsync(leaderboardKey, playerId, Order.Descending));
    }
    else
    {
        // Write operation (20%)
        var playerId = $"player{Random.Shared.Next(playerCount):D6}";
        var score = Random.Shared.Next(100, 10000);
        tasks.Add(db.SortedSetAddAsync(leaderboardKey, playerId, score));
    }
}

await Task.WhenAll(tasks);
sw.Stop();

Console.WriteLine($"✅ Completed {mixedOps:N0} mixed operations");
Console.WriteLine($"   Time: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"   Rate: {mixedOps / sw.Elapsed.TotalSeconds:N0} ops/sec");

// Display Final Stats
Console.WriteLine("\n📈 Final Leaderboard Statistics");
var totalPlayers = await db.SortedSetLengthAsync(leaderboardKey);
var topPlayer = await db.SortedSetRangeByRankWithScoresAsync(leaderboardKey, 0, 0, Order.Descending);

Console.WriteLine($"   Total Players: {totalPlayers:N0}");
if (topPlayer.Length > 0)
{
    Console.WriteLine($"   Top Player: {topPlayer[0].Element}");
    Console.WriteLine($"   Top Score: {topPlayer[0].Score:N0}");
}

// Display Top 10
Console.WriteLine("\n🏆 Top 10 Players:");
var top10 = await db.SortedSetRangeByRankWithScoresAsync(leaderboardKey, 0, 9, Order.Descending);
for (int i = 0; i < top10.Length; i++)
{
    Console.WriteLine($"   #{i + 1}: {top10[i].Element} - {top10[i].Score:N0} points");
}

// Cleanup
Console.WriteLine("\n🧹 Cleaning up test data...");
await db.KeyDeleteAsync(leaderboardKey);

Console.WriteLine("\n✨ Performance test completed!");
redis.Close();

