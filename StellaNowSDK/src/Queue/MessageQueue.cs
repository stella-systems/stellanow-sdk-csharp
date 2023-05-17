using StellaNowSDK.Models;

namespace StellaNowSDK.Queue;

public abstract class MessageQueue
{
    public abstract void Enqueue(StellaNowMessageWrapper? message);
    public abstract bool TryDequeue(out StellaNowMessageWrapper? message);
}