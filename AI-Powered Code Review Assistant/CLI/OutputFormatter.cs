using AI_Powered_Code_Review_Assistant.Models;
using Newtonsoft.Json;
using System.Text;

namespace AI_Powered_Code_Review_Assistant.CLI;

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

    private static string FormatJson(ReviewResult result)
    {
        return JsonConvert.SerializeObject(result, Formatting.Indented);
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

        html.AppendLine($"    <h1>Code Review Report</h1>");
        html.AppendLine($"    <p><strong>Generated:</strong> {result.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.AppendLine($"    <p><strong>Duration:</strong> {result.Duration.TotalSeconds:F2} seconds</p>");

        html.AppendLine("    <div class='summary'>");
        html.AppendLine($"        <h2>Summary</h2>");
        html.AppendLine($"        <p><strong>Overall Score:</strong> {result.Summary.OverallScore}</p>");
        html.AppendLine($"        <p><strong>Total Files:</strong> {result.Summary.TotalFiles}</p>");
        html.AppendLine($"        <p><strong>Files with Issues:</strong> {result.Summary.FilesWithIssues}</p>");
        html.AppendLine($"        <p><strong>Total Issues:</strong> {result.Summary.TotalIssues}</p>");
        html.AppendLine($"        <p><strong>Critical:</strong> {result.Summary.CriticalIssues}, <strong>High:</strong> {result.Summary.HighIssues}, <strong>Medium:</strong> {result.Summary.MediumIssues}, <strong>Low:</strong> {result.Summary.LowIssues}</p>");
        html.AppendLine("    </div>");

        foreach (var review in result.Reviews)
        {
            html.AppendLine("    <div class='file-review'>");
            html.AppendLine($"        <div class='file-header'>{review.FilePath}</div>");
            html.AppendLine("        <div>");

            foreach (var issue in review.Issues)
            {
                html.AppendLine($"            <div class='issue severity-{issue.Severity.ToString().ToLower()}'>");
                html.AppendLine($"                <h4>{issue.Title} ({issue.Severity})</h4>");
                html.AppendLine($"                <p>{issue.Description}</p>");
                if (issue.LineNumber.HasValue)
                {
                    html.AppendLine($"                <p><strong>Line:</strong> {issue.LineNumber}</p>");
                }
                html.AppendLine("            </div>");
            }

            foreach (var suggestion in review.Suggestions)
            {
                html.AppendLine("            <div class='suggestion'>");
                html.AppendLine($"                <h4>{suggestion.Title}</h4>");
                html.AppendLine($"                <p>{suggestion.Description}</p>");
                if (!string.IsNullOrEmpty(suggestion.RecommendedChange))
                {
                    html.AppendLine($"                <p><strong>Recommended:</strong> {suggestion.RecommendedChange}</p>");
                }
                html.AppendLine("            </div>");
            }

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static string FormatMarkdown(ReviewResult result)
    {
        var md = new StringBuilder();
        md.AppendLine("# Code Review Report");
        md.AppendLine();
        md.AppendLine($"**Generated:** {result.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        md.AppendLine($"**Duration:** {result.Duration.TotalSeconds:F2} seconds");
        md.AppendLine();

        md.AppendLine("## Summary");
        md.AppendLine();
        md.AppendLine($"- **Overall Score:** {result.Summary.OverallScore}");
        md.AppendLine($"- **Total Files:** {result.Summary.TotalFiles}");
        md.AppendLine($"- **Files with Issues:** {result.Summary.FilesWithIssues}");
        md.AppendLine($"- **Total Issues:** {result.Summary.TotalIssues}");
        md.AppendLine($"- **Critical:** {result.Summary.CriticalIssues} | **High:** {result.Summary.HighIssues} | **Medium:** {result.Summary.MediumIssues} | **Low:** {result.Summary.LowIssues}");
        md.AppendLine();

        if (result.Summary.TopRecommendations.Any())
        {
            md.AppendLine("### Top Recommendations");
            md.AppendLine();
            foreach (var recommendation in result.Summary.TopRecommendations)
            {
                md.AppendLine($"- {recommendation}");
            }
            md.AppendLine();
        }

        md.AppendLine("## File Reviews");
        md.AppendLine();

        foreach (var review in result.Reviews)
        {
            md.AppendLine($"### {review.FilePath}");
            md.AppendLine();

            if (review.Issues.Any())
            {
                md.AppendLine("#### Issues");
                md.AppendLine();
                foreach (var issue in review.Issues)
                {
                    var severityEmoji = issue.Severity switch
                    {
                        Severity.Critical => "🔴",
                        Severity.High => "🟠",
                        Severity.Medium => "🟡",
                        Severity.Low => "🟢",
                        _ => "ℹ️"
                    };

                    md.AppendLine($"**{severityEmoji} {issue.Title}** ({issue.Severity})");
                    md.AppendLine();
                    md.AppendLine(issue.Description);
                    if (issue.LineNumber.HasValue)
                    {
                        md.AppendLine($"*Line: {issue.LineNumber}*");
                    }
                    md.AppendLine();
                }
            }

            if (review.Suggestions.Any())
            {
                md.AppendLine("#### Suggestions");
                md.AppendLine();
                foreach (var suggestion in review.Suggestions)
                {
                    md.AppendLine($"**💡 {suggestion.Title}**");
                    md.AppendLine();
                    md.AppendLine(suggestion.Description);
                    if (!string.IsNullOrEmpty(suggestion.RecommendedChange))
                    {
                        md.AppendLine();
                        md.AppendLine($"*Recommended: {suggestion.RecommendedChange}*");
                    }
                    md.AppendLine();
                }
            }

            md.AppendLine("---");
            md.AppendLine();
        }

        return md.ToString();
    }
}