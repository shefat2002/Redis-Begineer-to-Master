namespace Project2.LeaderboardAPI.Models;

public class PlayerScore
{
    public string PlayerId { get; set; } = string.Empty;
    public double Score { get; set; }
    public long Rank { get; set; }
}

public class ScoreSubmission
{
    public string PlayerId { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class PlayerStats
{
    public string PlayerId { get; set; } = string.Empty;
    public double CurrentScore { get; set; }
    public long Rank { get; set; }
    public int TotalGames { get; set; }
    public double HighestScore { get; set; }
    public double AverageScore { get; set; }
    public DateTime LastPlayed { get; set; }
}
