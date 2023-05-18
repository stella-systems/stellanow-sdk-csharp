using StellaNowSDK.Messages;

namespace StellaNowSDK.Queue;

public interface IMessageQueueStrategy
{
    public abstract void Enqueue(StellaNowMessageWrapper? message);
    public abstract bool TryDequeue(out StellaNowMessageWrapper? message);
}