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

    private void StartReconnectMonitor()
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
                        await Task.Delay(TimeSpan.FromSeconds(5), _reconnectCancellationTokenSource.Token);
                    }
                }
            }, _reconnectCancellationTokenSource.Token);
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
        StartReconnectMonitor();
    }
    
    public async Task StopAsync()
    {
        await DisconnectAsync();
    }

    public async Task SendMessageAsync(StellaNowEventWrapper message)
    {
        _logger?.LogDebug("Sending Message: {Message}", message.Value.Metadata.MessageId);
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
        _logger?.LogDebug("Disposing");
        _reconnectCancellationTokenSource?.Cancel();
        _reconnectCancellationTokenSource?.Dispose();
    }
}