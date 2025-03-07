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

using StellaNowSDK.Messages;

namespace StellaNowSDK.Services;

/// <summary>
/// Defines a queue for managing and dispatching <see cref="StellaNowEventWrapper"/> messages.
/// </summary>
/// <remarks>
/// Implementations should handle asynchronous message processing, allowing messages to be enqueued
/// and later sent to a sink (e.g., MQTT). The queue may implement strategies for ordering (FIFO/LIFO)
/// or persistence and can be started or stopped on demand. Implementations should be thread-safe
/// when accessed concurrently.
/// </remarks>
public interface IStellaNowMessageQueue : IDisposable
{
    /// <summary>
    /// Begins processing enqueued messages in the background.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous start operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the queue is already started or has been disposed.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the queue has been disposed.</exception>
    Task StartProcessingAsync();

    /// <summary>
    /// Stops processing messages, causing the queue to cease dequeuing and dispatching.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous stop operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the queue is not started.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the queue has been disposed.</exception>
    Task StopProcessingAsync();

    /// <summary>
    /// Enqueues a <see cref="StellaNowEventWrapper"/> message for later dispatch.
    /// </summary>
    /// <param name="message">The event wrapper to add to the queue. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the queue has been disposed.</exception>
    void EnqueueMessage(StellaNowEventWrapper message);

    /// <summary>
    /// Checks whether the queue is empty.
    /// </summary>
    /// <returns><c>true</c> if no messages are waiting; otherwise, <c>false</c>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the queue has been disposed.</exception>
    bool IsQueueEmpty();

    /// <summary>
    /// Retrieves the current number of messages waiting in the queue.
    /// </summary>
    /// <returns>The count of enqueued messages.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the queue has been disposed.</exception>
    int GetMessageCountOnQueue();
}