namespace AI_Powered_Code_Review_Assistant.Models;

public class ReviewResult
{
    public List<Review> Reviews { get; set; } = new();
    public ReviewSummary Summary { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string RepoPath { get; set; } = string.Empty;
    public string CommitHash { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}

public class ReviewSummary
{
    public int TotalFiles { get; set; }
    public int FilesWithIssues { get; set; }
    public int TotalIssues { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public int MediumIssues { get; set; }
    public int LowIssues { get; set; }
    public int InfoIssues { get; set; }
    public int SecurityIssues { get; set; }
    public int PerformanceIssues { get; set; }
    public int QualityIssues { get; set; }
    public string OverallScore { get; set; } = string.Empty;
    public List<string> TopRecommendations { get; set; } = new();
}