using Microsoft.AspNetCore.Mvc;
using Project2.LeaderboardAPI.Models;
using Project2.LeaderboardAPI.Services;

namespace Project2.LeaderboardAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(
        ILeaderboardService leaderboardService,
        ILogger<LeaderboardController> logger)
    {
        _leaderboardService = leaderboardService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a player's score
    /// </summary>
    [HttpPost("score")]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreSubmission submission)
    {
        if (string.IsNullOrWhiteSpace(submission.PlayerId))
            return BadRequest("PlayerId is required");

        if (submission.Score < 0)
            return BadRequest("Score must be non-negative");

        var success = await _leaderboardService.SubmitScoreAsync(submission.PlayerId, submission.Score);
        
        if (success)
        {
            var rank = await _leaderboardService.GetPlayerRankAsync(submission.PlayerId);
            _logger.LogInformation(
                "Score submitted: Player={PlayerId}, Score={Score}, Rank={Rank}", 
                submission.PlayerId, submission.Score, rank);
            
            return Ok(new 
            { 
                message = "Score submitted successfully",
                playerId = submission.PlayerId,
                score = submission.Score,
                rank = rank
            });
        }

        return StatusCode(500, "Failed to submit score");
    }

    /// <summary>
    /// Get top N players
    /// </summary>
    [HttpGet("top/{count}")]
    public async Task<ActionResult<List<PlayerScore>>> GetTopPlayers(int count = 10)
    {
        if (count <= 0 || count > 100)
            return BadRequest("Count must be between 1 and 100");

        var topPlayers = await _leaderboardService.GetTopPlayersAsync(count);
        return Ok(topPlayers);
    }

    /// <summary>
    /// Get player's rank
    /// </summary>
    [HttpGet("rank/{playerId}")]
    public async Task<IActionResult> GetPlayerRank(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
            return BadRequest("PlayerId is required");

        var rank = await _leaderboardService.GetPlayerRankAsync(playerId);
        
        if (rank == -1)
            return NotFound($"Player {playerId} not found in leaderboard");

        var score = await _leaderboardService.GetPlayerScoreAsync(playerId);
        var totalPlayers = await _leaderboardService.GetTotalPlayersAsync();

        return Ok(new 
        { 
            playerId,
            rank,
            score,
            totalPlayers,
            percentile = Math.Round((1 - (double)rank / totalPlayers) * 100, 2)
        });
    }

    /// <summary>
    /// Get player statistics
    /// </summary>
    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<PlayerStats>> GetPlayerStats(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
            return BadRequest("PlayerId is required");

        var stats = await _leaderboardService.GetPlayerStatsAsync(playerId);
        
        if (stats == null)
            return NotFound($"Player {playerId} not found");

        return Ok(stats);
    }

    /// <summary>
    /// Get players in rank range
    /// </summary>
    [HttpGet("range/{start}/{end}")]
    public async Task<ActionResult<List<PlayerScore>>> GetPlayerRange(int start, int end)
    {
        if (start <= 0)
            return BadRequest("Start rank must be greater than 0");

        if (end < start)
            return BadRequest("End rank must be greater than or equal to start rank");

        if (end - start > 100)
            return BadRequest("Range cannot exceed 100 players");

        var players = await _leaderboardService.GetPlayerRangeAsync(start, end);
        return Ok(players);
    }

    /// <summary>
    /// Get leaderboard statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetLeaderboardStats()
    {
        var totalPlayers = await _leaderboardService.GetTotalPlayersAsync();
        var topPlayer = (await _leaderboardService.GetTopPlayersAsync(1)).FirstOrDefault();

        return Ok(new
        {
            totalPlayers,
            topPlayer = topPlayer?.PlayerId,
            topScore = topPlayer?.Score
        });
    }
}
