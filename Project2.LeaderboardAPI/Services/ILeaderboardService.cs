using Project2.LeaderboardAPI.Models;

namespace Project2.LeaderboardAPI.Services;

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
