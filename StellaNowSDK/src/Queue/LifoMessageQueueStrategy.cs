using System.Collections.Concurrent;
using StellaNowSDK.Messages;

namespace StellaNowSDK.Queue;

public class LifoMessageQueueStrategy : IMessageQueueStrategy
{
    private readonly ConcurrentStack<StellaNowEventWrapper?> _stack = new ConcurrentStack<StellaNowEventWrapper?>();

    public void Enqueue(StellaNowEventWrapper? message)
    {
        _stack.Push(message);
    }

    public bool TryDequeue(out StellaNowEventWrapper? message)
    {
        return _stack.TryPop(out message);
    }
}