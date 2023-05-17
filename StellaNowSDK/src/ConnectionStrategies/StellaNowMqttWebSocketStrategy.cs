using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using StellaNowSDK.Events;
using StellaNowSDK.Interfaces;
using StellaNowSDK.Models;

namespace StellaNowSDK.ConnectionStrategies;

public class StellaNowMqttWebSocketStrategy : IStellaNowConnectionStrategy
{
    private readonly IMqttClient _mqttClient;
    
    private readonly string _brokerUrl;
    private readonly string _clientId;
    
    public bool IsConnected => _mqttClient.IsConnected;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    
    public StellaNowMqttWebSocketStrategy(string brokerUrl, string clientId)
    {
        _brokerUrl = brokerUrl;
        _clientId = clientId;

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

    public async Task ConnectAsync()
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId(_clientId)
            .WithWebSocketServer(_brokerUrl)
            .Build();

        await _mqttClient.ConnectAsync(options);
    }

    public async Task DisconnectAsync()
    {
        await _mqttClient.DisconnectAsync();
    }

    public async Task SendMessageAsync(StellaNowMessageWrapper message)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(message.Topic)
            .WithPayload(message.Message)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build();

        await _mqttClient.PublishAsync(mqttMessage);
    }
}