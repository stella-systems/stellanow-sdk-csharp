// Copyright (C) 2023-2025 Stella Technologies (UK) Limited.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvironmentConfig;
using StellaNowSDK.Sinks;
using StellaNowSDK.Sinks.Mqtt;
using StellaNowSDK.Queue;
using StellaNowSDK.Sinks.Mqtt.AuthStrategy;

namespace StellaNowSDK.Services;

/// <summary>
/// Provides extension methods for registering StellaNow SDK services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class StellaNowExtensions
{
    /// <summary>
    /// Registers StellaNow SDK services with MQTT connectivity using OIDC authentication.
    /// </summary>
    /// <remarks>
    /// This method adds the necessary configurations and dependencies to the 
    /// <see cref="IServiceCollection"/> to enable OIDC-based authentication for publishing 
    /// messages via MQTT. It sets up <see cref="OidcMqttAuthStrategy"/> for broker connection, and 
    /// <see cref="StellaNowSdk"/> as the primary entry point for sending messages.
    /// </remarks>
    /// <param name="services">The DI services collection to which StellaNow services will be added.</param>
    /// <param name="environmentConfig">
    /// The environment configuration specifying API and broker URLs (e.g., dev or prod).
    /// </param>
    /// <param name="stellaNowConfig">
    /// Contains organization and project IDs for the StellaNow platform.
    /// </param>
    /// <param name="authCredentials">
    /// The OIDC username/password credentials used for direct grant authentication.
    /// </param>
    /// <param name="clientId">The optional MQTT client ID. If null or empty, a random ID will be generated.</param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/>, allowing for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stellaNowConfig.organizationId"/> or <paramref name="environmentConfig.BrokerUrl"/> is null or empty.</exception>
    public static IServiceCollection AddStellaNowSdkWithMqttAndOidcAuth(
        this IServiceCollection services,
        StellaNowEnvironmentConfig environmentConfig,
        StellaNowConfig stellaNowConfig,
        OidcAuthCredentials authCredentials,
        string? clientId = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environmentConfig);
        ArgumentNullException.ThrowIfNull(stellaNowConfig);
        ArgumentNullException.ThrowIfNull(authCredentials);
        ArgumentException.ThrowIfNullOrWhiteSpace(stellaNowConfig.organizationId, nameof(stellaNowConfig.organizationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentConfig.BrokerUrl, nameof(environmentConfig.BrokerUrl));

        // Register environment, config, and credentials
        services.AddSingleton(environmentConfig);
        services.AddSingleton(stellaNowConfig);
        services.AddSingleton(authCredentials);

        // Register loggers
        services.AddSingleton<ILogger<StellaNowSdk>, Logger<StellaNowSdk>>();
        services.AddSingleton<ILogger<StellaNowMessageQueue>, Logger<StellaNowMessageQueue>>();
        services.AddSingleton<ILogger<StellaNowMqttSink>, Logger<StellaNowMqttSink>>();

        // Register HttpClient for token and discovery document requests
        services.AddHttpClient();

        // Register queue and sink strategies
        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        services.AddSingleton<IMqttAuthStrategy, OidcMqttAuthStrategy>();
        services.AddSingleton<IStellaNowSink>(sp => new StellaNowMqttSink(
            sp.GetRequiredService<ILogger<StellaNowMqttSink>>(),
            sp.GetRequiredService<IMqttAuthStrategy>(),
            sp.GetRequiredService<StellaNowConfig>(),
            sp.GetRequiredService<StellaNowEnvironmentConfig>(),
            clientId));
        services.AddSingleton<IStellaNowMessageQueue, StellaNowMessageQueue>();

        // Register the main SDK
        services.AddSingleton<IStellaNowSdk, StellaNowSdk>();

        return services;
    }

    /// <summary>
    /// Registers StellaNow SDK services with MQTT connectivity using no authentication.
    /// </summary>
    /// <remarks>
    /// This method is intended for scenarios where no authentication is required (e.g., 
    /// local development with NanoMQ). It sets up <see cref="NoAuthMqttAuthStrategy"/> 
    /// for broker connection and <see cref="StellaNowSdk"/> as the main entry point for sending messages.
    /// </remarks>
    /// <param name="services">The DI services collection to which StellaNow services will be added.</param>
    /// <param name="environmentConfig">
    /// The environment configuration specifying API and broker URLs (e.g., local or dev).
    /// </param>
    /// <param name="config">
    /// Contains organization and project IDs for the StellaNow platform.
    /// </param>
    /// <param name="clientId">The optional MQTT client ID. If null or empty, a random ID will be generated.</param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/>, allowing for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="config.organizationId"/> or <paramref name="environmentConfig.BrokerUrl"/> is null or empty.</exception>
    public static IServiceCollection AddStellaNowSdkWithMqttAndNoAuth(
        this IServiceCollection services,
        StellaNowEnvironmentConfig environmentConfig,
        StellaNowConfig config,
        string? clientId = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environmentConfig);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrWhiteSpace(config.organizationId, nameof(config.organizationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentConfig.BrokerUrl, nameof(environmentConfig.BrokerUrl));

        // Register environment and config
        services.AddSingleton(environmentConfig);
        services.AddSingleton(config);

        // Register loggers
        services.AddSingleton<ILogger<StellaNowSdk>, Logger<StellaNowSdk>>();
        services.AddSingleton<ILogger<StellaNowMessageQueue>, Logger<StellaNowMessageQueue>>();
        services.AddSingleton<ILogger<StellaNowMqttSink>, Logger<StellaNowMqttSink>>();

        // Register queue and sink strategies
        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        services.AddSingleton<IMqttAuthStrategy, NoAuthMqttAuthStrategy>();
        services.AddSingleton<IStellaNowSink>(sp => new StellaNowMqttSink(
            sp.GetRequiredService<ILogger<StellaNowMqttSink>>(),
            sp.GetRequiredService<IMqttAuthStrategy>(),
            sp.GetRequiredService<StellaNowConfig>(),
            sp.GetRequiredService<StellaNowEnvironmentConfig>(),
            clientId));
        services.AddSingleton<IStellaNowMessageQueue, StellaNowMessageQueue>();

        // Register the main SDK
        services.AddSingleton<IStellaNowSdk, StellaNowSdk>();

        return services;
    }
}