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
using StellaNowSDK.Messages;
using StellaNowSDK.Services;
using StellaNowSDKDemo.Messages;

namespace StellaNowSDKDemo;

internal class Program
{
    private const int MessageCount = 1;
    
    private static IServiceProvider? _serviceProvider;
    private readonly ILogger<Program> _logger;
    private readonly IStellaNowSdk _stellaSdk;
    
    public Program(IStellaNowSdk stellaSdk, ILogger<Program> logger)
    {
        _stellaSdk = stellaSdk;
        _logger = logger;
    }

    private async Task RunAsync()
    {
        // Establish connection
        await _stellaSdk.StartAsync();

        // Ensure connection is established
        await Task.Delay(TimeSpan.FromSeconds(5));

        var uuid = Guid.NewGuid().ToString();
        
        // Send 5 messages
        for (int i = 0; i < MessageCount; i++)
        {
            var message = new UserLoginMessage(
                uuid,
                DateTime.UtcNow,
                2,
                uuid
            );

            // Send the message
            _stellaSdk.SendMessage(message, OnMessageSentAction);
            _logger.LogInformation("New Message Queued");

            _logger.LogInformation(
                "Queue has messages {HasMessages} and count is {Count}",
                _stellaSdk.HasMessagesPendingForDispatch(),
                _stellaSdk.MessagesPendingForDispatchCount()
            );
            
            // Delay between messages
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        // Disconnect and terminate
        await _stellaSdk.StopAsync();
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
        
        services.AddTransient<Program>();

        // Register StellaNowSdk with necessary configurations and environment.
        services.AddStellaNowSdk(
            StellaNowEnvironment.Integration,
            new StellaNowConfig
            {
                ApiKey = "username10@some.domain",
                ApiSecret = "1234567890",
                ClientId = "StellaNowSDK",
                OrganizationId = "e24f2940-04b0-493b-a28c-d809b2399382",
                ProjectId = "9e9f347a-b7e8-4221-a7b3-05111b3eb40e"
            }
        );

        // Build the service provider from the service collection.
        _serviceProvider = services.BuildServiceProvider();
    }

    private void OnMessageSentAction(StellaNowEventWrapper message)
    {
        _logger.LogInformation(
            "Send Confirmation: {MessagesId}",
            message.Value.Metadata.MessageId
        );
    }

    private static void DisposeServices()
    {
        switch (_serviceProvider)
        {
            case null:
                return;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
    
    static async Task Main(string[] args)
    {
        RegisterServices();

        var program = _serviceProvider?.GetRequiredService<Program>();
        await program?.RunAsync()!;

        DisposeServices();
    }
}