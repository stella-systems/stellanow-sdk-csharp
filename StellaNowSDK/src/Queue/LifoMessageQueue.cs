using System.Collections.Concurrent;
using StellaNowSDK.Models;

namespace StellaNowSDK.Queue;

public class LifoMessageQueue : MessageQueue
{
    private readonly ConcurrentStack<StellaNowMessageWrapper?> _stack = new ConcurrentStack<StellaNowMessageWrapper?>();

    public override void Enqueue(StellaNowMessageWrapper? message)
    {
        _stack.Push(message);
    }

    public override bool TryDequeue(out StellaNowMessageWrapper? message)
    {
        return _stack.TryPop(out message);
    }
}