// Copyright (C) 2022-2023 Stella Technologies (UK) Limited.
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
            StellaNowEnvironment.Integration,
            new StellaNowConfig
            {
                ApiKey = "username10@some.domain",
                ApiSecret = "1234567890",
                ClientId = "StellaNowSDK",
                OrganizationId = "62dbd729-54c0-43cd-9282-1828424f0873",
                ProjectId = "0a1aae1e-b798-4a8a-a9f7-6aa521c6209d"
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