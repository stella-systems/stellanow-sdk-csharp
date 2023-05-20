using StellaNowSDK.Messages;

namespace StellaNowSDK.Queue;

public interface IMessageQueueStrategy
{
    public abstract void Enqueue(StellaNowEventWrapper? message);
    public abstract bool TryDequeue(out StellaNowEventWrapper? message);
}