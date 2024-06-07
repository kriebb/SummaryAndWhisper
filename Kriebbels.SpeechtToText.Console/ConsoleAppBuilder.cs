using System.Collections;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kriebbels.SpeechtToText.Console;

public class ConsoleAppBuilder(string[] args)
{
    private readonly IConfiguration _configuration = new ConfigurationManager()
        .AddJsonFile("appsettings.json", false)
        .AddUserSecrets<Program>(false, true)
        .AddEnvironmentVariables()
        .AddCommandLine(args).Build();

    public IServiceCollection Services { get; } = new ServiceCollection();

    public void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        Services.AddSingleton(_configuration);
        Services.AddSingleton<OpenAiClientProvider>();
        Services.AddSingleton<IAudioClientProvider,AudioClientProvider>();
        Services.AddSingleton<IFileService, FileService>();
        Services.AddSingleton<IAudioProcessingService,AudioProcessingService>();

        Services.AddSingleton<ITranscriptionService,TranscriptionService>();
        Services.AddSingleton<IAudioRecordingService,AudioRecordingService>();
        Services.AddSingleton<ISilenceDetector, SilenceDetector>();
        Services.AddSingleton<IAudioSegmenter, AudioSegmenter>();
        Services.AddSingleton<IAudioFileTrimmer, AudioFileTrimmer>();
        
        Services.AddSingleton<ConsoleApp>();
            
        Services.AddLogging(builder =>
            {
                builder.AddSystemdConsole();
                builder.AddFilter(level => level == LogLevel.Debug);

            }
            
        );

        Services.AddOptions();
        
        configureServices(Services);
        
    }
    public ConsoleApp Build()
    {

        var provider = Services.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        
        
        var app = provider.GetRequiredService<ConsoleApp>();

        return app;
    }
}