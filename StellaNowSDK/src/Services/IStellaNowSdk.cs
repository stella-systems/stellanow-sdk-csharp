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

using StellaNowSDK.Events;
using StellaNowSDK.Messages;
using StellaNowSDK.Types;

namespace StellaNowSDK.Services;

/// <summary>
/// Primary interface for interacting with StellaNow services. Handles message sending,
/// connection lifecycle, and queue management.
/// </summary>
/// <remarks>
/// An implementation of this interface is typically responsible for connecting to a sink broker
///  and queueing messages.
/// </remarks>
public interface IStellaNowSdk
{
    /// <summary>
    /// Gets a value indicating whether the SDK is currently connected to the broker.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Occurs when the SDK (through its underlying sink) successfully connects to the broker.
    /// </summary>
    event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;

    /// <summary>
    /// Occurs when the SDK (through its underlying sink) disconnects from the broker.
    /// </summary>
    event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;

    /// <summary>
    /// Starts the SDK by initiating the message queue and establishing a connection to the broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task StartAsync();

    /// <summary>
    /// Stops the SDK, optionally waiting for the queue to empty before disconnecting.
    /// </summary>
    /// <param name="waitForEmptyQueue">
    /// If <c>true</c>, waits until all queued messages are dispatched before stopping.
    /// </param>
    /// <param name="timeout">
    /// The maximum time to wait for the queue to empty. Defaults to infnite if not specified.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous stop operation.</returns>
    Task StopAsync(bool waitForEmptyQueue = false, TimeSpan? timeout = null);

    /// <summary>
    /// Sends a <see cref="StellaNowMessageBase"/> by wrapping it into a <see cref="StellaNowMessageWrapper"/>
    /// and enqueueing it for dispatch.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="callback">
    /// An optional callback invoked once the message has been dispatched to the broker.
    /// </param>
    void SendMessage(StellaNowMessageBase message, OnMessageSent? callback = null);

    /// <summary>
    /// Sends a <see cref="StellaNowMessageWrapper"/> directly by enqueueing it for dispatch.
    /// </summary>
    /// <param name="message">The wrapped message containing metadata and JSON payload.</param>
    /// <param name="callback">
    /// An optional callback invoked once the message has been dispatched to the broker.
    /// </param>
    void SendMessage(StellaNowMessageWrapper message, OnMessageSent? callback = null);

    /// <summary>
    /// Determines whether there are any messages pending dispatch in the queue.
    /// </summary>
    /// <returns><c>true</c> if the queue has messages waiting; otherwise, <c>false</c>.</returns>
    bool HasMessagesPendingForDispatch();

    /// <summary>
    /// Gets the number of messages currently pending dispatch in the queue.
    /// </summary>
    /// <returns>The count of messages in the queue.</returns>
    int MessagesPendingForDispatchCount();
}