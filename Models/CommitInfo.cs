namespace AI_Powered_Code_Review_Assistant.Models;

public class CommitInfo
{
    public string Hash { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> ModifiedFiles { get; set; } = new();
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
}