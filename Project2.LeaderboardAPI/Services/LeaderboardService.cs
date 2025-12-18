using Project2.LeaderboardAPI.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Project2.LeaderboardAPI.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IDatabase _db;
    private const string LeaderboardKey = "leaderboard:global";
    private const string PlayerStatsPrefix = "player:stats:";

    public LeaderboardService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> SubmitScoreAsync(string playerId, double score)
    {
        // Use a transaction to ensure atomicity
        var tran = _db.CreateTransaction();
        
        // Update score in sorted set (leaderboard)
        var scoreTask = tran.SortedSetAddAsync(LeaderboardKey, playerId, score);
        
        // Update player statistics
        var statsKey = PlayerStatsPrefix + playerId;
        var statsUpdateTasks = new List<Task>
        {
            tran.HashIncrementAsync(statsKey, "totalGames", 1),
            tran.HashSetAsync(statsKey, "lastPlayed", DateTime.UtcNow.Ticks),
            tran.HashSetAsync(statsKey, "currentScore", score)
        };

        // Get current highest score to compare
        var currentHighest = await _db.HashGetAsync(statsKey, "highestScore");
        if (!currentHighest.HasValue || score > (double)currentHighest)
        {
            statsUpdateTasks.Add(tran.HashSetAsync(statsKey, "highestScore", score));
        }

        // Calculate and update average score
        var totalGames = await _db.HashGetAsync(statsKey, "totalGames");
        var totalScore = await _db.HashGetAsync(statsKey, "totalScore");
        var newTotalScore = (totalScore.HasValue ? (double)totalScore : 0) + score;
        var games = (totalGames.HasValue ? (int)totalGames : 0) + 1;
        var avgScore = newTotalScore / games;
        
        statsUpdateTasks.Add(tran.HashSetAsync(statsKey, "totalScore", newTotalScore));
        statsUpdateTasks.Add(tran.HashSetAsync(statsKey, "averageScore", avgScore));

        var executed = await tran.ExecuteAsync();
        return executed && await scoreTask;
    }

    public async Task<List<PlayerScore>> GetTopPlayersAsync(int count)
    {
        // Get top N players from sorted set (descending order)
        var topPlayers = await _db.SortedSetRangeByRankWithScoresAsync(
            LeaderboardKey,
            start: 0,
            stop: count - 1,
            order: Order.Descending);

        var result = new List<PlayerScore>();
        for (int i = 0; i < topPlayers.Length; i++)
        {
            result.Add(new PlayerScore
            {
                PlayerId = topPlayers[i].Element.ToString(),
                Score = topPlayers[i].Score,
                Rank = i + 1
            });
        }

        return result;
    }

    public async Task<long> GetPlayerRankAsync(string playerId)
    {
        // Get rank from sorted set (0-based, lower rank = higher score)
        var rank = await _db.SortedSetRankAsync(LeaderboardKey, playerId, Order.Descending);
        return rank.HasValue ? rank.Value + 1 : -1; // Convert to 1-based, -1 if not found
    }

    public async Task<PlayerStats?> GetPlayerStatsAsync(string playerId)
    {
        var statsKey = PlayerStatsPrefix + playerId;
        var stats = await _db.HashGetAllAsync(statsKey);

        if (stats.Length == 0)
            return null;

        var statsDict = stats.ToDictionary(x => x.Name.ToString(), x => x.Value);
        var currentScore = await _db.SortedSetScoreAsync(LeaderboardKey, playerId);
        var rank = await GetPlayerRankAsync(playerId);

        return new PlayerStats
        {
            PlayerId = playerId,
            CurrentScore = currentScore ?? 0,
            Rank = rank,
            TotalGames = statsDict.ContainsKey("totalGames") ? (int)statsDict["totalGames"] : 0,
            HighestScore = statsDict.ContainsKey("highestScore") ? (double)statsDict["highestScore"] : 0,
            AverageScore = statsDict.ContainsKey("averageScore") ? (double)statsDict["averageScore"] : 0,
            LastPlayed = statsDict.ContainsKey("lastPlayed") 
                ? new DateTime((long)statsDict["lastPlayed"]) 
                : DateTime.MinValue
        };
    }

    public async Task<List<PlayerScore>> GetPlayerRangeAsync(int start, int end)
    {
        // Get players in rank range (convert to 0-based indexing)
        var players = await _db.SortedSetRangeByRankWithScoresAsync(
            LeaderboardKey,
            start: start - 1,
            stop: end - 1,
            order: Order.Descending);

        var result = new List<PlayerScore>();
        for (int i = 0; i < players.Length; i++)
        {
            result.Add(new PlayerScore
            {
                PlayerId = players[i].Element.ToString(),
                Score = players[i].Score,
                Rank = start + i
            });
        }

        return result;
    }

    public async Task<double?> GetPlayerScoreAsync(string playerId)
    {
        var score = await _db.SortedSetScoreAsync(LeaderboardKey, playerId);
        return score;
    }

    public async Task<long> GetTotalPlayersAsync()
    {
        return await _db.SortedSetLengthAsync(LeaderboardKey);
    }
}
