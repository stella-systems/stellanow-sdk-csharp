using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

public class StellaNowSdk
{
    private string _organizationId;
    private string _projectId;
    
    private readonly IStellaNowConnectionStrategy _connectionStrategy;
    private readonly StellaNowMessageQueueService _messageQueueService;
    
    public bool IsConnected => _connectionStrategy?.IsConnected ?? false;
    
    public StellaNowSdk(
        IStellaNowConnectionStrategy connectionStrategy, IMessageQueueStrategy messageQueueStrategy,
        string organizationId, string projectId) 
    {
        _connectionStrategy = connectionStrategy;
        _organizationId = organizationId;
        _projectId = projectId;
        _messageQueueService = new StellaNowMessageQueueService(messageQueueStrategy, _connectionStrategy);
    }

    public async Task StartAsync()
    {
        await _connectionStrategy.StartAsync();
    }

    public async Task StopAsync()
    {
        await _connectionStrategy.StopAsync();
    }

    public void Send(StellaNowMessageWrapper message)
    {
        _messageQueueService.EnqueueMessage(
            new StellaNowEventWrapper(
                new EventKey(_organizationId, _projectId),
                message)
        );
    }
}