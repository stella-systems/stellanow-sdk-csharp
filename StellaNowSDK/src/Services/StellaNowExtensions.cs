using StellaNowSDK.Authentication;
using StellaNowSDK.Config;
using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Enums;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class StellaNowExtensions
{
    public static IServiceCollection AddStellaNowSdk(
        this IServiceCollection services,
        StellaNowEnvironment environment, 
        StellaNowConfig config)
    {
        switch (environment)
        {
            case StellaNowEnvironment.Development:
                services.AddSingleton<StellaNowEnvironmentConfig, StellaNowDevEnvironmentConfig>();
                break;
            case StellaNowEnvironment.Integrations:
                services.AddSingleton<StellaNowEnvironmentConfig, StellaNowIntEnvironmentConfig>();
                break;
        }

        services.AddSingleton(config);
        
        services.AddSingleton<ILogger<StellaNowSdk>, Logger<StellaNowSdk>>();
        services.AddSingleton<StellaNowAuthenticationService>();
        services.AddSingleton<ILogger<StellaNowMessageQueue>, Logger<StellaNowMessageQueue>>();
        services.AddSingleton<ILogger<StellaNowMqttWebSocketStrategy>, Logger<StellaNowMqttWebSocketStrategy>>();
        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        
        services.AddSingleton<IStellaNowConnectionStrategy, StellaNowMqttWebSocketStrategy>();
        services.AddSingleton<IStellaNowMessageQueue, StellaNowMessageQueue>();
        services.AddSingleton<IStellaNowSdk, StellaNowSdk>();

        return services;
    }
}