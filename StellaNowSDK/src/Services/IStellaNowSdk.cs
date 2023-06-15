using StellaNowSDK.Events;
using StellaNowSDK.Messages;

namespace StellaNowSDK.Services;

public interface IStellaNowSdk
{
    bool IsConnected { get; }

    event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;

    Task StartAsync();
    Task StopAsync();
    void SendMessage(StellaNowMessageWrapper message);
}