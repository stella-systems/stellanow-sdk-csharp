using StellaNowSDK.Messages;

namespace StellaNowSDK.Services;

public interface IStellaNowMessageQueue : IDisposable
{
    void StartProcessing();
    void EnqueueMessage(StellaNowEventWrapper message);
}