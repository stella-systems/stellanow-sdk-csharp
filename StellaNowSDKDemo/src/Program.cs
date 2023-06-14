using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellaNowSDK.Config;
using StellaNowSDK.Enums;
using StellaNowSDK.Services;
using StellaNowSDKDemo.Messages;

namespace StellaNowSDKDemo;

internal class Program
{
    private static IServiceProvider _serviceProvider;
    
    static async Task Main(string[] args)
    {
        RegisterServices();

        var stellaSdk = _serviceProvider.GetRequiredService<IStellaNowSdk>();
        var logger = _serviceProvider.GetRequiredService<ILogger<Program>>();

        await stellaSdk.StartAsync();

        // Ensure connection is established
        await Task.Delay(TimeSpan.FromSeconds(5));

        while (true)
        {
            var uuid = Guid.NewGuid().ToString();

            var message = new UserLoginMessage(
                uuid, uuid,
                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            );

            // Send the message
            stellaSdk.SendMessage(message);
            logger.LogInformation("New Message Queued");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        
        DisposeServices();
    }
    
    private static void RegisterServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
            });
            builder.SetMinimumLevel(LogLevel.Debug);
        });

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