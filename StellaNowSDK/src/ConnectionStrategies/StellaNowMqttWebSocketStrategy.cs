using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using StellaNowSDK.Events;
using StellaNowSDK.Messages;

namespace StellaNowSDK.ConnectionStrategies;

public sealed class StellaNowMqttWebSocketStrategy : IStellaNowConnectionStrategy, IDisposable
{
    private ILogger<StellaNowMqttWebSocketStrategy>? _logger;

    private readonly IMqttClient _mqttClient;
    
    private readonly string _brokerUrl;
    private readonly string _clientId;
    private readonly string _topic;

    private readonly string? _username;
    private readonly string? _password;

    private string? _token =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gR" +
        "G9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.98lAtR21JMLpxbXT04bRdINZx4y9z5gXgLV7q3Be6b0";
    
    public bool IsConnected => _mqttClient.IsConnected;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    private CancellationTokenSource? _reconnectCancellationTokenSource;
    
    public StellaNowMqttWebSocketStrategy(
        ILogger<StellaNowMqttWebSocketStrategy>? logger,
        string brokerUrl, string clientId, string username, string password, string topic)
    {
        _logger = logger;
        _brokerUrl = brokerUrl;
        _clientId = clientId;
        _username = username;
        _password = password;
        _topic = topic;
        
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
        
        _mqttClient.ConnectedAsync += async (args) => await OnConnectedAsync(new StellaNowConnectedEventArgs());
        _mqttClient.DisconnectedAsync += async (args) => await OnDisconnectedAsync(new StellaNowDisconnectedEventArgs());
    }

    private async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        _logger?.LogInformation("StellaNowMqttWebSocketStrategy: Connected");
        if (ConnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    private async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        _logger?.LogInformation("StellaNowMqttWebSocketStrategy: Disconnected");
        if (DisconnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    private void StartReconnectMonitor()
    {
        _ = Task.Run(
            async () =>
            {
                _logger?.LogInformation("StellaNowMqttWebSocketStrategy: Started Reconnection Monitor");
                
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
                        _logger?.LogInformation("StellaNowMqttWebSocketStrategy: Reconnection Monitor: Unhandled Exception");
                        // Handle the exception properly (logging etc.).
                    }
                    finally
                    {
                        // Check the connection state every 5 seconds and perform a reconnect if required.
                        await Task.Delay(TimeSpan.FromSeconds(5), _reconnectCancellationTokenSource.Token);
                    }
                }
            }, _reconnectCancellationTokenSource.Token);
    }

    private async Task ConnectAsync()
    {
        _logger?.LogInformation("StellaNowMqttWebSocketStrategy: Connecting");
        
        // TODO: call Keycloak
        
        var options = new MqttClientOptionsBuilder()
            .WithClientId(_clientId)
            .WithWebSocketServer(_brokerUrl)
            .WithTls()
            .WithCredentials(_username, _token)
            .Build();

        await _mqttClient.ConnectAsync(options);
    }

    private async Task DisconnectAsync()
    {
        _logger?.LogInformation("StellaNowMqttWebSocketStrategy: Disconnecting");
        
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
        StartReconnectMonitor();
    }
    
    public async Task StopAsync()
    {
        await DisconnectAsync();
    }

    public async Task SendMessageAsync(StellaNowEventWrapper message)
    {
        _logger?.LogDebug("StellaNowMqttWebSocketStrategy: Sending Message: " + message.Value.Metadata.MessageId);
        var messageString = message.GetForDispatch();
        var messageBytes = Encoding.UTF8.GetBytes(messageString);
        
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(messageBytes)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(mqttMessage);
    }
    
    public void Dispose()
    {
        _logger?.LogDebug("StellaNowMqttWebSocketStrategy: Disposing");
        _reconnectCancellationTokenSource?.Cancel();
        _reconnectCancellationTokenSource?.Dispose();
    }
}