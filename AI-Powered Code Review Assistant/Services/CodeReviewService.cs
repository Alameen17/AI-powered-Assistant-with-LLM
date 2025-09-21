using AI_Powered_Code_Review_Assistant.Interfaces;
using AI_Powered_Code_Review_Assistant.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AI_Powered_Code_Review_Assistant.Services;

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

            _logger.LogInformation("Completed review of commit {CommitHash} in {Duration}ms", commitHash, stopwatch.ElapsedMilliseconds);
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

    public async Task<ReviewResult> ReviewPullRequestAsync(string repoPath, string baseBranch, string headBranch)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting review of PR from {BaseBranch} to {HeadBranch} in {RepoPath}", baseBranch, headBranch, repoPath);

        try
        {
            var changes = await _gitService.GetPullRequestChangesAsync(repoPath, baseBranch, headBranch);
            var reviews = new List<Review>();

            foreach (var change in changes)
            {
                if (ShouldSkipFile(change.FilePath))
                {
                    continue;
                }

                var review = await ReviewFileChangeAsync(change, $"PR from {baseBranch} to {headBranch}");
                if (review != null)
                {
                    reviews.Add(review);
                }
            }

            var allChanges = string.Join("\n\n", changes.Select(c => $"File: {c.FilePath}\n{c.Diff}"));
            var prAnalysis = await _llmService.AnalyzePullRequestAsync(allChanges, $"Pull request from {baseBranch} to {headBranch}");

            var result = new ReviewResult
            {
                Reviews = reviews,
                RepoPath = repoPath,
                Duration = stopwatch.Elapsed,
                Summary = GenerateSummary(reviews)
            };

            _logger.LogInformation("Completed PR review in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing PR from {BaseBranch} to {HeadBranch}", baseBranch, headBranch);
            return new ReviewResult
            {
                RepoPath = repoPath,
                Duration = stopwatch.Elapsed
            };
        }
    }

    public async Task<ReviewResult> ReviewFilesAsync(string repoPath, IEnumerable<string> filePaths)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting review of {FileCount} files in {RepoPath}", filePaths.Count(), repoPath);

        try
        {
            var reviews = new List<Review>();

            foreach (var filePath in filePaths)
            {
                if (ShouldSkipFile(filePath))
                {
                    continue;
                }

                var content = await _gitService.GetFileContentAsync(repoPath, filePath);
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                var review = await ReviewFileContentAsync(filePath, content, "File review");
                if (review != null)
                {
                    reviews.Add(review);
                }
            }

            var result = new ReviewResult
            {
                Reviews = reviews,
                RepoPath = repoPath,
                Duration = stopwatch.Elapsed,
                Summary = GenerateSummary(reviews)
            };

            _logger.LogInformation("Completed file review in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing files");
            return new ReviewResult
            {
                RepoPath = repoPath,
                Duration = stopwatch.Elapsed
            };
        }
    }

    public async Task<ReviewResult> ReviewDirectoryAsync(string directoryPath)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting review of directory {DirectoryPath}", directoryPath);

        try
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                              .Where(f => !ShouldSkipFile(f))
                              .ToList();

            var reviews = new List<Review>();

            foreach (var filePath in files)
            {
                var content = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                var relativePath = Path.GetRelativePath(directoryPath, filePath);
                var review = await ReviewFileContentAsync(relativePath, content, "Directory review");
                if (review != null)
                {
                    reviews.Add(review);
                }
            }

            var result = new ReviewResult
            {
                Reviews = reviews,
                RepoPath = directoryPath,
                Duration = stopwatch.Elapsed,
                Summary = GenerateSummary(reviews)
            };

            _logger.LogInformation("Completed directory review in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing directory {DirectoryPath}", directoryPath);
            return new ReviewResult
            {
                RepoPath = directoryPath,
                Duration = stopwatch.Elapsed
            };
        }
    }

    public async Task<ReviewResult> ReviewFilesAsync(Dictionary<string, string> uploadedFiles)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting review of {FileCount} uploaded files", uploadedFiles.Count);

        try
        {
            var reviews = new List<Review>();

            foreach (var file in uploadedFiles)
            {
                if (string.IsNullOrEmpty(file.Value))
                {
                    continue;
                }

                var review = await ReviewFileContentAsync(file.Key, file.Value, "Uploaded file review");
                if (review != null)
                {
                    reviews.Add(review);
                }
            }

            var result = new ReviewResult
            {
                Reviews = reviews,
                RepoPath = "Uploaded Files",
                Duration = stopwatch.Elapsed,
                Summary = GenerateSummary(reviews)
            };

            _logger.LogInformation("Completed uploaded files review in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing uploaded files");
            return new ReviewResult
            {
                RepoPath = "Uploaded Files",
                Duration = stopwatch.Elapsed
            };
        }
    }

    private async Task<Review?> ReviewFileChangeAsync(FileDiff change, string context)
    {
        try
        {
            var content = change.ChangeType == ChangeType.Deleted ? change.OldContent : change.NewContent;
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            return await ReviewFileContentAsync(change.FilePath, content, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing file change for {FilePath}", change.FilePath);
            return null;
        }
    }

    private async Task<Review?> ReviewFileContentAsync(string filePath, string content, string context)
    {
        try
        {
            var reviewText = await _llmService.ReviewCodeAsync(content, context, filePath);
            var securityAnalysis = await _llmService.AnalyzeSecurityVulnerabilitiesAsync(content, filePath);
            var qualityAnalysis = await _llmService.CheckCodeQualityAsync(content, filePath);

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

    private List<Issue> ParseIssuesFromAnalysis(string securityAnalysis, string qualityAnalysis)
    {
        var issues = new List<Issue>();

        issues.AddRange(ParseSecurityIssues(securityAnalysis));
        issues.AddRange(ParseQualityIssues(qualityAnalysis));

        return issues;
    }

    private List<Issue> ParseSecurityIssues(string analysis)
    {
        var issues = new List<Issue>();

        if (analysis.Contains("Critical", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new Issue
            {
                Title = "Security Vulnerability Detected",
                Description = ExtractCriticalSection(analysis),
                Category = IssueCategory.Security,
                Severity = Severity.Critical,
                Rule = "Security Analysis"
            });
        }

        return issues;
    }

    private List<Issue> ParseQualityIssues(string analysis)
    {
        var issues = new List<Issue>();

        if (analysis.Contains("complexity", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new Issue
            {
                Title = "High Complexity Detected",
                Description = ExtractComplexitySection(analysis),
                Category = IssueCategory.Maintainability,
                Severity = Severity.Medium,
                Rule = "Code Quality Analysis"
            });
        }

        return issues;
    }

    private List<Suggestion> ParseSuggestionsFromAnalysis(string analysis)
    {
        var suggestions = new List<Suggestion>();

        if (analysis.Contains("suggestion", StringComparison.OrdinalIgnoreCase) ||
            analysis.Contains("improve", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add(new Suggestion
            {
                Title = "General Improvement",
                Description = ExtractSuggestionSection(analysis),
                RecommendedChange = "See description for details"
            });
        }

        return suggestions;
    }

    private string ExtractCriticalSection(string text)
    {
        var lines = text.Split('\n');
        var criticalLines = lines.Where(line =>
            line.Contains("critical", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("vulnerability", StringComparison.OrdinalIgnoreCase))
            .Take(3);

        return string.Join(" ", criticalLines).Trim();
    }

    private string ExtractComplexitySection(string text)
    {
        var lines = text.Split('\n');
        var complexityLines = lines.Where(line =>
            line.Contains("complexity", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("maintainability", StringComparison.OrdinalIgnoreCase))
            .Take(2);

        return string.Join(" ", complexityLines).Trim();
    }

    private string ExtractSuggestionSection(string text)
    {
        var lines = text.Split('\n');
        var suggestionLines = lines.Where(line =>
            line.Contains("suggestion", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("improve", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("recommend", StringComparison.OrdinalIgnoreCase))
            .Take(2);

        return string.Join(" ", suggestionLines).Trim();
    }

    private ReviewType DetermineReviewType(List<Issue> issues)
    {
        if (issues.Any(i => i.Category == IssueCategory.Security))
            return ReviewType.Security;

        if (issues.Any(i => i.Category == IssueCategory.Performance))
            return ReviewType.Performance;

        if (issues.Any(i => i.Category == IssueCategory.Maintainability))
            return ReviewType.Maintainability;

        return ReviewType.CodeQuality;
    }

    private Severity DetermineOverallSeverity(List<Issue> issues)
    {
        if (issues.Any(i => i.Severity == Severity.Critical))
            return Severity.Critical;

        if (issues.Any(i => i.Severity == Severity.High))
            return Severity.High;

        if (issues.Any(i => i.Severity == Severity.Medium))
            return Severity.Medium;

        if (issues.Any(i => i.Severity == Severity.Low))
            return Severity.Low;

        return Severity.Info;
    }

    private ReviewSummary GenerateSummary(List<Review> reviews)
    {
        var allIssues = reviews.SelectMany(r => r.Issues).ToList();

        return new ReviewSummary
        {
            TotalFiles = reviews.Count,
            FilesWithIssues = reviews.Count(r => r.Issues.Any()),
            TotalIssues = allIssues.Count,
            CriticalIssues = allIssues.Count(i => i.Severity == Severity.Critical),
            HighIssues = allIssues.Count(i => i.Severity == Severity.High),
            MediumIssues = allIssues.Count(i => i.Severity == Severity.Medium),
            LowIssues = allIssues.Count(i => i.Severity == Severity.Low),
            InfoIssues = allIssues.Count(i => i.Severity == Severity.Info),
            SecurityIssues = allIssues.Count(i => i.Category == IssueCategory.Security),
            PerformanceIssues = allIssues.Count(i => i.Category == IssueCategory.Performance),
            QualityIssues = allIssues.Count(i => i.Category == IssueCategory.CodeStyle || i.Category == IssueCategory.Maintainability),
            OverallScore = CalculateOverallScore(allIssues),
            TopRecommendations = GetTopRecommendations(reviews)
        };
    }

    private string CalculateOverallScore(List<Issue> issues)
    {
        if (!issues.Any()) return "A+ (Excellent)";

        var score = 100;
        score -= issues.Count(i => i.Severity == Severity.Critical) * 25;
        score -= issues.Count(i => i.Severity == Severity.High) * 15;
        score -= issues.Count(i => i.Severity == Severity.Medium) * 10;
        score -= issues.Count(i => i.Severity == Severity.Low) * 5;
        score -= issues.Count(i => i.Severity == Severity.Info) * 2;

        return score switch
        {
            >= 90 => "A (Excellent)",
            >= 80 => "B (Good)",
            >= 70 => "C (Fair)",
            >= 60 => "D (Poor)",
            _ => "F (Needs Significant Improvement)"
        };
    }

    private List<string> GetTopRecommendations(List<Review> reviews)
    {
        return reviews
            .SelectMany(r => r.Suggestions)
            .Take(5)
            .Select(s => s.Title)
            .ToList();
    }

    private bool ShouldSkipFile(string filePath)
    {
        var skipExtensions = new[] { ".exe", ".dll", ".bin", ".obj", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".pdf", ".zip", ".7z", ".rar" };
        var skipDirectories = new[] { "bin", "obj", "node_modules", ".git", ".vs", "packages", "dist", "build" };

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (skipExtensions.Contains(extension))
            return true;

        return skipDirectories.Any(dir => filePath.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar) ||
                                         filePath.StartsWith(dir + Path.DirectorySeparatorChar));
    }
}