// Copyright (C) 2022-2025 Stella Technologies (UK) Limited.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using Microsoft.Extensions.Logging;
using StellaNowSDK.Sinks;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;

namespace StellaNowSDK.Services;

/// <summary>
/// A concrete implementation of <see cref="IStellaNowMessageQueue"/> that uses
/// an <see cref="IMessageQueueStrategy"/> (e.g., FIFO/LIFO) to store messages 
/// and an <see cref="IStellaNowSink"/> to dispatch them.
/// </summary>
/// <remarks>
/// This class manages a background task that continuously attempts to dequeue 
/// and send messages, delaying if no messages are available or if the sink is disconnected.
/// </remarks>
public sealed class StellaNowMessageQueue: IStellaNowMessageQueue
{
    private readonly ILogger<StellaNowMessageQueue>? _logger;
    
    private readonly IMessageQueueStrategy _messageQueueStrategy;
    private readonly IStellaNowSink _sink;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Task? _queueProcessingTask;
    
    private StellaNowEventWrapper? _currentMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="StellaNowMessageQueue"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages and errors.</param>
    /// <param name="messageQueueStrategy">
    /// The strategy used to enqueue and dequeue messages (FIFO, LIFO, etc.).
    /// </param>
    /// <param name="sink">The sink responsible for sending messages to the broker or service.</param>
    public StellaNowMessageQueue(
        ILogger<StellaNowMessageQueue>? logger,
        IMessageQueueStrategy messageQueueStrategy, IStellaNowSink sink)
    {
        _logger = logger;
        _messageQueueStrategy = messageQueueStrategy;
        _sink = sink;
    }

    /// <inheritdoc />
    public void StartProcessing()
    {
        if (_queueProcessingTask == null || _queueProcessingTask.IsCompleted)
        {
            _queueProcessingTask = Task.Run(ProcessMessageQueueAsync);
        }
    }
    
    /// <inheritdoc />
    public void StopProcessing()
    {
        _cancellationTokenSource?.Cancel();
    }

    /// <inheritdoc />
    public void EnqueueMessage(StellaNowEventWrapper message)
    {
        _logger?.LogDebug("Queueing Message: {Message}", message.Value.Metadata.MessageId);
        _messageQueueStrategy.Enqueue(message);
    }

    /// <inheritdoc />
    public bool IsQueueEmpty()
    {
        return _messageQueueStrategy.IsEmpty();
    }

    /// <inheritdoc />
    public int GetMessageCountOnQueue()
    {
        return _messageQueueStrategy.GetMessageCount();
    }

    /// <summary>
    /// Continuously processes messages by dequeuing and sending them to the sink 
    /// until cancellation is requested.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the background queue processing.</returns>
    private async Task ProcessMessageQueueAsync()
    {
        _logger?.LogDebug("Start Processing Message Queue");
        
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                if (_currentMessage == null && 
                    _sink.IsConnected && 
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
                    
                    await _sink.SendMessageAsync(_currentMessage);
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
    
    /// <summary>
    /// Disposes the queue, cancelling any background processing tasks 
    /// and releasing associated resources.
    /// </summary>
    public void Dispose()
    {
        _logger?.LogDebug("Disposing");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}