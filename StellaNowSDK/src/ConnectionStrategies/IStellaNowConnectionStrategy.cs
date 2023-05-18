using StellaNowSDK.Events;
using StellaNowSDK.Messages;

namespace StellaNowSDK.ConnectionStrategies;

public interface IStellaNowConnectionStrategy
{
    event Func<StellaNowConnectedEventArgs, Task> ConnectedAsync;
    event Func<StellaNowDisconnectedEventArgs, Task> DisconnectedAsync;
    
    bool IsConnected { get; }
    
    Task ConnectAsync();

    Task DisconnectAsync();

    Task SendMessageAsync(StellaNowMessageWrapper message);
}