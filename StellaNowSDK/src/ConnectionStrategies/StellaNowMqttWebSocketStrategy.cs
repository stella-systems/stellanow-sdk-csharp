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

using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using StellaNowSDK.Authentication;
using StellaNowSDK.Config;
using StellaNowSDK.Events;
using StellaNowSDK.Messages;

namespace StellaNowSDK.ConnectionStrategies;

public sealed class StellaNowMqttWebSocketStrategy : IStellaNowConnectionStrategy, IDisposable
{
    private ILogger<StellaNowMqttWebSocketStrategy>? _logger;

    private readonly IMqttClient _mqttClient;

    private readonly StellaNowAuthenticationService _authService;

    private readonly StellaNowConfig _config;
    private readonly StellaNowEnvironmentConfig _envConfig; 
    
    private readonly string _topic;
    
    public bool IsConnected => _mqttClient.IsConnected;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    private CancellationTokenSource? _reconnectCancellationTokenSource;
    
    public StellaNowMqttWebSocketStrategy(
        ILogger<StellaNowMqttWebSocketStrategy>? logger,
        StellaNowAuthenticationService authService,
        StellaNowEnvironmentConfig envConfig,
        StellaNowConfig config)
    {
        _logger = logger;
        _authService = authService;
        _config = config;
        _envConfig = envConfig;
            
        _topic = "in/" + _config.OrganizationId;
        
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
        
        _mqttClient.ConnectedAsync += async (args) => await OnConnectedAsync(new StellaNowConnectedEventArgs());
        _mqttClient.DisconnectedAsync += async (args) => await OnDisconnectedAsync(new StellaNowDisconnectedEventArgs());
    }

    private async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        _logger?.LogInformation("Connected");
        if (ConnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    private async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        _logger?.LogInformation("Disconnected");
        if (DisconnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    private void StartReconnectMonitor(CancellationToken token)
    {
        _ = Task.Run(
            async () =>
            {
                _logger?.LogInformation("Started Reconnection Monitor");
                
                while (true)
                {
                    try
                    {
                        if (!_mqttClient.IsConnected)
                        {
                            await ConnectAsync();
                        }
                    }
                    catch
                    {
                        _logger?.LogInformation("Reconnection Monitor: Unhandled Exception");
                        // Handle the exception properly (logging etc.).
                    }
                    finally
                    {
                        // Check the connection state every 5 seconds and perform a reconnect if required.
                        await Task.Delay(TimeSpan.FromSeconds(5), token);
                    }
                }
            }, token);
    }

    private async Task ConnectAsync()
    {
        _logger?.LogInformation("Connecting");

        await _authService.AuthenticateAsync();
        
        var options = new MqttClientOptionsBuilder()
            .WithClientId(_config.ClientId)
            .WithWebSocketServer(_envConfig.BrokerUrl)
            .WithTls()
            .WithCredentials(_authService.GetAccessToken(), _authService.GetAccessToken())
            .Build();

        await _mqttClient.ConnectAsync(options);
    }

    private async Task DisconnectAsync()
    {
        _logger?.LogInformation("Disconnecting");
        
        if(_reconnectCancellationTokenSource is { IsCancellationRequested: false })
        {
            _reconnectCancellationTokenSource.Cancel();
            _reconnectCancellationTokenSource.Dispose();
            _reconnectCancellationTokenSource = null;
        }
        
        await _mqttClient.DisconnectAsync();
    }

    public async Task StartAsync()
    {
        _reconnectCancellationTokenSource = new CancellationTokenSource();
        StartReconnectMonitor(_reconnectCancellationTokenSource.Token);
    }
    
    public async Task StopAsync()
    {
        await DisconnectAsync();
    }

    public async Task SendMessageAsync(StellaNowEventWrapper message)
    {
        _logger?.LogDebug("Sending Message: {Message}", message.Value.Metadata.MessageId);
        
        var messageBytes = Encoding.UTF8.GetBytes(message.ToString());
        
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(messageBytes)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(mqttMessage);
    }
    
    public void Dispose()
    {
        _logger?.LogDebug("Disposing");
        _reconnectCancellationTokenSource?.Cancel();
        _reconnectCancellationTokenSource?.Dispose();
    }
}