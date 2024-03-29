// Copyright (C) 2022-2024 Stella Technologies (UK) Limited.
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
using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Events;
using StellaNowSDK.Messages;
using StellaNowSDK.Types;

namespace StellaNowSDK.Services;

public sealed class StellaNowSdk: IStellaNowSdk, IDisposable
{
    private readonly ILogger<IStellaNowSdk>? _logger;

    private readonly StellaNowCredentials _credentials;
    
    private readonly IStellaNowConnectionStrategy _connectionStrategy;
    private readonly IStellaNowMessageQueue _messageQueue;

    private readonly EventKey _eventKey;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    public bool IsConnected => _connectionStrategy?.IsConnected ?? false;
    
    public StellaNowSdk(
        ILogger<StellaNowSdk>? logger,
        IStellaNowConnectionStrategy connectionStrategy, 
        IStellaNowMessageQueue messageQueue,
        StellaNowCredentials credentials)
    {
        _logger = logger;
        _connectionStrategy = connectionStrategy;
        _credentials = credentials;
        _messageQueue = messageQueue;

        _eventKey = new EventKey(_credentials.OrganizationId, _credentials.ProjectId);
        
        _connectionStrategy.ConnectedAsync += OnConnectedAsync;
        _connectionStrategy.DisconnectedAsync += OnDisconnectedAsync;
    }

    public async Task StartAsync()
    {
        _logger?.LogInformation("Starting");
        _messageQueue.StartProcessing();
        await _connectionStrategy.StartAsync();
    }

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
        await _connectionStrategy.StopAsync();
    }

    public void SendMessage(StellaNowMessageBase message, OnMessageSent? callback = null)
    {
        SendMessage(new StellaNowMessageWrapper(message), callback);
    }
    
    public void SendMessage(StellaNowMessageWrapper message, OnMessageSent? callback = null)
    {
        _messageQueue.EnqueueMessage(
            new StellaNowEventWrapper(_eventKey, message, callback)
        );
    }

    public bool HasMessagesPendingForDispatch()
    {
        return !_messageQueue.IsQueueEmpty();
    }

    public int MessagesPendingForDispatchCount()
    {
        return _messageQueue.GetMessageCountOnQueue();
    }
    
    private async Task OnConnectedAsync(StellaNowConnectedEventArgs e)
    {
        if (ConnectedAsync is { } handler)
        {
            await handler(e);
        }
    }

    private async Task OnDisconnectedAsync(StellaNowDisconnectedEventArgs e)
    {
        if (DisconnectedAsync is { } handler)
        {
            await handler(e);
        }
    }
    
    public void Dispose()
    {
        _logger?.LogDebug("Disposing");
        _connectionStrategy.ConnectedAsync -= OnConnectedAsync;
        _connectionStrategy.DisconnectedAsync -= OnDisconnectedAsync;
    }

}