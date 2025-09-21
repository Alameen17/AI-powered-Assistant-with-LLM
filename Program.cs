using AI_Powered_Code_Review_Assistant.CLI;
using AI_Powered_Code_Review_Assistant.Interfaces;
using AI_Powered_Code_Review_Assistant.Services;

var builder = WebApplication.CreateBuilder(args);

// Check if running as CLI (has command line arguments)
var isCliMode = args.Length > 0;

if (isCliMode)
{
    // CLI Mode - run as console application
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
}
else
{
    // Web Mode - run as Blazor Server application

    // Add services to the container
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddSignalR();

    // Register our custom services
    builder.Services.AddTransient<ILlmService, LlmService>();
    builder.Services.AddTransient<IGitService, GitService>();
    builder.Services.AddTransient<ICodeReviewService, CodeReviewService>();

    var openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (string.IsNullOrEmpty(openAiApiKey))
    {
        Console.WriteLine("⚠️  OpenAI API key not configured. Web interface will have limited functionality.");
        Console.WriteLine("   Configure using environment variable OPENAI_API_KEY or appsettings.json");
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
}
