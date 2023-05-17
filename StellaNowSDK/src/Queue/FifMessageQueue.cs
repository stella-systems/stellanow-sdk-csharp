using System.Collections.Concurrent;
using StellaNowSDK.Models;

namespace StellaNowSDK.Queue;

public class FifoMessageQueue : MessageQueue
{
    private readonly ConcurrentQueue<StellaNowMessageWrapper?> _queue = new ConcurrentQueue<StellaNowMessageWrapper?>();

    public override void Enqueue(StellaNowMessageWrapper? message)
    {
        _queue.Enqueue(message);
    }

    public override bool TryDequeue(out StellaNowMessageWrapper? message)
    {
        return _queue.TryDequeue(out message);
    }
}