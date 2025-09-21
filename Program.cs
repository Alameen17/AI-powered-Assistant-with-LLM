using AI_Powered_Code_Review_Assistant.CLI;
using AI_Powered_Code_Review_Assistant.Interfaces;
using AI_Powered_Code_Review_Assistant.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
});

var openAiApiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(openAiApiKey))
{
    Console.WriteLine("⚠️  OpenAI API key not found. Please configure it using:");
    Console.WriteLine("   dotnet run -- configure --openai-key YOUR_API_KEY");
    Console.WriteLine("   or set the OPENAI_API_KEY environment variable");
    Console.WriteLine();
}

services.AddTransient<ILlmService, LlmService>();
services.AddTransient<IGitService, GitService>();
services.AddTransient<ICodeReviewService, CodeReviewService>();
services.AddTransient<CliHandler>();

var serviceProvider = services.BuildServiceProvider();

try
{
    var cliHandler = serviceProvider.GetRequiredService<CliHandler>();
    var exitCode = await cliHandler.HandleArgumentsAsync(args);
    Environment.Exit(exitCode);
}
catch (Exception ex)
{
    var logger = serviceProvider.GetService<ILogger<Program>>();
    logger?.LogError(ex, "Application error");
    Console.WriteLine($"Fatal error: {ex.Message}");
    Environment.Exit(1);
}
