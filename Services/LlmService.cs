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

    public async Task<string> AnalyzeSecurityVulnerabilitiesAsync(string code, string fileName)
    {
        var prompt = $@"
You are a security expert. Analyze the following code for security vulnerabilities.

File: {fileName}

Code:
```
{code}
```

Focus specifically on:
1. Injection vulnerabilities (SQL, XSS, Command injection, etc.)
2. Authentication and authorization issues
3. Input validation problems
4. Cryptographic issues
5. Information disclosure
6. Insecure configurations
7. OWASP Top 10 vulnerabilities

Provide specific recommendations to fix any issues found. Rate severity as Critical, High, Medium, Low, or Info.";

        return await CallOpenAIAsync(prompt);
    }

    public async Task<string> CheckCodeQualityAsync(string code, string fileName)
    {
        var prompt = $@"
You are a code quality expert. Analyze the following code for quality issues.

File: {fileName}

Code:
```
{code}
```

Evaluate:
1. Code complexity and maintainability
2. Naming conventions
3. Code organization and structure
4. Documentation and comments
5. Error handling
6. Testing considerations
7. Design patterns usage
8. SOLID principles adherence

Provide actionable recommendations for improvement.";

        return await CallOpenAIAsync(prompt);
    }

    public async Task<string> SuggestImprovementsAsync(string code, string fileName)
    {
        var prompt = $@"
You are a senior software engineer. Suggest improvements for the following code.

File: {fileName}

Code:
```
{code}
```

Provide suggestions for:
1. Performance optimizations
2. Code refactoring opportunities
3. Modern language features that could be used
4. Better design patterns
5. Improved error handling
6. Enhanced readability
7. Resource management

Include specific code examples where helpful.";

        return await CallOpenAIAsync(prompt);
    }

    public async Task<string> AnalyzePullRequestAsync(string changes, string description)
    {
        var prompt = $@"
You are reviewing a pull request. Analyze the changes and provide feedback.

Pull Request Description: {description}

Changes:
```
{changes}
```

Please provide:
1. Overall assessment of the changes
2. Potential impact analysis
3. Breaking changes identification
4. Testing recommendations
5. Documentation requirements
6. Deployment considerations
7. Risk assessment

Provide a summary recommendation: Approve, Request Changes, or Needs Discussion.";

        return await CallOpenAIAsync(prompt);
    }

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