using Microsoft.Extensions.Logging;
using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Events;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

public sealed class StellaNowSdk: IStellaNowSdk, IDisposable
{
    private readonly ILogger<IStellaNowSdk>? _logger;
    
    private readonly string _organizationId;
    private readonly string _projectId;
    
    private readonly IStellaNowConnectionStrategy _connectionStrategy;
    private readonly IStellaNowMessageQueue _messageQueue;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    public bool IsConnected => _connectionStrategy?.IsConnected ?? false;
    
    public StellaNowSdk(
        ILogger<StellaNowSdk>? logger,
        IStellaNowConnectionStrategy connectionStrategy, 
        IStellaNowMessageQueue messageQueue,
        string organizationId, string projectId)
    {
        _logger = logger;
        _connectionStrategy = connectionStrategy;
        _organizationId = organizationId;
        _projectId = projectId;
        _messageQueue = messageQueue;
        
        _connectionStrategy.ConnectedAsync += OnConnectedAsync;
        _connectionStrategy.DisconnectedAsync += OnDisconnectedAsync;
    }

    public async Task StartAsync()
    {
        _logger?.LogInformation("StellaNowSdk: Starting");
        await _connectionStrategy.StartAsync();
    }

    public async Task StopAsync()
    {
        _logger?.LogInformation("StellaNowSdk: Stopping");
        await _connectionStrategy.StopAsync();
    }

    public void SendMessage(StellaNowMessageWrapper message)
    {
        _messageQueue.EnqueueMessage(
            new StellaNowEventWrapper(
                new EventKey(_organizationId, _projectId),
                message)
        );
    }
    
    private async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        if (ConnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    private async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        if (DisconnectedAsync is { } handler)
        {
            await handler(e);
        }
    }
    
    public void Dispose()
    {
        _logger?.LogDebug("StellaNowSdk: Disposing");
        _connectionStrategy.ConnectedAsync -= OnConnectedAsync;
        _connectionStrategy.DisconnectedAsync -= OnDisconnectedAsync;
    }

}