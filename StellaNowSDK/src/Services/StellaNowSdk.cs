// Copyright (C) 2022-2023 Stella Technologies (UK) Limited.
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

    private readonly StellaNowConfig _config;
    
    private readonly IStellaNowConnectionStrategy _connectionStrategy;
    private readonly IStellaNowMessageQueue _messageQueue;
    
    public event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;
    public event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;
    
    public bool IsConnected => _connectionStrategy?.IsConnected ?? false;
    
    public StellaNowSdk(
        ILogger<StellaNowSdk>? logger,
        IStellaNowConnectionStrategy connectionStrategy, 
        IStellaNowMessageQueue messageQueue,
        StellaNowConfig config)
    {
        _logger = logger;
        _connectionStrategy = connectionStrategy;
        _config = config;
        _messageQueue = messageQueue;
        
        _connectionStrategy.ConnectedAsync += OnConnectedAsync;
        _connectionStrategy.DisconnectedAsync += OnDisconnectedAsync;
    }

    public async Task StartAsync()
    {
        _logger?.LogInformation("Starting");
        _messageQueue.StartProcessing();
        await _connectionStrategy.StartAsync();
    }

    public async Task StopAsync()
    {
        _logger?.LogInformation("Stopping");
        _messageQueue.StopProcessing();
        await _connectionStrategy.StopAsync();
    }

    public void SendMessage(StellaNowMessageWrapper message, OnMessageSent? callback = null)
    {
        _messageQueue.EnqueueMessage(
            new StellaNowEventWrapper(
                new EventKey(_config.OrganizationId, _config.ProjectId),
                message,
                callback)
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