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
                _logger.LogWarning("Commit {CommitHash} has {ParentCount} parents, expected 1", commitHash, commit?.Parents.Count() ?? 0);
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

                if (change.Status != ChangeKind.Deleted)
                {
                    fileDiff.NewContent = await GetFileContentAtCommitAsync(repo, commit, change.Path);
                }

                if (change.Status != ChangeKind.Added)
                {
                    fileDiff.OldContent = await GetFileContentAtCommitAsync(repo, parent, change.Path);
                }

                fileDiffs.Add(fileDiff);
            }

            return fileDiffs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting changes for commit {CommitHash} in repo {RepoPath}", commitHash, repoPath);
            return Enumerable.Empty<FileDiff>();
        }
    }

    public async Task<IEnumerable<FileDiff>> GetPullRequestChangesAsync(string repoPath, string baseBranch, string headBranch)
    {
        try
        {
            using var repo = new Repository(repoPath);
            var baseCommit = repo.Branches[baseBranch]?.Tip;
            var headCommit = repo.Branches[headBranch]?.Tip;

            if (baseCommit == null || headCommit == null)
            {
                _logger.LogError("Could not find branches {BaseBranch} or {HeadBranch}", baseBranch, headBranch);
                return Enumerable.Empty<FileDiff>();
            }

            var changes = repo.Diff.Compare<TreeChanges>(baseCommit.Tree, headCommit.Tree);
            var fileDiffs = new List<FileDiff>();

            foreach (var change in changes)
            {
                var diff = repo.Diff.Compare<Patch>(baseCommit.Tree, headCommit.Tree, new[] { change.Path });
                var fileDiff = new FileDiff
                {
                    FilePath = change.Path,
                    ChangeType = MapChangeType(change.Status),
                    Diff = diff.Content,
                    LinesAdded = diff.LinesAdded,
                    LinesDeleted = diff.LinesDeleted
                };

                if (change.Status != ChangeKind.Deleted)
                {
                    fileDiff.NewContent = await GetFileContentAtCommitAsync(repo, headCommit, change.Path);
                }

                if (change.Status != ChangeKind.Added)
                {
                    fileDiff.OldContent = await GetFileContentAtCommitAsync(repo, baseCommit, change.Path);
                }

                fileDiffs.Add(fileDiff);
            }

            return fileDiffs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PR changes between {BaseBranch} and {HeadBranch} in repo {RepoPath}", baseBranch, headBranch, repoPath);
            return Enumerable.Empty<FileDiff>();
        }
    }

    public Task<IEnumerable<string>> GetModifiedFilesAsync(string repoPath)
    {
        try
        {
            using var repo = new Repository(repoPath);
            var status = repo.RetrieveStatus();

            var result = status.Where(s => s.State != FileStatus.Unaltered && s.State != FileStatus.Ignored)
                         .Select(s => s.FilePath)
                         .ToList();

            return Task.FromResult<IEnumerable<string>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modified files in repo {RepoPath}", repoPath);
            return Task.FromResult(Enumerable.Empty<string>());
        }
    }

    public async Task<string> GetFileContentAsync(string repoPath, string filePath)
    {
        try
        {
            var fullPath = Path.Combine(repoPath, filePath);
            if (File.Exists(fullPath))
            {
                return await File.ReadAllTextAsync(fullPath);
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FilePath} in repo {RepoPath}", filePath, repoPath);
            return string.Empty;
        }
    }

    public Task<CommitInfo> GetCommitInfoAsync(string repoPath, string commitHash)
    {
        try
        {
            using var repo = new Repository(repoPath);
            var commit = repo.Lookup<Commit>(commitHash);

            if (commit == null)
            {
                return Task.FromResult(new CommitInfo());
            }

            var stats = repo.Diff.Compare<PatchStats>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);

            var result = new CommitInfo
            {
                Hash = commit.Sha,
                Author = commit.Author.Name,
                Email = commit.Author.Email,
                Date = commit.Author.When.DateTime,
                Message = commit.Message,
                LinesAdded = stats.TotalLinesAdded,
                LinesDeleted = stats.TotalLinesDeleted,
                ModifiedFiles = commit.Parents.Any()
                    ? repo.Diff.Compare<TreeChanges>(commit.Parents.First().Tree, commit.Tree).Select(c => c.Path).ToList()
                    : new List<string>()
            };

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting commit info for {CommitHash} in repo {RepoPath}", commitHash, repoPath);
            return Task.FromResult(new CommitInfo());
        }
    }

    public bool IsGitRepository(string path)
    {
        try
        {
            return Repository.IsValid(path);
        }
        catch
        {
            return false;
        }
    }

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
            _logger.LogError(ex, "Error getting file content for {Path} at commit {CommitHash}", path, commit.Sha);
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