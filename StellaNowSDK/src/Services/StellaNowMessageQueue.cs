using Microsoft.Extensions.Logging;
using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

public sealed class StellaNowMessageQueue: IStellaNowMessageQueue
{
    private readonly ILogger<StellaNowMessageQueue>? _logger;
    
    private readonly IMessageQueueStrategy _messageQueueStrategy;
    private readonly IStellaNowConnectionStrategy _connectionStrategy;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Task? _queueProcessingTask;
    
    private StellaNowEventWrapper? _currentMessage;

    public StellaNowMessageQueue(
        ILogger<StellaNowMessageQueue>? logger,
        IMessageQueueStrategy messageQueueStrategy, IStellaNowConnectionStrategy connectionStrategy)
    {
        _logger = logger;
        _messageQueueStrategy = messageQueueStrategy;
        _connectionStrategy = connectionStrategy;
    }

    public void StartProcessing()
    {
        if (_queueProcessingTask == null || _queueProcessingTask.IsCompleted)
        {
            _queueProcessingTask = Task.Run(ProcessMessageQueueAsync);
        }
    }
    
    public void StopProcessing()
    {
        _cancellationTokenSource?.Cancel();
    }

    public void EnqueueMessage(StellaNowEventWrapper message)
    {
        _logger?.LogDebug("Queueing Message: {Message}", message.Value.Metadata.MessageId);
        _messageQueueStrategy.Enqueue(message);
    }

    private async Task ProcessMessageQueueAsync()
    {
        _logger?.LogDebug("Start Processing Message Queue");
        
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                if (_currentMessage == null && 
                    _connectionStrategy.IsConnected && 
                    _messageQueueStrategy.TryDequeue(out var message))
                {
                    _currentMessage = message;
                }
                
                // Try to send the current message
                if (_currentMessage != null)
                {
                    _logger?.LogInformation(
                        "Attempting to send message: {Message}", 
                        _currentMessage.Value.Metadata.MessageId);
                    
                    await _connectionStrategy.SendMessageAsync(_currentMessage);
                    _currentMessage = null;  // Clear the current message after it was successfully sent
                }
                else
                {
                    // If the client is not connected or there's no message to process, delay to avoid tight looping
                    await Task.Delay(500);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation("Thread Cancelled");
                // The CancellationTokenSource was cancelled, which means we should stop processing the queue
                break;
            }
            catch
            {
                _logger?.LogError("Unhandled Exception");
                // Handle any other exceptions that might occur during message processing
                await Task.Delay(500);
            }
        }
        
        _logger?.LogDebug("Ended Processing Message Queue");
    }
    
    public void Dispose()
    {
        _logger?.LogDebug("Disposing");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}