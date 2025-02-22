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

public static class StellaNowExtensions
{
    public static IServiceCollection AddStellaNowSdkWithMqttAndOidcAuth(
        this IServiceCollection services,
        StellaNowEnvironmentConfig environmentConfig, 
        StellaNowConfig stellaNowConfig,
        OidcAuthCredentials authCredentials)
    {
        services.AddSingleton(environmentConfig);
        services.AddSingleton(stellaNowConfig);
        services.AddSingleton(authCredentials);
        
        services.AddSingleton<ILogger<StellaNowSdk>, Logger<StellaNowSdk>>();
        services.AddSingleton<ILogger<StellaNowMessageQueue>, Logger<StellaNowMessageQueue>>();
        services.AddSingleton<ILogger<StellaNowMqttSink>, Logger<StellaNowMqttSink>>();
        
        services.AddHttpClient();
        services.AddSingleton<StellaNowAuthenticationService>();

        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        services.AddSingleton<IMqttConnectionStrategy, OidcMqttConnectionStrategy>();
        services.AddSingleton<IStellaNowSink, StellaNowMqttSink>();
        services.AddSingleton<IStellaNowMessageQueue, StellaNowMessageQueue>();
        services.AddSingleton<IStellaNowSdk, StellaNowSdk>();

        return services;
    }
    
    public static IServiceCollection AddStellaNowSdkWithMqttAndNoAuth(
        this IServiceCollection services,
        StellaNowEnvironmentConfig environmentConfig, 
        StellaNowConfig config)
    {
        services.AddSingleton(environmentConfig);
        services.AddSingleton(config);
        
        services.AddSingleton<ILogger<StellaNowSdk>, Logger<StellaNowSdk>>();
        services.AddSingleton<ILogger<StellaNowMessageQueue>, Logger<StellaNowMessageQueue>>();
        services.AddSingleton<ILogger<StellaNowMqttSink>, Logger<StellaNowMqttSink>>();
        
        services.AddHttpClient();

        services.AddSingleton<IMessageQueueStrategy, FifoMessageQueueStrategy>();
        services.AddSingleton<IMqttConnectionStrategy, NoAuthMqttConnectionStrategy>();
        services.AddSingleton<IStellaNowSink, StellaNowMqttSink>();
        services.AddSingleton<IStellaNowMessageQueue, StellaNowMessageQueue>();
        services.AddSingleton<IStellaNowSdk, StellaNowSdk>();

        return services;
    }
}    