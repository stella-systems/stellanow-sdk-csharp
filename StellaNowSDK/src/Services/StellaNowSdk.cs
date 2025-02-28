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
/// This class coordinates the sink (e.g. MQTT connection) and a message queue to ensure reliable 
/// message sending to the StellaNow platform. It also exposes events for connection status changes.
/// </remarks>
public sealed class StellaNowSdk: IStellaNowSdk, IDisposable
{
    private readonly ILogger<IStellaNowSdk>? _logger;

    private readonly StellaNowConfig _config;
    
    private readonly IStellaNowSink _sink;
    private readonly IStellaNowMessageQueue _messageQueue;
    
    /// <inheritdoc />
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    
    /// <inheritdoc />
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    /// <inheritdoc />
    public bool IsConnected => _sink?.IsConnected ?? false;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StellaNowSdk"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages and errors.</param>
    /// <param name="sink">The sink responsible for connecting to the broker and publishing messages.</param>
    /// <param name="messageQueue">The internal queue used to batch and dispatch messages asynchronously.</param>
    /// <param name="config">Contains organization and project IDs for identifying events in StellaNow.</param>
    public StellaNowSdk(
        ILogger<StellaNowSdk>? logger,
        IStellaNowSink sink, 
        IStellaNowMessageQueue messageQueue,
        StellaNowConfig config)
    {
        _logger = logger;
        _sink = sink;
        _config = config;
        _messageQueue = messageQueue;
        
        _sink.ConnectedAsync += OnConnectedAsync;
        _sink.DisconnectedAsync += OnDisconnectedAsync;
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        _logger?.LogInformation("Starting");
        _messageQueue.StartProcessing();
        await _sink.StartAsync();
    }

    /// <inheritdoc />
    public async Task StopAsync(bool waitForEmptyQueue = false, TimeSpan? timeout = null)
    {
        _logger?.LogInformation("Stopping");
        
        if (waitForEmptyQueue)
        {
            var cancellationTokenSource = new CancellationTokenSource(timeout ?? TimeSpan.FromMinutes(1)); // default to 1 minute timeout

            _logger?.LogInformation("Waiting for message queue to empty");
            while (this.HasMessagesPendingForDispatch())
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    _logger?.LogWarning("Timeout exceeded while waiting for the message queue to empty");
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100)); // delay can be adjusted based on needs
            }
        }
        
        _messageQueue.StopProcessing();
        await _sink.StopAsync();
    }

    /// <inheritdoc />
    public void SendMessage(StellaNowMessageBase message, OnMessageSent? callback = null)
    {
        SendMessage(new StellaNowMessageWrapper(message), callback);
    }
    
    /// <inheritdoc />
    public void SendMessage(StellaNowMessageWrapper message, OnMessageSent? callback = null)
    {
        var entityId = message.Metadata.EntityTypeIds!.FirstOrDefault()?.EntityId;

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new InvalidOperationException(
                $"Trying to send message with missing entity id. Message ID: {message.Metadata.MessageId}"
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

    /// <inheritdoc />
    public bool HasMessagesPendingForDispatch()
    {
        return !_messageQueue.IsQueueEmpty();
    }

    /// <inheritdoc />
    public int MessagesPendingForDispatchCount()
    {
        return _messageQueue.GetMessageCountOnQueue();
    }
    
    /// <summary>
    /// Internal handler for when the sink connects, raising the <see cref="ConnectedAsync"/> event.
    /// </summary>
    /// <param name="e">Connection event arguments.</param>
    private async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        if (ConnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    /// <summary>
    /// Internal handler for when the sink disconnects, raising the <see cref="DisconnectedAsync"/> event.
    /// </summary>
    /// <param name="e">Disconnection event arguments.</param>

    private async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        if (DisconnectedAsync is { } handler)
        {
            await handler(e);
        }
    }
    
    /// <summary>
    /// Unsubscribes from sink events and releases resources used by this SDK.
    /// </summary>
    public void Dispose()
    {
        _logger?.LogDebug("Disposing");
        _sink.ConnectedAsync -= OnConnectedAsync;
        _sink.DisconnectedAsync -= OnDisconnectedAsync;
    }

}