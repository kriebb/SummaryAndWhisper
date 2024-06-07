using Kriebbels.SpeechtToText.Console;
using Microsoft.Extensions.DependencyInjection;

namespace Kriebbels.SpeechToText.Tests;

public class ConsoleAppBuilderTests
{
    [Fact]
    public void ServiceProviderIsRegisteredAndValid()
    {
        // Arrange
        var consoleAppBuilder = new ConsoleAppBuilder(new string[0]);
        consoleAppBuilder.ConfigureServices(services => { });
        var services = consoleAppBuilder.Services;
        var app = consoleAppBuilder.Build();
        var serviceProvider = app.ServiceProvider;

        // Act & Assert
        foreach (var service in services)
        {
            var serviceInstance = serviceProvider.GetService(service.ServiceType);
            Assert.NotNull(serviceInstance);
        }
    }
}