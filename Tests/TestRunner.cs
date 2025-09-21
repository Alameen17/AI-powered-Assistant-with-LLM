using AI_Powered_Code_Review_Assistant.Interfaces;
using AI_Powered_Code_Review_Assistant.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AI_Powered_Code_Review_Assistant.Tests;

public static class TestRunner
{
    public static async Task RunAllTestsAsync()
    {
        Console.WriteLine("Running AI-Powered Code Review Assistant Tests");
        Console.WriteLine("==============================================");

        await RunGitServiceTests();
        await RunCodeReviewServiceTests();
        await RunLlmServiceTests();

        Console.WriteLine("\nTests completed!");
    }

    private static async Task RunGitServiceTests()
    {
        Console.WriteLine("\n--- Git Service Tests ---");
        var tests = new GitServiceTests();

        tests.IsGitRepository_WithValidPath_ReturnsTrue();
        tests.IsGitRepository_WithInvalidPath_ReturnsFalse();
        await tests.GetFileContentAsync_WithValidFile_ReturnsContent();
    }

    private static async Task RunCodeReviewServiceTests()
    {
        Console.WriteLine("\n--- Code Review Service Tests ---");

        var mockLlm = new Mock<ILlmService>();
        var mockGit = new Mock<IGitService>();
        var mockLogger = new Mock<ILogger<CodeReviewService>>();

        mockLlm.Setup(x => x.ReviewCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync("Mock review result");

        mockGit.Setup(x => x.IsGitRepository(It.IsAny<string>()))
               .Returns(true);

        var service = new CodeReviewService(mockLlm.Object, mockGit.Object, mockLogger.Object);

        Console.WriteLine("Code Review Service initialized successfully: PASS");
    }

    private static async Task RunLlmServiceTests()
    {
        Console.WriteLine("\n--- LLM Service Tests ---");

        var mockConfig = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<LlmService>>();

        mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        mockConfig.Setup(x => x["OpenAI:Model"]).Returns("gpt-4");

        try
        {
            var service = new LlmService(mockConfig.Object, mockLogger.Object);
            Console.WriteLine("LLM Service initialized successfully: PASS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LLM Service initialization failed: {ex.Message}");
        }
    }
}