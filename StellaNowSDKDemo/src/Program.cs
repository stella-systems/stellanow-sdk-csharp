using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellaNowSDK.Config;
using StellaNowSDK.Enums;
using StellaNowSDK.Services;
using StellaNowSDKDemo.Messages;

namespace StellaNowSDKDemo;

internal class Program
{
    private const int MessageCount = 10;
    
    private static IServiceProvider _serviceProvider;
    
    static async Task Main(string[] args)
    {
        RegisterServices();

        var stellaSdk = _serviceProvider.GetRequiredService<IStellaNowSdk>();
        var logger = _serviceProvider.GetRequiredService<ILogger<Program>>();

        // Establish connection
        await stellaSdk.StartAsync();

        // Ensure connection is established
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Send 5 messages
        for (int i = 0; i < MessageCount; i++)
        {
            var uuid = Guid.NewGuid().ToString();

            var message = new UserLoginMessage(
                uuid, uuid,
                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            );

            // Send the message
            stellaSdk.SendMessage(message);
            logger.LogInformation("New Message Queued");

            // Delay between messages
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        // Disconnect and terminate
        await stellaSdk.StopAsync();
        DisposeServices();
    }

    private static void RegisterServices()
    {
        // Create a new instance of ServiceCollection to register application services.
        var services = new ServiceCollection();

        // Configure logging to use a simple console logger and set the minimum log level.
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
            });
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Register StellaNowSdk with necessary configurations and environment.
        services.AddStellaNowSdk(
            StellaNowEnvironment.Integrations,
            new StellaNowConfig
            {
                ApiKey = "username10@some.domain",
                ApiSecret = "1234567890",
                ClientId = "StellaNowCsharpSDK",
                OrganizationId = "62dbd729-54c0-43cd-9282-1828424f0873",
                ProjectId = "9569c762-a633-4606-b2a0-c05b4fbac542"
            }
        );

        // Build the service provider from the service collection.
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void DisposeServices()
    {
        if (_serviceProvider == null)
        {
            return;
        }
        
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}