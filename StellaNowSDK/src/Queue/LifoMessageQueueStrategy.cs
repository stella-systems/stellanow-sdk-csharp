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

using System.Collections.Concurrent;
using StellaNowSDK.Messages;

namespace StellaNowSDK.Queue;

/// <summary>
/// A last-in, first-out (LIFO) queue strategy for storing <see cref="StellaNowEventWrapper"/> messages.
/// </summary>
/// <remarks>
/// Internally uses a <see cref="ConcurrentStack{T}"/> to ensure thread-safe push/pop operations.
/// </remarks>
public sealed class LifoMessageQueueStrategy : IMessageQueueStrategy
{
    private readonly ConcurrentStack<StellaNowEventWrapper> _stack = new();

    /// <inheritdoc />
    public void Enqueue(StellaNowEventWrapper message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _stack.Push(message);
    }

    /// <inheritdoc />
    public bool TryDequeue(out StellaNowEventWrapper? message)
    {
        return _stack.TryPop(out message);
    }

    /// <inheritdoc />
    public bool IsEmpty()
    {
        return _stack.IsEmpty; // Use IsEmpty property for better performance
    }

    /// <inheritdoc />
    public int GetMessageCount()
    {
        return _stack.Count;
    }
}