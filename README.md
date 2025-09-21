# AI-Powered Code Review Assistant

A comprehensive .NET application that leverages Large Language Models (LLMs) to perform automated code reviews, security analysis, and quality assessments.

## Features

### Core Capabilities
- **Commit Review**: Analyze specific git commits for code quality and security issues
- **Pull Request Review**: Compare branches and provide comprehensive PR feedback
- **File Review**: Review individual files or groups of files
- **Directory Review**: Scan entire directories for code issues

### Analysis Types
- **Security Vulnerability Detection**: Identify potential security risks and vulnerabilities
- **Code Quality Analysis**: Assess maintainability, complexity, and best practices
- **Performance Analysis**: Suggest optimizations and performance improvements
- **Best Practices**: Ensure adherence to coding standards and patterns

### Output Formats
- **JSON**: Structured data for integration with other tools
- **HTML**: Rich, formatted reports for web viewing
- **Markdown**: Documentation-friendly format for repositories

## Quick Start

### Prerequisites
- .NET 9.0 or later
- Git (for repository analysis)
- OpenAI API key

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd AI-Powered-Code-Review-Assistant
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Configure OpenAI API key:
```bash
dotnet run -- configure --openai-key YOUR_API_KEY
```

### Basic Usage

#### Review a Git Commit
```bash
dotnet run -- review-commit --repo /path/to/repo --commit abc123def --format markdown
```

#### Review a Pull Request
```bash
dotnet run -- review-pr --repo /path/to/repo --base main --head feature-branch --format html --output report.html
```

#### Review Specific Files
```bash
dotnet run -- review-files --repo /path/to/repo --files "src/file1.cs,src/file2.cs" --format json
```

#### Review Directory
```bash
dotnet run -- review-directory --directory /path/to/source --format markdown --output review.md
```

## Configuration

### OpenAI Settings
- **API Key**: Set via `configure` command or `OPENAI_API_KEY` environment variable
- **Model**: Default is `gpt-4`, can be changed via `configure --openai-model`

### File Filtering
The application automatically excludes:
- Binary files (`.exe`, `.dll`, `.bin`, etc.)
- Image files (`.png`, `.jpg`, etc.)
- Build directories (`bin`, `obj`, `node_modules`, etc.)
- Version control directories (`.git`, `.vs`, etc.)

## Architecture

### Core Components

#### Services
- **LlmService**: Handles OpenAI API integration and AI-powered analysis
- **GitService**: Manages git repository operations and diff analysis
- **CodeReviewService**: Orchestrates the review process and generates results

#### Models
- **FileDiff**: Represents changes in files
- **Review**: Contains analysis results for individual files
- **ReviewResult**: Aggregates all reviews and provides summary statistics
- **Issue/Suggestion**: Represents specific findings and recommendations

#### CLI Interface
- **CliHandler**: Processes command-line arguments and coordinates operations
- **OutputFormatter**: Formats results in JSON, HTML, or Markdown

### Key Features

#### Security Analysis
- Injection vulnerability detection (SQL, XSS, Command injection)
- Authentication and authorization issues
- Input validation problems
- Cryptographic vulnerabilities
- OWASP Top 10 coverage

#### Code Quality Metrics
- Complexity analysis
- Maintainability assessment
- Naming convention checks
- Design pattern usage
- SOLID principles adherence

#### Performance Analysis
- Resource management issues
- Optimization opportunities
- Memory leak detection
- Algorithm efficiency suggestions

## Example Output

### Summary Statistics
- Overall code quality score (A-F scale)
- Total files analyzed
- Issues by severity (Critical, High, Medium, Low, Info)
- Security, Performance, and Quality issue counts
- Top recommendations

### Detailed Analysis
- File-by-file breakdown
- Specific line number references
- Issue descriptions and severity ratings
- Actionable improvement suggestions
- Code snippets and examples

## Advanced Usage

### Custom Configuration
Edit `appsettings.json` to customize:
- File size limits
- Supported file extensions
- Exclusion patterns
- OpenAI model settings

### Integration with CI/CD
The tool can be integrated into build pipelines:
```bash
# Exit with non-zero code if critical issues found
dotnet run -- review-commit --repo . --commit $GIT_COMMIT --format json --output review.json
```

### Batch Processing
Review multiple commits or branches in sequence for trend analysis.

## Development

### Project Structure
```
├── Services/           # Core business logic
├── Interfaces/         # Service contracts
├── Models/            # Data models
├── CLI/               # Command-line interface
├── Configuration/     # Settings and config
└── Tests/             # Unit tests
```

### Adding New Analysis Types
1. Extend `ILlmService` with new analysis methods
2. Update `LlmService` implementation
3. Modify `CodeReviewService` to incorporate new analysis
4. Add CLI options if needed

### Testing
Run the included tests:
```bash
dotnet run -- test
```

## Troubleshooting

### Common Issues

**"OpenAI API key not found"**
- Set the API key using the configure command or environment variable

**"Not a valid git repository"**
- Ensure the path points to a directory containing a `.git` folder

**"No changes found"**
- Verify the commit hash exists and has file changes

**Performance Issues**
- Large files or repositories may take time to analyze
- Consider reviewing specific files instead of entire directories

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or contributions, please open an issue on the project repository.