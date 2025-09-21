namespace AI_Powered_Code_Review_Assistant.Models;

public class FileDiff
{
    public string FilePath { get; set; } = string.Empty;
    public string OldContent { get; set; } = string.Empty;
    public string NewContent { get; set; } = string.Empty;
    public string Diff { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
    public string FileExtension => Path.GetExtension(FilePath);
    public string Language => GetLanguageFromExtension(FileExtension);

    private static string GetLanguageFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".cc" or ".cxx" => "C++",
            ".c" => "C",
            ".go" => "Go",
            ".rs" => "Rust",
            ".php" => "PHP",
            ".rb" => "Ruby",
            ".sql" => "SQL",
            ".html" => "HTML",
            ".css" => "CSS",
            ".json" => "JSON",
            ".xml" => "XML",
            ".yaml" or ".yml" => "YAML",
            _ => "Unknown"
        };
    }
}

public enum ChangeType
{
    Added,
    Modified,
    Deleted,
    Renamed
}