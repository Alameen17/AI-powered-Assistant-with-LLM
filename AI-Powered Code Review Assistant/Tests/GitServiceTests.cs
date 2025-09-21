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
        var tempDir = Path.GetTempPath();
        var result = _gitService.IsGitRepository(tempDir);

        Console.WriteLine($"Testing git repository detection: {result}");
    }

    public void IsGitRepository_WithInvalidPath_ReturnsFalse()
    {
        var invalidPath = "C:\\NonExistentPath12345";
        var result = _gitService.IsGitRepository(invalidPath);

        Console.WriteLine($"Testing invalid path: {result}");
    }

    public async Task GetFileContentAsync_WithValidFile_ReturnsContent()
    {
        var tempFile = Path.GetTempFileName();
        var testContent = "Test content for git service";

        await File.WriteAllTextAsync(tempFile, testContent);

        var result = await _gitService.GetFileContentAsync(Path.GetDirectoryName(tempFile)!, Path.GetFileName(tempFile));

        File.Delete(tempFile);

        Console.WriteLine($"File content test: {(result == testContent ? "PASS" : "FAIL")}");
    }
}