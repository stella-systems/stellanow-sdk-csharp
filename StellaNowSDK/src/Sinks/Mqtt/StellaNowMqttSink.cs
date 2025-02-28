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

using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using NanoidDotNet;
using StellaNowSDK.Config;
using StellaNowSDK.Events;
using StellaNowSDK.Messages;
using StellaNowSDK.Sinks.Mqtt.ConnectionStrategy;

namespace StellaNowSDK.Sinks.Mqtt;

/// <summary>
/// An MQTT-based sink for StellaNow messages, handling connection, disconnection, 
/// and message publishing to the broker.
/// </summary>
/// <remarks>
/// Uses a specified <see cref="IMqttConnectionStrategy"/> to handle authentication 
/// or no-auth connections, and manages reconnection if the broker disconnects.
/// </remarks>
public sealed class StellaNowMqttSink : IStellaNowSink, IDisposable
{
    private ILogger<IStellaNowSink>? _logger;

    private readonly IMqttClient _mqttClient;
    
    /// <summary>
    /// Gets the unique MQTT client ID used when connecting to the broker.
    /// </summary>
    public string MqttClientId { get; }
    
    private readonly IMqttConnectionStrategy _mqttConnectionStrategy;

    private readonly StellaNowConfig _config;
    
    private readonly string _topic;
    
    /// <inheritdoc />
    public bool IsConnected => _mqttClient.IsConnected;
    
    /// <summary>
    /// Occurs when the sink successfully connects to the broker.
    /// </summary>
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    
    /// <summary>
    /// Occurs when the sink disconnects from the broker.
    /// </summary>
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    private CancellationTokenSource? _reconnectCancellationTokenSource;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StellaNowMqttSink"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages and errors.</param>
    /// <param name="connectionStrategy">
    /// Strategy for connecting to the MQTT broker (no-auth, OIDC, user/pass).
    /// </param>
    /// <param name="config">
    /// Contains organization and project IDs, used to build the topic path.
    /// </param>
    public StellaNowMqttSink(
        ILogger<StellaNowMqttSink>? logger,
        IMqttConnectionStrategy connectionStrategy,
        StellaNowConfig config)
    {
        _logger = logger;
        _mqttConnectionStrategy = connectionStrategy;
        _config = config;
            
        _topic = "in/" + _config.organizationId;
        
        MqttClientId = $"StellaNowSDK_{Nanoid.Generate(size: 10)}";
        
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
        
        _mqttClient.ConnectedAsync += async (args) => await OnConnectedAsync(new StellaNowConnectedEventArgs());
        _mqttClient.DisconnectedAsync += async (args) => await OnDisconnectedAsync(new StellaNowDisconnectedEventArgs());
        
        _logger.LogInformation($"SDK Client ID is \"{MqttClientId}\"");
    }

    /// <summary>
    /// Asynchronously raises the <see cref="ConnectedAsync"/> event when the MQTT client connects.
    /// </summary>
    /// <param name="e">The connection event data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        _logger?.LogInformation("Connected");
        if (ConnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    /// <summary>
    /// Asynchronously raises the <see cref="DisconnectedAsync"/> event when the MQTT client disconnects.
    /// </summary>
    /// <param name="e">The disconnection event data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        _logger?.LogInformation("Disconnected");
        if (DisconnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    /// <summary>
    /// Starts a background task that periodically checks the MQTT connection and attempts 
    /// to reconnect if disconnected.
    /// </summary>
    /// <param name="token">A <see cref="CancellationToken"/> for stopping the monitor.</param>
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

    /// <summary>
    /// Establishes a connection to the MQTT broker using the configured connection strategy.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous connect operation.</returns>
    private async Task ConnectAsync()
    {
        _logger?.LogInformation("Connecting");

        await _mqttConnectionStrategy.ConnectAsync(_mqttClient, MqttClientId);
    }

    /// <summary>
    /// Disconnects from the MQTT broker, stopping the reconnection monitor if it is active.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous disconnect operation.</returns>
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

    /// <summary>
    /// Starts the sink, which in turn starts a reconnection monitor 
    /// to maintain a persistent connection to the broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        _reconnectCancellationTokenSource = new CancellationTokenSource();
        StartReconnectMonitor(_reconnectCancellationTokenSource.Token);
    }
    
    /// <summary>
    /// Stops the sink, cancelling any reconnection attempts and disconnecting from the broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopAsync()
    {
        await DisconnectAsync();
    }

    /// <summary>
    /// Sends a StellaNow event message to the broker by publishing it to the configured topic.
    /// </summary>
    /// <param name="message">The event wrapper containing the message payload and metadata.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous publish operation.</returns>
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
        
        message.Callback?.Invoke(message);
    }
    
    /// <summary>
    /// Disposes of resources used by this sink, including cancelling reconnection tasks.
    /// </summary>
    public void Dispose()
    {
        _logger?.LogDebug("Disposing");
        _reconnectCancellationTokenSource?.Cancel();
        _reconnectCancellationTokenSource?.Dispose();
    }
}