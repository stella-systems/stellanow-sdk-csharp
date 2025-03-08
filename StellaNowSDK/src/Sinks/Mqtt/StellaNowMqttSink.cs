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

using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using NanoidDotNet;

using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvironmentConfig;
using StellaNowSDK.Events;
using StellaNowSDK.Exceptions;
using StellaNowSDK.Exceptions.Sinks.Mqtt;
using StellaNowSDK.Messages;
using StellaNowSDK.Sinks.Mqtt.AuthStrategy;

namespace StellaNowSDK.Sinks.Mqtt;

/// <summary>
/// An MQTT-based sink for StellaNow messages, handling connection, disconnection, 
/// and message publishing to the broker.
/// </summary>
/// <remarks>
/// Uses a specified <see cref="IMqttAuthStrategy"/> to handle authentication 
/// or no-auth connections, and manages reconnection if the broker disconnects.
/// </remarks>
public sealed class StellaNowMqttSink : IStellaNowSink, IDisposable
{
    private readonly ILogger<StellaNowMqttSink> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly IMqttAuthStrategy _mqttAuthStrategy;
    private readonly StellaNowEnvironmentConfig _envConfig;
    private readonly string _topic;
    private readonly object _lockObject = new(); // For thread safety

    public string MqttClientId { get; }
    public bool IsConnected => _mqttClient.IsConnected;
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;

    private CancellationTokenSource? _reconnectCancellationTokenSource;
    private Task? _reconnectMonitorTask; // Store the monitor task for shutdown
    private const int BaseReconnectDelaySeconds = 5;
    private const int MaxReconnectDelaySeconds = 60;

    public StellaNowMqttSink(
        ILogger<StellaNowMqttSink> logger,
        IMqttAuthStrategy authStrategy,
        StellaNowConfig stellaNowConfig,
        StellaNowEnvironmentConfig envConfig,
        string? clientId = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(authStrategy);
        ArgumentNullException.ThrowIfNull(stellaNowConfig);
        ArgumentNullException.ThrowIfNull(envConfig);
        ArgumentException.ThrowIfNullOrWhiteSpace(stellaNowConfig.organizationId, nameof(stellaNowConfig.organizationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(envConfig.BrokerUrl, nameof(envConfig.BrokerUrl));
        if (!Uri.TryCreate(envConfig.BrokerUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Broker URL is not a valid URI", nameof(envConfig.BrokerUrl));

        _logger = logger;
        _mqttAuthStrategy = authStrategy;
        _envConfig = envConfig;
        _topic = $"in/{stellaNowConfig.organizationId}";

        MqttClientId = string.IsNullOrWhiteSpace(clientId)
            ? $"StellaNowSDK_{Nanoid.Generate(size: 10)}"
            : clientId.Trim();

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ConnectedAsync += OnConnectedAsync;
        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;

        _logger.LogInformation("SDK Client ID is \"{ClientId}\"", MqttClientId);
    }

    private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
    {
        _logger.LogInformation("Connected to MQTT broker");
        if (ConnectedAsync is { } handler)
            await handler(new StellaNowConnectedEventArgs());
    }

    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        _logger.LogInformation("Disconnected from MQTT broker");
        if (DisconnectedAsync is { } handler)
            await handler(new StellaNowDisconnectedEventArgs());
    }

    private async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to connect to MQTT broker at {BrokerUrl}", _envConfig.BrokerUrl);
            await _mqttAuthStrategy.ConnectAsync(_mqttClient, MqttClientId);
            _logger.LogInformation("Connected to MQTT broker at {BrokerUrl}", _envConfig.BrokerUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker at {BrokerUrl}", _envConfig.BrokerUrl);
            throw;
        }
    }

    private async Task DisconnectAsync()
    {
        try
        {
            _logger.LogInformation("Disconnecting from MQTT broker");
            lock (_lockObject)
            {
                _reconnectCancellationTokenSource?.Cancel();
                // Await the monitor task to ensure it completes
                if (_reconnectMonitorTask is { IsCompleted: false })
                {
                    _reconnectMonitorTask.GetAwaiter().GetResult(); // Synchronous wait after cancellation
                }
                _reconnectCancellationTokenSource?.Dispose();
                _reconnectCancellationTokenSource = null;
                _reconnectMonitorTask = null;
            }
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("Disconnected from MQTT broker");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from MQTT broker at {BrokerUrl}", _envConfig.BrokerUrl);
            throw new StellaNowException("Failed to disconnect from the MQTT broker", ex);
        }
    }

    private async Task StartReconnectMonitor(CancellationToken token)
    {
        _logger.LogInformation("Started reconnection monitor");
        var attempt = 0;

        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        attempt++;
                        _logger.LogInformation("Attempting to connect (Attempt {Attempt})", attempt);
                        await ConnectAsync(token);
                        attempt = 0; // Reset on success
                    }
                }
                catch (MqttConnectionException ex)
                {
                    _logger.LogError(ex, "Connection attempt {Attempt} failed due to a connection error", attempt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Connection attempt {Attempt} failed due to an unexpected error", attempt);
                }

                var delaySeconds = Math.Min(BaseReconnectDelaySeconds * Math.Pow(2, attempt - 1), MaxReconnectDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Reconnection monitor cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in reconnection monitor");
            throw; // Propagate to the continuation
        }
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        lock (_lockObject)
        {
            if (_reconnectCancellationTokenSource != null)
                throw new InvalidOperationException("Sink is already started");

            _reconnectCancellationTokenSource = new CancellationTokenSource();
        }

        // Start the monitor immediately, which will handle the initial connection
        _reconnectMonitorTask = StartReconnectMonitor(_reconnectCancellationTokenSource.Token)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Reconnection monitor failed unexpectedly");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

        // Wait for a short period to give the initial connection a chance to succeed
        var initialConnectTimeout = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < initialConnectTimeout)
        {
            if (_mqttClient.IsConnected)
            {
                _logger.LogInformation("Initial connection established successfully");
                break;
            }
            await Task.Delay(500); // Check every 500ms
        }

        // If not connected yet, the monitor will keep trying
        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("Initial connection attempt did not succeed within {TimeoutSeconds} seconds. The reconnection monitor will continue trying in the background", initialConnectTimeout.TotalSeconds);
        }
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        await DisconnectAsync();
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(StellaNowEventWrapper message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!IsConnected)
            throw new MqttConnectionException("Cannot send message: Sink is not connected", _envConfig.BrokerUrl);

        _logger.LogDebug("Sending message with ID: {MessageId}", message.Value.Metadata.MessageId);

        var messageBytes = Encoding.UTF8.GetBytes(message.ToString());
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(messageBytes)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        try
        {
            await _mqttClient.PublishAsync(mqttMessage);
            message.Callback?.Invoke(message);
            _logger.LogDebug("Message with ID {MessageId} sent successfully", message.Value.Metadata.MessageId);
        }
        catch (MqttCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to send message with ID {MessageId} due to a communication error", message.Value.Metadata.MessageId);
            throw new MqttConnectionException("Failed to send message to the MQTT broker", _envConfig.BrokerUrl, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while sending message with ID {MessageId}", message.Value.Metadata.MessageId);
            throw new StellaNowException("Unexpected error while sending message to the MQTT broker", ex);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.LogDebug("Disposing StellaNowMqttSink");
        lock (_lockObject)
        {
            _reconnectCancellationTokenSource?.Cancel();
            if (_reconnectMonitorTask is { IsCompleted: false })
            {
                _reconnectMonitorTask.GetAwaiter().GetResult(); // Wait for the task to complete
            }
            _reconnectCancellationTokenSource?.Dispose();
            _reconnectCancellationTokenSource = null;
            _reconnectMonitorTask = null;
        }
        _mqttClient.ConnectedAsync -= OnConnectedAsync;
        _mqttClient.DisconnectedAsync -= OnDisconnectedAsync;
        _mqttClient.Dispose();
    }
}