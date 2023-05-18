using System.Collections.Concurrent;
using StellaNowSDK.Messages;

namespace StellaNowSDK.Queue;

public class FifoMessageQueueStrategy : IMessageQueueStrategy
{
    private readonly ConcurrentQueue<StellaNowMessageWrapper?> _queue = new ConcurrentQueue<StellaNowMessageWrapper?>();

    public void Enqueue(StellaNowMessageWrapper? message)
    {
        _queue.Enqueue(message);
    }

    public bool TryDequeue(out StellaNowMessageWrapper? message)
    {
        return _queue.TryDequeue(out message);
    }
}