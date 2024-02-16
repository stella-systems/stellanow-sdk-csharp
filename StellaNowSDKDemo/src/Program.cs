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
    private static CancellationTokenSource _cts = new CancellationTokenSource();
    
    private static IServiceProvider? _serviceProvider;
    private readonly ILogger<Program> _logger;
    private readonly IStellaNowSdk _stellaSdk;
    
    public Program(IStellaNowSdk stellaSdk, ILogger<Program> logger)
    {
        _stellaSdk = stellaSdk;
        _logger = logger;
    }

    private async Task RunAsync(int? messageCount)
    {
        // Establish connection
        await _stellaSdk.StartAsync();

        _logger.LogInformation("Press 'ENTER' to stop the application!");
        
        // Ensure connection is established
        await Task.Delay(TimeSpan.FromSeconds(5));

        var uuid = Guid.NewGuid().ToString();

        int i = 0;

        // Either send messages based on count or indefinitely
        while (!messageCount.HasValue || i < messageCount.Value)
        {
            if (_cts.IsCancellationRequested)
            {
                break; // Exit the loop if cancellation is requested
            }
            var message = new UserLoginMessage(
                uuid,
                DateTime.UtcNow,
                2
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
            
            i++;
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
                ApiKey = "<YOUR-API-KEY>",
                ApiSecret = "<YOUR-API-SECRET>",
                ClientId = "StellaNowSDK",
                OrganizationId = "<YOUR-ORGANIZATION-UUID>",
                ProjectId = "<YOUR-PROJECT-UUID>"
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
        int? messageCount = null;
        if (args.Length > 0 && int.TryParse(args[0], out int count))
        {
            messageCount = count;
        }

        RegisterServices();

        // This task waits for Enter key and then cancels the message sending
        Task.Run(() =>
        {
            Console.ReadLine();
            _cts.Cancel();
        });

        var program = _serviceProvider?.GetRequiredService<Program>();
        await program?.RunAsync(messageCount)!;

        DisposeServices();
    }
}