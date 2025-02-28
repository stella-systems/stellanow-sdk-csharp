// Copyright (C) 2022-2025 Stella Technologies (UK) Limited.
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
using StellaNowSDK.Authentication;
using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvirnmentConfig;
using StellaNowSDK.Sinks;
using StellaNowSDK.Sinks.Mqtt;
using StellaNowSDK.Queue;
using StellaNowSDK.Sinks.Mqtt.ConnectionStrategy;

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
    /// messages via MQTT. It sets up <see cref="StellaNowAuthenticationService"/> for token retrieval, 
    /// <see cref="OidcMqttConnectionStrategy"/> for broker connection, and 
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
    /// <returns>
    /// The modified <see cref="IServiceCollection"/>, allowing for method chaining.
    /// </returns>
    public static IServiceCollection AddStellaNowSdkWithMqttAndOidcAuth(
        this IServiceCollection services,
        StellaNowEnvironmentConfig environmentConfig, 
        StellaNowConfig stellaNowConfig,
        OidcAuthCredentials authCredentials)
    {
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

        // Register authentication service
        services.AddSingleton<StellaNowAuthenticationService>();

        // Register queue and sink strategies
        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        services.AddSingleton<IMqttConnectionStrategy, OidcMqttConnectionStrategy>();
        services.AddSingleton<IStellaNowSink, StellaNowMqttSink>();
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
    /// local development with NanoMQ). It sets up <see cref="NoAuthMqttConnectionStrategy"/> 
    /// for broker connection and <see cref="StellaNowSdk"/> as the main entry point for sending messages.
    /// </remarks>
    /// <param name="services">The DI services collection to which StellaNow services will be added.</param>
    /// <param name="environmentConfig">
    /// The environment configuration specifying API and broker URLs (e.g., local or dev).
    /// </param>
    /// <param name="config">
    /// Contains organization and project IDs for the StellaNow platform.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/>, allowing for method chaining.
    /// </returns>
    public static IServiceCollection AddStellaNowSdkWithMqttAndNoAuth(
        this IServiceCollection services,
        StellaNowEnvironmentConfig environmentConfig, 
        StellaNowConfig config)
    {
        // Register environment and config
        services.AddSingleton(environmentConfig);
        services.AddSingleton(config);
        
        // Register loggers
        services.AddSingleton<ILogger<StellaNowSdk>, Logger<StellaNowSdk>>();
        services.AddSingleton<ILogger<StellaNowMessageQueue>, Logger<StellaNowMessageQueue>>();
        services.AddSingleton<ILogger<StellaNowMqttSink>, Logger<StellaNowMqttSink>>();

        // Register queue and sink strategies
        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        services.AddSingleton<IMqttConnectionStrategy, NoAuthMqttConnectionStrategy>();
        services.AddSingleton<IStellaNowSink, StellaNowMqttSink>();
        services.AddSingleton<IStellaNowMessageQueue, StellaNowMessageQueue>();

        // Register the main SDK
        services.AddSingleton<IStellaNowSdk, StellaNowSdk>();

        return services;
    }
}    