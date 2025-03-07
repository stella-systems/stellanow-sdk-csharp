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
using StellaNowSDK.Config;
using StellaNowSDK.Sinks;
using StellaNowSDK.Events;
using StellaNowSDK.Messages;
using StellaNowSDK.Types;

namespace StellaNowSDK.Services;

/// <summary>
/// Default implementation of the <see cref="IStellaNowSdk"/> interface, orchestrating
/// message dispatch, connection events, and queue management.
/// </summary>
/// <remarks>
/// This class coordinates the sink (e.g., MQTT connection) and a message queue to ensure reliable 
/// message sending to the StellaNow platform. It also exposes events for connection status changes.
/// </remarks>
public sealed class StellaNowSdk : IStellaNowSdk, IDisposable
{
    private readonly ILogger<StellaNowSdk> _logger; // Required logger
    private readonly StellaNowConfig _config;
    private readonly IStellaNowSink _sink;
    private readonly IStellaNowMessageQueue _messageQueue;
    private readonly object _lockObject = new(); // For thread safety
    private bool _disposed;
    private bool _isStarted;

    /// <inheritdoc />
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;

    /// <inheritdoc />
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;

    /// <inheritdoc />
    public bool IsConnected => _sink?.IsConnected ?? false;

    /// <summary>
    /// Initializes a new instance of the <see cref="StellaNowSdk"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages and errors. Must not be null.</param>
    /// <param name="sink">The sink responsible for connecting to the broker and publishing messages. Must not be null.</param>
    /// <param name="messageQueue">The internal queue used to batch and dispatch messages asynchronously. Must not be null.</param>
    /// <param name="config">Contains organization and project IDs for identifying events in StellaNow. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public StellaNowSdk(
        ILogger<StellaNowSdk> logger,
        IStellaNowSink sink,
        IStellaNowMessageQueue messageQueue,
        StellaNowConfig config)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentNullException.ThrowIfNull(messageQueue);
        ArgumentNullException.ThrowIfNull(config);

        _logger = logger;
        _sink = sink;
        _messageQueue = messageQueue;
        _config = config;

        _sink.ConnectedAsync += OnConnectedAsync;
        _sink.DisconnectedAsync += OnDisconnectedAsync;
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowSdk));

        lock (_lockObject)
        {
            if (_isStarted)
                throw new InvalidOperationException("SDK is already started.");

            _isStarted = true;
        }

        _logger.LogInformation("Starting");
        await _sink.StartAsync().ConfigureAwait(false);
        await _messageQueue.StartProcessingAsync().ConfigureAwait(false);

        _logger.LogInformation("Started");
    }

    /// <inheritdoc />
    public async Task StopAsync(bool waitForEmptyQueue = false, TimeSpan? timeout = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowSdk));

        lock (_lockObject)
        {
            if (!_isStarted)
                throw new InvalidOperationException("SDK is not started.");

            _isStarted = false;
        }

        _logger.LogInformation("Stopping");

        if (waitForEmptyQueue)
        {
            var timeoutValue = timeout ?? TimeSpan.FromMinutes(1); // Default to 1 minute
            using var cts = new CancellationTokenSource(timeoutValue);

            _logger.LogInformation("Waiting for message queue to empty");
            while (!_messageQueue.IsQueueEmpty())
            {
                if (cts.IsCancellationRequested)
                {
                    _logger.LogWarning("Timeout exceeded while waiting for the message queue to empty");
                    break;
                }

                await Task.Delay(100, cts.Token).ConfigureAwait(false);
            }
        }

        await _messageQueue.StopProcessingAsync().ConfigureAwait(false);
        await _sink.StopAsync().ConfigureAwait(false);

        _logger.LogInformation("Stopped");
    }

    /// <inheritdoc />
    public void SendMessage(StellaNowMessageBase message, OnMessageSent? callback = null)
    {
        SendMessage(new StellaNowMessageWrapper(message), callback);
    }

    /// <inheritdoc />
    public void SendMessage(StellaNowMessageWrapper message, OnMessageSent? callback = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowSdk));

        lock (_lockObject)
        {
            if (!_isStarted)
                throw new InvalidOperationException("SDK is not started.");

            ArgumentNullException.ThrowIfNull(message);

            var entityId = message.Metadata.EntityTypeIds?.FirstOrDefault()?.EntityId;
            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new InvalidOperationException(
                    $"Trying to send message with missing entity ID. Message ID: {message.Metadata.MessageId}"
                );
            }

            _messageQueue.EnqueueMessage(
                new StellaNowEventWrapper(
                    new EventKey(_config.organizationId, _config.projectId, entityId),
                    message,
                    callback
                )
            );
        }
    }

    /// <inheritdoc />
    public bool HasMessagesPendingForDispatch()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowSdk));

        return !_messageQueue.IsQueueEmpty();
    }

    /// <inheritdoc />
    public int MessagesPendingForDispatchCount()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StellaNowSdk));

        return _messageQueue.GetMessageCountOnQueue();
    }

    private async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        _logger.LogInformation("Connected");
        if (ConnectedAsync is { } handler)
            await handler(e).ConfigureAwait(false);
    }

    private async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        _logger.LogInformation("Disconnected");
        if (DisconnectedAsync is { } handler)
            await handler(e).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing");
        lock (_lockObject)
        {
            _isStarted = false;
            _sink.ConnectedAsync -= OnConnectedAsync;
            _sink.DisconnectedAsync -= OnDisconnectedAsync;
            if (_sink is IDisposable disposableSink)
                disposableSink.Dispose();
            if (_messageQueue is IDisposable disposableQueue)
                disposableQueue.Dispose();
        }
        _disposed = true;
    }
}