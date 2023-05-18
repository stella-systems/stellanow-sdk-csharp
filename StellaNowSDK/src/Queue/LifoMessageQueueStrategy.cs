using System.Collections.Concurrent;
using StellaNowSDK.Messages;

namespace StellaNowSDK.Queue;

public class LifoMessageQueueStrategy : IMessageQueueStrategy
{
    private readonly ConcurrentStack<StellaNowMessageWrapper?> _stack = new ConcurrentStack<StellaNowMessageWrapper?>();

    public void Enqueue(StellaNowMessageWrapper? message)
    {
        _stack.Push(message);
    }

    public bool TryDequeue(out StellaNowMessageWrapper? message)
    {
        return _stack.TryPop(out message);
    }
}