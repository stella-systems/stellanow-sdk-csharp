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

    private readonly string? _username = "CsharpSdkTest";
    private readonly string? _password = "password";

    private string? _token =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gR" +
        "G9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.98lAtR21JMLpxbXT04bRdINZx4y9z5gXgLV7q3Be6b0";
    
    public bool IsConnected => _mqttClient.IsConnected;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    private CancellationTokenSource? _reconnectCancellationTokenSource;
    
    public StellaNowMqttWebSocketStrategy(string brokerUrl, string clientId, string username, string password, string topic)
    {
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

    private async Task ConnectAsync()
    {
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
        var messageString = message.GetForDispatch();
        var messageBytes = Encoding.UTF8.GetBytes(messageString);
        
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(messageBytes)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build();

        await _mqttClient.PublishAsync(mqttMessage);
    }
}