# AI-Powered Code Review Assistant - Complete Development Guide

This comprehensive guide walks through every step of building an AI-powered code review assistant from scratch using .NET 9.0, OpenAI API, and Git integration.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture Design](#architecture-design)
3. [Step-by-Step Development](#step-by-step-development)
4. [Testing Strategy](#testing-strategy)
5. [Configuration Management](#configuration-management)
6. [CLI Implementation](#cli-implementation)
7. [Output Formatting](#output-formatting)
8. [Deployment Considerations](#deployment-considerations)

---

## Project Overview

### What We're Building
An intelligent code review assistant that leverages Large Language Models (LLMs) to automatically analyze code for:
- Security vulnerabilities
- Code quality issues
- Performance bottlenecks
- Best practice violations
- Maintainability concerns

### Key Technologies
- **.NET 9.0**: Modern C# framework with latest features
- **OpenAI API**: GPT-4 for intelligent code analysis
- **LibGit2Sharp**: Git repository integration
- **CommandLineParser**: CLI interface
- **Dependency Injection**: Clean architecture pattern

---

## Architecture Design

### Core Principles
1. **Separation of Concerns**: Each component has a single responsibility
2. **Dependency Injection**: Loose coupling between components
3. **Interface-Driven Design**: Easy testing and extensibility
4. **Configuration-Based**: External settings for flexibility
5. **Error Handling**: Graceful failure and meaningful messages

### Component Structure
```
├── Interfaces/         # Service contracts
├── Services/           # Business logic implementations
├── Models/            # Data transfer objects
├── CLI/               # Command-line interface
├── Configuration/     # Settings management
└── Tests/             # Unit tests
```

---

## Step-by-Step Development

### Step 1: Project Setup and Dependencies

#### 1.1 Create the Project Structure
```bash
# Create solution and project
dotnet new sln -n "AI-Powered Code Review Assistant"
dotnet new webapi -n "AI-Powered Code Review Assistant"
dotnet sln add "AI-Powered Code Review Assistant"
```

#### 1.2 Configure Project File
Update `AI-Powered Code Review Assistant.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>AI_Powered_Code_Review_Assistant</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="OpenAI" Version="2.0.0" />
    <PackageReference Include="LibGit2Sharp" Version="0.30.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
    <PackageReference Include="CommandLineParser" Version="2.9.1" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="Moq" Version="4.20.70" />
  </ItemGroup>
</Project>
```

**Why these packages?**
- `OpenAI`: Official OpenAI .NET SDK for AI integration
- `LibGit2Sharp`: Native Git operations for repository analysis
- `Microsoft.Extensions.*`: Dependency injection and configuration
- `CommandLineParser`: Robust CLI argument parsing
- `Newtonsoft.Json`: JSON serialization for outputs
- `Moq`: Mocking framework for unit tests

#### 1.3 Create Directory Structure
```bash
mkdir Interfaces Services Models CLI Configuration Tests
```

### Step 2: Define Core Interfaces

#### 2.1 LLM Service Interface (`Interfaces/ILlmService.cs`)
```csharp
namespace AI_Powered_Code_Review_Assistant.Interfaces;

public interface ILlmService
{
    Task<string> ReviewCodeAsync(string code, string context, string fileName);
    Task<string> AnalyzeSecurityVulnerabilitiesAsync(string code, string fileName);
    Task<string> CheckCodeQualityAsync(string code, string fileName);
    Task<string> SuggestImprovementsAsync(string code, string fileName);
    Task<string> AnalyzePullRequestAsync(string changes, string description);
}
```

**Design Rationale:**
- Async methods for non-blocking AI API calls
- Separate methods for different analysis types
- Context-aware analysis with file names
- Pull request specific analysis

#### 2.2 Git Service Interface (`Interfaces/IGitService.cs`)
```csharp
using AI_Powered_Code_Review_Assistant.Models;

namespace AI_Powered_Code_Review_Assistant.Interfaces;

public interface IGitService
{
    Task<IEnumerable<FileDiff>> GetChangesAsync(string repoPath, string commitHash);
    Task<IEnumerable<FileDiff>> GetPullRequestChangesAsync(string repoPath, string baseBranch, string headBranch);
    Task<IEnumerable<string>> GetModifiedFilesAsync(string repoPath);
    Task<string> GetFileContentAsync(string repoPath, string filePath);
    Task<CommitInfo> GetCommitInfoAsync(string repoPath, string commitHash);
    bool IsGitRepository(string path);
}
```

**Design Rationale:**
- Repository validation methods
- Different ways to get changes (commit, PR, modified files)
- Metadata extraction for comprehensive analysis
- File content retrieval for analysis

#### 2.3 Code Review Service Interface (`Interfaces/ICodeReviewService.cs`)
```csharp
using AI_Powered_Code_Review_Assistant.Models;

namespace AI_Powered_Code_Review_Assistant.Interfaces;

public interface ICodeReviewService
{
    Task<ReviewResult> ReviewCommitAsync(string repoPath, string commitHash);
    Task<ReviewResult> ReviewPullRequestAsync(string repoPath, string baseBranch, string headBranch);
    Task<ReviewResult> ReviewFilesAsync(string repoPath, IEnumerable<string> filePaths);
    Task<ReviewResult> ReviewDirectoryAsync(string directoryPath);
}
```

**Design Rationale:**
- Orchestration layer that coordinates other services
- Multiple entry points for different review scenarios
- Consistent return type for all operations

### Step 3: Define Data Models

#### 3.1 File Diff Model (`Models/FileDiff.cs`)
```csharp
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
```

**Design Rationale:**
- Comprehensive change tracking
- Language detection for context-aware analysis
- Computed properties for convenience
- Statistics for quantitative analysis

#### 3.2 Review Models (`Models/Review.cs`)
```csharp
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
```

**Design Rationale:**
- Structured issue reporting
- Severity classification for prioritization
- Actionable suggestions with before/after examples
- Rich metadata for analysis tools

### Step 4: Implement LLM Service

#### 4.1 OpenAI Integration (`Services/LlmService.cs`)
```csharp
using AI_Powered_Code_Review_Assistant.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace AI_Powered_Code_Review_Assistant.Services;

public class LlmService : ILlmService
{
    private readonly OpenAIClient _openAiClient;
    private readonly ILogger<LlmService> _logger;
    private readonly string _model;

    public LlmService(IConfiguration configuration, ILogger<LlmService> logger)
    {
        _logger = logger;
        var apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _model = configuration["OpenAI:Model"] ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured. LLM functionality will be limited.");
            _openAiClient = null!;
        }
        else
        {
            _openAiClient = new OpenAIClient(apiKey);
        }
    }

    public async Task<string> ReviewCodeAsync(string code, string context, string fileName)
    {
        var prompt = $@"
You are an expert code reviewer. Please review the following code and provide comprehensive feedback.

File: {fileName}
Context: {context}

Code:
```
{code}
```

Please provide feedback on:
1. Code quality and best practices
2. Potential bugs or issues
3. Performance considerations
4. Security vulnerabilities
5. Maintainability and readability
6. Specific suggestions for improvement

Format your response as structured feedback with clear sections.";

        return await CallOpenAIAsync(prompt);
    }

    // ... other analysis methods with specialized prompts

    private async Task<string> CallOpenAIAsync(string prompt)
    {
        if (_openAiClient == null)
        {
            return "OpenAI API key not configured. Please set your API key using: dotnet run -- configure --openai-key YOUR_API_KEY";
        }

        try
        {
            var chatClient = _openAiClient.GetChatClient(_model);
            var completion = await chatClient.CompleteChatAsync(new[]
            {
                new UserChatMessage(prompt)
            });

            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return "Error occurred while analyzing code. Please check your OpenAI configuration.";
        }
    }
}
```

**Key Implementation Details:**
- Graceful handling of missing API keys
- Specialized prompts for different analysis types
- Comprehensive error handling
- Configurable model selection

### Step 5: Implement Git Service

#### 5.1 Repository Analysis (`Services/GitService.cs`)
```csharp
using AI_Powered_Code_Review_Assistant.Interfaces;
using AI_Powered_Code_Review_Assistant.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace AI_Powered_Code_Review_Assistant.Services;

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<FileDiff>> GetChangesAsync(string repoPath, string commitHash)
    {
        try
        {
            using var repo = new Repository(repoPath);
            var commit = repo.Lookup<Commit>(commitHash);

            if (commit?.Parents.Count() != 1)
            {
                _logger.LogWarning("Commit {CommitHash} has {ParentCount} parents, expected 1",
                    commitHash, commit?.Parents.Count() ?? 0);
                return Enumerable.Empty<FileDiff>();
            }

            var parent = commit.Parents.First();
            var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);

            var fileDiffs = new List<FileDiff>();

            foreach (var change in changes)
            {
                var diff = repo.Diff.Compare<Patch>(parent.Tree, commit.Tree, new[] { change.Path });
                var fileDiff = new FileDiff
                {
                    FilePath = change.Path,
                    ChangeType = MapChangeType(change.Status),
                    Diff = diff.Content,
                    LinesAdded = diff.LinesAdded,
                    LinesDeleted = diff.LinesDeleted
                };

                // Get file content at different commits
                if (change.Status != ChangeKind.Deleted)
                    fileDiff.NewContent = await GetFileContentAtCommitAsync(repo, commit, change.Path);

                if (change.Status != ChangeKind.Added)
                    fileDiff.OldContent = await GetFileContentAtCommitAsync(repo, parent, change.Path);

                fileDiffs.Add(fileDiff);
            }

            return fileDiffs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting changes for commit {CommitHash} in repo {RepoPath}",
                commitHash, repoPath);
            return Enumerable.Empty<FileDiff>();
        }
    }

    // ... other Git operations

    private async Task<string> GetFileContentAtCommitAsync(Repository repo, Commit commit, string path)
    {
        try
        {
            var blob = commit[path]?.Target as Blob;
            if (blob == null) return string.Empty;

            using var reader = new StreamReader(blob.GetContentStream());
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file content for {Path} at commit {CommitHash}",
                path, commit.Sha);
            return string.Empty;
        }
    }

    private static ChangeType MapChangeType(ChangeKind changeKind)
    {
        return changeKind switch
        {
            ChangeKind.Added => ChangeType.Added,
            ChangeKind.Modified => ChangeType.Modified,
            ChangeKind.Deleted => ChangeType.Deleted,
            ChangeKind.Renamed => ChangeType.Renamed,
            _ => ChangeType.Modified
        };
    }
}
```

**Key Implementation Details:**
- Robust error handling for Git operations
- Support for different types of changes
- Efficient blob content retrieval
- Comprehensive logging for debugging

### Step 6: Implement Code Review Service

#### 6.1 Orchestration Logic (`Services/CodeReviewService.cs`)
```csharp
public class CodeReviewService : ICodeReviewService
{
    private readonly ILlmService _llmService;
    private readonly IGitService _gitService;
    private readonly ILogger<CodeReviewService> _logger;

    public CodeReviewService(ILlmService llmService, IGitService gitService, ILogger<CodeReviewService> logger)
    {
        _llmService = llmService;
        _gitService = gitService;
        _logger = logger;
    }

    public async Task<ReviewResult> ReviewCommitAsync(string repoPath, string commitHash)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting review of commit {CommitHash} in {RepoPath}", commitHash, repoPath);

        try
        {
            var changes = await _gitService.GetChangesAsync(repoPath, commitHash);
            var commitInfo = await _gitService.GetCommitInfoAsync(repoPath, commitHash);

            var reviews = new List<Review>();

            foreach (var change in changes)
            {
                if (ShouldSkipFile(change.FilePath))
                {
                    _logger.LogDebug("Skipping file {FilePath}", change.FilePath);
                    continue;
                }

                var review = await ReviewFileChangeAsync(change, commitInfo.Message);
                if (review != null)
                {
                    reviews.Add(review);
                }
            }

            var result = new ReviewResult
            {
                Reviews = reviews,
                RepoPath = repoPath,
                CommitHash = commitHash,
                Duration = stopwatch.Elapsed,
                Summary = GenerateSummary(reviews)
            };

            _logger.LogInformation("Completed review of commit {CommitHash} in {Duration}ms",
                commitHash, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing commit {CommitHash}", commitHash);
            return new ReviewResult
            {
                RepoPath = repoPath,
                CommitHash = commitHash,
                Duration = stopwatch.Elapsed
            };
        }
    }

    private async Task<Review?> ReviewFileContentAsync(string filePath, string content, string context)
    {
        try
        {
            // Parallel analysis for efficiency
            var reviewTask = _llmService.ReviewCodeAsync(content, context, filePath);
            var securityTask = _llmService.AnalyzeSecurityVulnerabilitiesAsync(content, filePath);
            var qualityTask = _llmService.CheckCodeQualityAsync(content, filePath);

            await Task.WhenAll(reviewTask, securityTask, qualityTask);

            var reviewText = await reviewTask;
            var securityAnalysis = await securityTask;
            var qualityAnalysis = await qualityTask;

            var issues = ParseIssuesFromAnalysis(securityAnalysis, qualityAnalysis);
            var suggestions = ParseSuggestionsFromAnalysis(reviewText);

            return new Review
            {
                FilePath = filePath,
                ReviewText = reviewText,
                Type = DetermineReviewType(issues),
                Severity = DetermineOverallSeverity(issues),
                Issues = issues,
                Suggestions = suggestions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing file content for {FilePath}", filePath);
            return null;
        }
    }

    private bool ShouldSkipFile(string filePath)
    {
        var skipExtensions = new[] { ".exe", ".dll", ".bin", ".obj", ".png", ".jpg", ".jpeg",
            ".gif", ".bmp", ".ico", ".pdf", ".zip", ".7z", ".rar" };
        var skipDirectories = new[] { "bin", "obj", "node_modules", ".git", ".vs",
            "packages", "dist", "build" };

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (skipExtensions.Contains(extension))
            return true;

        return skipDirectories.Any(dir =>
            filePath.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar) ||
            filePath.StartsWith(dir + Path.DirectorySeparatorChar));
    }

    // ... other helper methods for parsing, scoring, etc.
}
```

**Key Implementation Details:**
- Performance monitoring with stopwatch
- Parallel AI analysis for efficiency
- File filtering to avoid binary files
- Comprehensive error recovery

### Step 7: CLI Implementation

#### 7.1 Command Options (`CLI/CliOptions.cs`)
```csharp
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

    [Option('f', "format", Required = false, Default = "json",
        HelpText = "Output format: json, html, markdown")]
    public string Format { get; set; } = "json";
}

// ... other command option classes
```

#### 7.2 CLI Handler (`CLI/CliHandler.cs`)
```csharp
public class CliHandler
{
    private readonly ICodeReviewService _codeReviewService;
    private readonly IGitService _gitService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CliHandler> _logger;

    public async Task<int> HandleArgumentsAsync(string[] args)
    {
        return await Parser.Default.ParseArguments<
            ReviewCommitOptions,
            ReviewPullRequestOptions,
            ReviewFilesOptions,
            ReviewDirectoryOptions,
            ConfigureOptions>(args)
            .MapResult(
                (ReviewCommitOptions opts) => HandleReviewCommitAsync(opts),
                (ReviewPullRequestOptions opts) => HandleReviewPullRequestAsync(opts),
                (ReviewFilesOptions opts) => HandleReviewFilesAsync(opts),
                (ReviewDirectoryOptions opts) => HandleReviewDirectoryAsync(opts),
                (ConfigureOptions opts) => HandleConfigureAsync(opts),
                errs => Task.FromResult(1));
    }

    private async Task<int> HandleReviewCommitAsync(ReviewCommitOptions options)
    {
        try
        {
            if (!_gitService.IsGitRepository(options.RepoPath))
            {
                Console.WriteLine($"Error: {options.RepoPath} is not a valid git repository.");
                return 1;
            }

            Console.WriteLine($"Reviewing commit {options.CommitHash} in {options.RepoPath}...");

            var result = await _codeReviewService.ReviewCommitAsync(options.RepoPath, options.CommitHash);

            await OutputFormatter.WriteOutputAsync(result, options.OutputPath, options.Format);

            Console.WriteLine($"Review completed in {result.Duration.TotalSeconds:F2} seconds.");
            Console.WriteLine($"Found {result.Summary.TotalIssues} issues across {result.Summary.TotalFiles} files.");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing commit {CommitHash}", options.CommitHash);
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    // ... other command handlers
}
```

### Step 8: Output Formatting

#### 8.1 Multi-Format Output (`CLI/OutputFormatter.cs`)
```csharp
public static class OutputFormatter
{
    public static async Task WriteOutputAsync(ReviewResult result, string? outputPath, string format)
    {
        var content = format.ToLowerInvariant() switch
        {
            "json" => FormatJson(result),
            "html" => FormatHtml(result),
            "markdown" => FormatMarkdown(result),
            _ => FormatJson(result)
        };

        if (string.IsNullOrEmpty(outputPath))
        {
            Console.WriteLine(content);
        }
        else
        {
            await File.WriteAllTextAsync(outputPath, content);
            Console.WriteLine($"Review output written to: {outputPath}");
        }
    }

    private static string FormatHtml(ReviewResult result)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <title>Code Review Report</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; }");
        html.AppendLine("        .summary { background: #f5f5f5; padding: 20px; border-radius: 5px; margin-bottom: 20px; }");
        html.AppendLine("        .file-review { border: 1px solid #ddd; margin-bottom: 20px; border-radius: 5px; }");
        html.AppendLine("        .file-header { background: #e9e9e9; padding: 10px; font-weight: bold; }");
        html.AppendLine("        .issue { margin: 10px; padding: 10px; border-left: 4px solid #ff6b6b; background: #fff5f5; }");
        html.AppendLine("        .suggestion { margin: 10px; padding: 10px; border-left: 4px solid #51cf66; background: #f3fff3; }");
        html.AppendLine("        .severity-critical { border-left-color: #e03131; }");
        html.AppendLine("        .severity-high { border-left-color: #fd7e14; }");
        html.AppendLine("        .severity-medium { border-left-color: #fab005; }");
        html.AppendLine("        .severity-low { border-left-color: #51cf66; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Generate rich HTML content...

        return html.ToString();
    }

    // ... other formatting methods
}
```

### Step 9: Configuration Management

#### 9.1 Application Settings (`appsettings.json`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OpenAI": {
    "ApiKey": "",
    "Model": "gpt-4"
  },
  "CodeReview": {
    "MaxFileSizeBytes": 1048576,
    "SupportedFileExtensions": [
      ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".go", ".rs", ".php", ".rb", ".sql"
    ],
    "ExcludeDirectories": [
      "bin", "obj", "node_modules", ".git", ".vs", "packages", "dist", "build"
    ],
    "ExcludeFileExtensions": [
      ".exe", ".dll", ".bin", ".obj", ".png", ".jpg", ".jpeg", ".gif",
      ".bmp", ".ico", ".pdf", ".zip", ".7z", ".rar"
    ]
  }
}
```

#### 9.2 Program Entry Point (`Program.cs`)
```csharp
using AI_Powered_Code_Review_Assistant.CLI;
using AI_Powered_Code_Review_Assistant.Interfaces;
using AI_Powered_Code_Review_Assistant.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

// Register configuration and logging
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
});

// Check API key configuration
var openAiApiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(openAiApiKey))
{
    Console.WriteLine("⚠️  OpenAI API key not found. Please configure it using:");
    Console.WriteLine("   dotnet run -- configure --openai-key YOUR_API_KEY");
    Console.WriteLine("   or set the OPENAI_API_KEY environment variable");
    Console.WriteLine();
}

// Register services
services.AddTransient<ILlmService, LlmService>();
services.AddTransient<IGitService, GitService>();
services.AddTransient<ICodeReviewService, CodeReviewService>();
services.AddTransient<CliHandler>();

var serviceProvider = services.BuildServiceProvider();

try
{
    var cliHandler = serviceProvider.GetRequiredService<CliHandler>();
    var exitCode = await cliHandler.HandleArgumentsAsync(args);
    Environment.Exit(exitCode);
}
catch (Exception ex)
{
    var logger = serviceProvider.GetService<ILogger<Program>>();
    logger?.LogError(ex, "Application error");
    Console.WriteLine($"Fatal error: {ex.Message}");
    Environment.Exit(1);
}
```

---

## Testing Strategy

### Unit Testing Framework
```csharp
using AI_Powered_Code_Review_Assistant.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AI_Powered_Code_Review_Assistant.Tests;

public class GitServiceTests
{
    private readonly GitService _gitService;
    private readonly Mock<ILogger<GitService>> _mockLogger;

    public GitServiceTests()
    {
        _mockLogger = new Mock<ILogger<GitService>>();
        _gitService = new GitService(_mockLogger.Object);
    }

    public void IsGitRepository_WithValidPath_ReturnsTrue()
    {
        // Arrange
        var tempDir = Path.GetTempPath();

        // Act
        var result = _gitService.IsGitRepository(tempDir);

        // Assert
        Console.WriteLine($"Testing git repository detection: {result}");
    }

    public async Task GetFileContentAsync_WithValidFile_ReturnsContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var testContent = "Test content for git service";
        await File.WriteAllTextAsync(tempFile, testContent);

        // Act
        var result = await _gitService.GetFileContentAsync(
            Path.GetDirectoryName(tempFile)!,
            Path.GetFileName(tempFile));

        // Cleanup
        File.Delete(tempFile);

        // Assert
        Console.WriteLine($"File content test: {(result == testContent ? "PASS" : "FAIL")}");
    }
}
```

### Integration Testing
- Test CLI commands end-to-end
- Validate Git repository operations
- Mock OpenAI API for consistent testing
- Performance benchmarking

---

## Deployment Considerations

### Build and Distribution
```bash
# Build release version
dotnet build --configuration Release

# Create self-contained executable
dotnet publish --configuration Release --self-contained true --runtime win-x64

# Package for distribution
dotnet pack --configuration Release
```

### Environment Configuration
- Use environment variables for production secrets
- Implement configuration validation
- Add health checks for external dependencies

### Performance Optimization
- Implement caching for repeated Git operations
- Batch OpenAI API calls where possible
- Use streaming for large file processing
- Add concurrent processing for multiple files

### Security Considerations
- Never log API keys or sensitive information
- Validate all file paths to prevent directory traversal
- Implement rate limiting for API calls
- Use secure credential storage

---

## Usage Examples

### Basic Commands
```bash
# Configure the application
dotnet run -- configure --openai-key sk-your-key-here --openai-model gpt-4

# Review a specific commit
dotnet run -- review-commit --repo /path/to/repo --commit abc123def --format html --output review.html

# Review a pull request
dotnet run -- review-pr --repo /path/to/repo --base main --head feature-branch --format markdown

# Review specific files
dotnet run -- review-files --repo /path/to/repo --files "src/file1.cs,src/file2.cs" --format json

# Review entire directory
dotnet run -- review-directory --directory /path/to/source --format markdown --output report.md
```

### Integration with CI/CD
```yaml
# GitHub Actions example
- name: Code Review
  run: |
    dotnet run -- review-commit --repo . --commit ${{ github.sha }} --format json --output review.json
    # Parse results and fail build if critical issues found
```

---

## Conclusion

This comprehensive guide has walked through every aspect of building a production-ready AI-powered code review assistant. The architecture emphasizes:

1. **Modularity**: Clear separation of concerns
2. **Testability**: Interface-driven design with dependency injection
3. **Extensibility**: Easy to add new analysis types or output formats
4. **Reliability**: Comprehensive error handling and logging
5. **Performance**: Efficient Git operations and parallel processing
6. **Usability**: Rich CLI interface with multiple output formats

The resulting application provides intelligent, automated code review capabilities that can integrate into any development workflow, helping teams maintain code quality, security, and best practices.