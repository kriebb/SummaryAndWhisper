// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kriebbels.SpeechtToText.Console;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var consoleAppBuilder = new ConsoleAppBuilder(args);
        var app = consoleAppBuilder.Build();
        var logger = app.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting application...");
        await app.StartAsync();
        
        logger.LogInformation("Press any key to stop...");
        System.Console.ReadKey();
        await app.StopAsync();
    }
}