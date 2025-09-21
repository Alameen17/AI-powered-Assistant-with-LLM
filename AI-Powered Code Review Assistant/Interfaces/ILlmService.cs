namespace AI_Powered_Code_Review_Assistant.Interfaces;

public interface ILlmService
{
    Task<string> ReviewCodeAsync(string code, string context, string fileName);
    Task<string> AnalyzeSecurityVulnerabilitiesAsync(string code, string fileName);
    Task<string> CheckCodeQualityAsync(string code, string fileName);
    Task<string> SuggestImprovementsAsync(string code, string fileName);
    Task<string> AnalyzePullRequestAsync(string changes, string description);
}