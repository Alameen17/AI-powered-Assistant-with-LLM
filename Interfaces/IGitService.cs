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