using CommandLine;

namespace AI_Powered_Code_Review_Assistant.CLI;

[Verb("review-commit", HelpText = "Review a specific git commit")]
public class ReviewCommitOptions
{
    [Option('r', "repo", Required = true, HelpText = "Path to the git repository")]
    public string RepoPath { get; set; } = string.Empty;

    [Option('c', "commit", Required = true, HelpText = "Commit hash to review")]
    public string CommitHash { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output file path (optional)")]
    public string? OutputPath { get; set; }

    [Option('f', "format", Required = false, Default = "json", HelpText = "Output format: json, html, markdown")]
    public string Format { get; set; } = "json";
}

[Verb("review-pr", HelpText = "Review a pull request")]
public class ReviewPullRequestOptions
{
    [Option('r', "repo", Required = true, HelpText = "Path to the git repository")]
    public string RepoPath { get; set; } = string.Empty;

    [Option('b', "base", Required = true, HelpText = "Base branch name")]
    public string BaseBranch { get; set; } = string.Empty;

    [Option('h', "head", Required = true, HelpText = "Head branch name")]
    public string HeadBranch { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output file path (optional)")]
    public string? OutputPath { get; set; }

    [Option('f', "format", Required = false, Default = "json", HelpText = "Output format: json, html, markdown")]
    public string Format { get; set; } = "json";
}

[Verb("review-files", HelpText = "Review specific files")]
public class ReviewFilesOptions
{
    [Option('r', "repo", Required = true, HelpText = "Path to the git repository")]
    public string RepoPath { get; set; } = string.Empty;

    [Option('f', "files", Required = true, HelpText = "Comma-separated list of file paths to review")]
    public string Files { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output file path (optional)")]
    public string? OutputPath { get; set; }

    [Option("format", Required = false, Default = "json", HelpText = "Output format: json, html, markdown")]
    public string Format { get; set; } = "json";
}

[Verb("review-directory", HelpText = "Review all files in a directory")]
public class ReviewDirectoryOptions
{
    [Option('d', "directory", Required = true, HelpText = "Directory path to review")]
    public string DirectoryPath { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output file path (optional)")]
    public string? OutputPath { get; set; }

    [Option('f', "format", Required = false, Default = "json", HelpText = "Output format: json, html, markdown")]
    public string Format { get; set; } = "json";

    [Option("exclude", Required = false, HelpText = "Comma-separated list of patterns to exclude")]
    public string? ExcludePatterns { get; set; }
}

[Verb("configure", HelpText = "Configure the application settings")]
public class ConfigureOptions
{
    [Option("openai-key", Required = false, HelpText = "Set OpenAI API key")]
    public string? OpenAiKey { get; set; }

    [Option("openai-model", Required = false, HelpText = "Set OpenAI model (default: gpt-4)")]
    public string? OpenAiModel { get; set; }

    [Option("show", Required = false, HelpText = "Show current configuration")]
    public bool ShowConfig { get; set; }
}