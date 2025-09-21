namespace AI_Powered_Code_Review_Assistant.Models;

public class Review
{
    public string FilePath { get; set; } = string.Empty;
    public string ReviewText { get; set; } = string.Empty;
    public ReviewType Type { get; set; }
    public Severity Severity { get; set; }
    public List<Issue> Issues { get; set; } = new();
    public List<Suggestion> Suggestions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Issue
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueCategory Category { get; set; }
    public Severity Severity { get; set; }
    public int? LineNumber { get; set; }
    public string CodeSnippet { get; set; } = string.Empty;
    public string Rule { get; set; } = string.Empty;
}

public class Suggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RecommendedChange { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public string CodeBefore { get; set; } = string.Empty;
    public string CodeAfter { get; set; } = string.Empty;
}

public enum ReviewType
{
    Security,
    CodeQuality,
    Performance,
    Maintainability,
    General
}

public enum IssueCategory
{
    Security,
    Performance,
    BugRisk,
    CodeStyle,
    Maintainability,
    Documentation,
    Testing,
    Architecture
}

public enum Severity
{
    Info,
    Low,
    Medium,
    High,
    Critical
}