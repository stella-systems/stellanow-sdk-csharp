using System.Collections.Concurrent;
using StellaNowSDK.Messages;

namespace StellaNowSDK.Queue;

public sealed class FifoMessageQueueStrategy : IMessageQueueStrategy
{
    private readonly ConcurrentQueue<StellaNowEventWrapper?> _queue = new ConcurrentQueue<StellaNowEventWrapper?>();

    public void Enqueue(StellaNowEventWrapper? message)
    {
        _queue.Enqueue(message);
    }

    public bool TryDequeue(out StellaNowEventWrapper? message)
    {
        return _queue.TryDequeue(out message);
    }
}