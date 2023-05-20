using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

public class StellaNowMessageQueueService
{
    private readonly IMessageQueueStrategy _messageQueueStrategy;
    private readonly IStellaNowConnectionStrategy _connectionStrategy;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Task? _queueProcessingTask;

    public StellaNowMessageQueueService(IMessageQueueStrategy messageQueueStrategy, IStellaNowConnectionStrategy connectionStrategy)
    {
        _messageQueueStrategy = messageQueueStrategy;
        _connectionStrategy = connectionStrategy;
    }

    public void StartProcessing()
    {
        _queueProcessingTask = Task.Run(ProcessMessageQueueAsync);
    }
    
    public void EnqueueMessage(StellaNowEventWrapper message)
    {
        _messageQueueStrategy.Enqueue(message);
    }

    private async Task ProcessMessageQueueAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                if (_connectionStrategy.IsConnected && _messageQueueStrategy.TryDequeue(out var message))
                {
                    await _connectionStrategy.SendMessageAsync(message);
                }
                else
                {
                    // If the client is not connected or the queue is empty, delay to avoid tight looping
                    await Task.Delay(500);
                }
            }
            catch (OperationCanceledException)
            {
                // The CancellationTokenSource was cancelled, which means we should stop processing the queue
                break;
            }
            catch
            {
                // Handle any other exceptions that might occur during message processing
            }
        }
    }
}