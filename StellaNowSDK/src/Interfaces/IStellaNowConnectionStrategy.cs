using StellaNowSDK.Events;
using StellaNowSDK.Models;

namespace StellaNowSDK.Interfaces;

public interface IStellaNowConnectionStrategy
{
    event Func<StellaNowConnectedEventArgs, Task> ConnectedAsync;
    event Func<StellaNowDisconnectedEventArgs, Task> DisconnectedAsync;
    
    bool IsConnected { get; }
    
    Task ConnectAsync();

    Task DisconnectAsync();

    Task SendMessageAsync(StellaNowMessageWrapper message);
}