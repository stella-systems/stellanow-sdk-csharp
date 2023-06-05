using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class StellaNowExtensions
{
    public static IServiceCollection AddStellaNowSdk(
        this IServiceCollection services, 
        string organizationId, string projectId,
        string brokerUrl, string clientId, string username, string password, string topic)
    {
        services.AddSingleton<ILogger<StellaNowSdk>, Logger<StellaNowSdk>>();
        services.AddSingleton<ILogger<StellaNowMessageQueue>, Logger<StellaNowMessageQueue>>();
        services.AddSingleton<ILogger<StellaNowMqttWebSocketStrategy>, Logger<StellaNowMqttWebSocketStrategy>>();
        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        
        services.AddSingleton<IStellaNowConnectionStrategy>(
            serviceProvider => new StellaNowMqttWebSocketStrategy(
                serviceProvider.GetRequiredService<ILogger<StellaNowMqttWebSocketStrategy>>(),
                brokerUrl, clientId, username, password, topic)
        );

        services.AddSingleton<IStellaNowMessageQueue>(
            serviceProvider => new StellaNowMessageQueue(
                serviceProvider.GetRequiredService<ILogger<StellaNowMessageQueue>>(),
                serviceProvider.GetRequiredService<IMessageQueueStrategy>(),
                serviceProvider.GetRequiredService<IStellaNowConnectionStrategy>())
        );

        services.AddSingleton<IStellaNowSdk>(
            serviceProvider => new StellaNowSdk(
                serviceProvider.GetRequiredService<ILogger<StellaNowSdk>>(),
                serviceProvider.GetRequiredService<IStellaNowConnectionStrategy>(),
                serviceProvider.GetRequiredService<IStellaNowMessageQueue>(),
                organizationId, projectId)
        );

        return services;
    }
}