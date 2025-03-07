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
using StellaNowSDK.Exceptions.Sinks.Mqtt;
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
public sealed class StellaNowMessageQueue : IStellaNowMessageQueue
{
    private readonly ILogger<StellaNowMessageQueue> _logger; // Required logger
    private readonly IMessageQueueStrategy _messageQueueStrategy;
    private readonly IStellaNowSink _sink;
    private readonly object _lockObject = new(); // For thread safety
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _queueProcessingTask;
    private StellaNowEventWrapper? _currentMessage;
    private bool _disposed;

    // Retry delay configuration
    private const int BaseRetryDelayMs = 500;
    private const int MaxRetryDelayMs = 5000;
    private int _retryCount = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="StellaNowMessageQueue"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages and errors. Must not be null.</param>
    /// <param name="messageQueueStrategy">
    /// The strategy used to enqueue and dequeue messages (FIFO, LIFO, etc.). Must not be null.
    /// </param>
    /// <param name="sink">The sink responsible for sending messages to the broker or service. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public StellaNowMessageQueue(
        ILogger<StellaNowMessageQueue> logger,
        IMessageQueueStrategy messageQueueStrategy,
        IStellaNowSink sink)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(messageQueueStrategy);
        ArgumentNullException.ThrowIfNull(sink);

        _logger = logger;
        _messageQueueStrategy = messageQueueStrategy;
        _sink = sink;
    }

    /// <inheritdoc />
    public async Task StartProcessingAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowMessageQueue));

        lock (_lockObject)
        {
            if (_queueProcessingTask != null && !_queueProcessingTask.IsCompleted)
                return; // Already running, no-op

            _queueProcessingTask = Task.Run(ProcessMessageQueueAsync, _cancellationTokenSource.Token);
        }

        _logger.LogDebug("Started processing message queue");
    }

    /// <inheritdoc />
    public async Task StopProcessingAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowMessageQueue));

        lock (_lockObject)
        {
            if (_queueProcessingTask == null || _queueProcessingTask.IsCompleted)
                return; // Not running, no-op

            _cancellationTokenSource.Cancel();
        }

        try
        {
            if (_queueProcessingTask != null)
                await _queueProcessingTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Queue processing task cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while stopping queue processing");
            throw;
        }
        finally
        {
            _logger.LogDebug("Stopped processing message queue");
        }
    }

    /// <inheritdoc />
    public void EnqueueMessage(StellaNowEventWrapper message)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowMessageQueue));

        ArgumentNullException.ThrowIfNull(message);

        lock (_lockObject)
        {
            _logger.LogDebug("Queueing message with ID: {MessageId}", message.Value.Metadata.MessageId);
            _messageQueueStrategy.Enqueue(message);
        }
    }

    /// <inheritdoc />
    public bool IsQueueEmpty()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowMessageQueue));

        lock (_lockObject)
        {
            return _messageQueueStrategy.IsEmpty();
        }
    }

    /// <inheritdoc />
    public int GetMessageCountOnQueue()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowMessageQueue));

        lock (_lockObject)
        {
            return _messageQueueStrategy.GetMessageCount();
        }
    }

    private async Task ProcessMessageQueueAsync()
    {
        _logger.LogDebug("Started processing message queue");

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                StellaNowEventWrapper? message = null;
                lock (_lockObject)
                {
                    // Dequeue a new message only if no current message and sink is connected
                    if (_currentMessage == null && _sink.IsConnected && _messageQueueStrategy.TryDequeue(out message))
                    {
                        _currentMessage = message;
                    }
                }

                if (_currentMessage != null)
                {
                    var messageToSend = _currentMessage; // Capture for logging
                    _logger.LogInformation("Attempting to send message with ID: {MessageId}", messageToSend.Value.Metadata.MessageId);

                    if (_sink.IsConnected) // Check connection before sending
                    {
                        try
                        {
                            await _sink.SendMessageAsync(messageToSend);
                            lock (_lockObject)
                            {
                                _currentMessage = null; // Clear only after successful send
                                _retryCount = 0; // Reset retry count on success
                            }
                            _logger.LogInformation("Message with ID {MessageId} sent successfully", messageToSend.Value.Metadata.MessageId);
                        }
                        catch (MqttConnectionException ex)
                        {
                            _logger.LogWarning(ex, "Failed to send message with ID {MessageId} due to connection error, will retry on next iteration", messageToSend.Value.Metadata.MessageId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unexpected error sending message with ID {MessageId}, will retry on next iteration", messageToSend.Value.Metadata.MessageId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Sink is not connected, delaying retry for message with ID {MessageId}", messageToSend.Value.Metadata.MessageId);
                    }

                    // Apply exponential backoff if not connected or on failure
                    if (!_sink.IsConnected || _retryCount > 0)
                    {
                        _retryCount++;
                        var delayMs = Math.Min(BaseRetryDelayMs * (1 << (_retryCount - 1)), MaxRetryDelayMs);
                        await Task.Delay(delayMs, _cancellationTokenSource.Token).ConfigureAwait(false);
                    }
                }
                else
                {
                    await Task.Delay(500, _cancellationTokenSource.Token).ConfigureAwait(false); // Avoid tight looping when no messages
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Queue processing cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in message queue processing");
                await Task.Delay(500, _cancellationTokenSource.Token).ConfigureAwait(false); // Delay to prevent tight looping on error
            }
        }

        _logger.LogDebug("Ended processing message queue");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing");
        _cancellationTokenSource.Cancel();
        if (_queueProcessingTask != null && !_queueProcessingTask.IsCompleted)
        {
            _queueProcessingTask.GetAwaiter().GetResult(); // Synchronous wait
        }
        _cancellationTokenSource.Dispose();
        lock (_lockObject)
        {
            _currentMessage = null; // Clear on dispose
        }
        _disposed = true;
    }
}