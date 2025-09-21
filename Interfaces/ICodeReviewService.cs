using AI_Powered_Code_Review_Assistant.Models;

namespace AI_Powered_Code_Review_Assistant.Interfaces;

public interface ICodeReviewService
{
    Task<ReviewResult> ReviewCommitAsync(string repoPath, string commitHash);
    Task<ReviewResult> ReviewPullRequestAsync(string repoPath, string baseBranch, string headBranch);
    Task<ReviewResult> ReviewFilesAsync(string repoPath, IEnumerable<string> filePaths);
    Task<ReviewResult> ReviewDirectoryAsync(string directoryPath);
}