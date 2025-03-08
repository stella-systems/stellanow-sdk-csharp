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

namespace StellaNowSDK.Queue;

/// <summary>
/// Defines the contract for a message queue strategy in StellaNow.
/// </summary>
/// <remarks>
/// Different implementations can store messages in various data structures or orders (e.g., FIFO, LIFO).
/// Implementations should be thread-safe for concurrent access.
/// </remarks>
public interface IMessageQueueStrategy
{
    /// <summary>
    /// Enqueues the specified <see cref="StellaNowEventWrapper"/> message into the queue.
    /// </summary>
    /// <param name="message">The message to be queued. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    void Enqueue(StellaNowEventWrapper message);

    /// <summary>
    /// Attempts to dequeue a <see cref="StellaNowEventWrapper"/> from the queue.
    /// </summary>
    /// <param name="message">
    /// When this method returns, contains the dequeued message if successful; otherwise, null.
    /// </param>
    /// <returns>
    /// <c>true</c> if a message was successfully dequeued; <c>false</c> if the queue was empty.
    /// </returns>
    bool TryDequeue(out StellaNowEventWrapper? message);

    /// <summary>
    /// Indicates whether the queue is currently empty.
    /// </summary>
    /// <returns><c>true</c> if the queue is empty; otherwise, <c>false</c>.</returns>
    bool IsEmpty();

    /// <summary>
    /// Gets the number of messages currently in the queue.
    /// </summary>
    /// <returns>The count of messages in the queue.</returns>
    int GetMessageCount();
}