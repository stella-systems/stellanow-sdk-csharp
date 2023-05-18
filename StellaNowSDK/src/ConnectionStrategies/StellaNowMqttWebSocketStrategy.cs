using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using StellaNowSDK.Events;
using StellaNowSDK.Messages;

namespace StellaNowSDK.ConnectionStrategies;

public class StellaNowMqttWebSocketStrategy : IStellaNowConnectionStrategy
{
    private readonly IMqttClient _mqttClient;
    
    private readonly string _brokerUrl;
    private readonly string _clientId;
    private readonly string _topic;
    
    public bool IsConnected => _mqttClient.IsConnected;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    private CancellationTokenSource _reconnectCancellationTokenSource;
    
    public StellaNowMqttWebSocketStrategy(string brokerUrl, string clientId, string topic)
    {
        _brokerUrl = brokerUrl;
        _clientId = clientId;
        _topic = topic;

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
        
        _mqttClient.ConnectedAsync += async (args) => await OnConnectedAsync(new StellaNowConnectedEventArgs());
        _mqttClient.DisconnectedAsync += async (args) => await OnDisconnectedAsync(new StellaNowDisconnectedEventArgs());
    }
    
    protected virtual async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        if (ConnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    protected virtual async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        if (DisconnectedAsync is { } handler)
        {
            await handler(e);
        }
    }
    
    private void StartReconnectMonitor()
    {
        _reconnectCancellationTokenSource?.Cancel();
        _reconnectCancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(
            async () =>
            {
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

    public async Task ConnectAsync()
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId(_clientId)
            .WithWebSocketServer(_brokerUrl)
            .WithTls()
            .Build();

        await _mqttClient.ConnectAsync(options);
        
        StartReconnectMonitor();
    }

    public async Task DisconnectAsync()
    {
        _reconnectCancellationTokenSource?.Cancel();
        
        await _mqttClient.DisconnectAsync();
    }

    public async Task SendMessageAsync(StellaNowMessageWrapper message)
    {
        var messageString = message.ToString();
        var messageBytes = Encoding.UTF8.GetBytes(messageString);
        
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(messageBytes)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build();

        await _mqttClient.PublishAsync(mqttMessage);
    }
}