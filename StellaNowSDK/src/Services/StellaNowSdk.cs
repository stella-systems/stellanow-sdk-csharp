using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

public class StellaNowSdk
{    
    private readonly IStellaNowConnectionStrategy _connectionStrategy;
    private readonly StellaNowMessageQueueService _messageQueueService;
    
    public bool IsConnected => _connectionStrategy?.IsConnected ?? false;
    
    public StellaNowSdk(IStellaNowConnectionStrategy connectionStrategy, IMessageQueueStrategy messageQueueStrategy)
    {
        _connectionStrategy = connectionStrategy;
        _messageQueueService = new StellaNowMessageQueueService(messageQueueStrategy, _connectionStrategy);
    }

    public async Task ConnectAsync()
    {
        await _connectionStrategy.ConnectAsync();
    }

    public async Task DisconnectAsync()
    {
        await _connectionStrategy.DisconnectAsync();
    }

    public void Send(StellaNowMessageWrapper message)
    {
        _messageQueueService.EnqueueMessage(message);
    }
}