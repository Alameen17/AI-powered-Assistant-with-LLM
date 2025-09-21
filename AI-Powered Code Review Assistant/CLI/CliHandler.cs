using AI_Powered_Code_Review_Assistant.Interfaces;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AI_Powered_Code_Review_Assistant.CLI;

public class CliHandler
{
    private readonly ICodeReviewService _codeReviewService;
    private readonly IGitService _gitService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CliHandler> _logger;

    public CliHandler(
        ICodeReviewService codeReviewService,
        IGitService gitService,
        IConfiguration configuration,
        ILogger<CliHandler> logger)
    {
        _codeReviewService = codeReviewService;
        _gitService = gitService;
        _configuration = configuration;
        _logger = logger;
    }

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

    private async Task<int> HandleReviewPullRequestAsync(ReviewPullRequestOptions options)
    {
        try
        {
            if (!_gitService.IsGitRepository(options.RepoPath))
            {
                Console.WriteLine($"Error: {options.RepoPath} is not a valid git repository.");
                return 1;
            }

            Console.WriteLine($"Reviewing pull request from {options.BaseBranch} to {options.HeadBranch} in {options.RepoPath}...");

            var result = await _codeReviewService.ReviewPullRequestAsync(options.RepoPath, options.BaseBranch, options.HeadBranch);

            await OutputFormatter.WriteOutputAsync(result, options.OutputPath, options.Format);

            Console.WriteLine($"Review completed in {result.Duration.TotalSeconds:F2} seconds.");
            Console.WriteLine($"Found {result.Summary.TotalIssues} issues across {result.Summary.TotalFiles} files.");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing pull request from {BaseBranch} to {HeadBranch}", options.BaseBranch, options.HeadBranch);
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleReviewFilesAsync(ReviewFilesOptions options)
    {
        try
        {
            if (!_gitService.IsGitRepository(options.RepoPath))
            {
                Console.WriteLine($"Error: {options.RepoPath} is not a valid git repository.");
                return 1;
            }

            var files = options.Files.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(f => f.Trim())
                                   .ToList();

            Console.WriteLine($"Reviewing {files.Count} files in {options.RepoPath}...");

            var result = await _codeReviewService.ReviewFilesAsync(options.RepoPath, files);

            await OutputFormatter.WriteOutputAsync(result, options.OutputPath, options.Format);

            Console.WriteLine($"Review completed in {result.Duration.TotalSeconds:F2} seconds.");
            Console.WriteLine($"Found {result.Summary.TotalIssues} issues across {result.Summary.TotalFiles} files.");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing files");
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleReviewDirectoryAsync(ReviewDirectoryOptions options)
    {
        try
        {
            if (!Directory.Exists(options.DirectoryPath))
            {
                Console.WriteLine($"Error: Directory {options.DirectoryPath} does not exist.");
                return 1;
            }

            Console.WriteLine($"Reviewing directory {options.DirectoryPath}...");

            var result = await _codeReviewService.ReviewDirectoryAsync(options.DirectoryPath);

            await OutputFormatter.WriteOutputAsync(result, options.OutputPath, options.Format);

            Console.WriteLine($"Review completed in {result.Duration.TotalSeconds:F2} seconds.");
            Console.WriteLine($"Found {result.Summary.TotalIssues} issues across {result.Summary.TotalFiles} files.");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing directory {DirectoryPath}", options.DirectoryPath);
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleConfigureAsync(ConfigureOptions options)
    {
        try
        {
            if (options.ShowConfig)
            {
                ShowCurrentConfiguration();
                return 0;
            }

            var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            var config = await File.ReadAllTextAsync(configFile);

            if (!string.IsNullOrEmpty(options.OpenAiKey))
            {
                Console.WriteLine("Setting OpenAI API key...");
                Environment.SetEnvironmentVariable("OPENAI_API_KEY", options.OpenAiKey, EnvironmentVariableTarget.User);
                Console.WriteLine("OpenAI API key set successfully.");
            }

            if (!string.IsNullOrEmpty(options.OpenAiModel))
            {
                Console.WriteLine($"Setting OpenAI model to {options.OpenAiModel}...");
                Environment.SetEnvironmentVariable("OPENAI_MODEL", options.OpenAiModel, EnvironmentVariableTarget.User);
                Console.WriteLine("OpenAI model set successfully.");
            }

            Console.WriteLine("Configuration updated. Restart the application for changes to take effect.");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring application");
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private void ShowCurrentConfiguration()
    {
        Console.WriteLine("Current Configuration:");
        Console.WriteLine("=====================");

        var apiKey = _configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var model = _configuration["OpenAI:Model"] ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4";

        Console.WriteLine($"OpenAI API Key: {(string.IsNullOrEmpty(apiKey) ? "Not set" : "Set (hidden)")}");
        Console.WriteLine($"OpenAI Model: {model}");
        Console.WriteLine();

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("⚠️  OpenAI API key is not configured. Use the configure command to set it:");
            Console.WriteLine("   dotnet run -- configure --openai-key YOUR_API_KEY");
        }
    }
}