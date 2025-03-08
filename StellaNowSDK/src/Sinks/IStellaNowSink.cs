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

namespace StellaNowSDK.Sinks;

/// <summary>
/// Defines a sink for sending messages to the StellaNow platform, as well as starting and stopping
/// connections to a message broker.
/// </summary>
public interface IStellaNowSink
{
    /// <summary>
    /// Raised when the sink successfully connects to the broker.
    /// May be null if no handlers are subscribed.
    /// </summary>
    event Func<StellaNowConnectedEventArgs, Task>? ConnectedAsync;

    /// <summary>
    /// Raised when the sink disconnects from the broker.
    /// May be null if no handlers are subscribed.
    /// </summary>
    event Func<StellaNowDisconnectedEventArgs, Task>? DisconnectedAsync;

    /// <summary>
    /// Indicates whether the sink is currently connected to the broker.
    /// This value should be thread-safe and reflect the latest connection state.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Starts the sink, typically establishing a connection to the broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="MqttConnectionException">Thrown if the connection to the broker fails.</exception>
    /// <exception cref="StellaNowException">Thrown for unexpected errors during the start process.</exception>
    Task StartAsync();

    /// <summary>
    /// Stops the sink, disconnecting from the broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="StellaNowException">Thrown for unexpected errors during the stop process.</exception>
    Task StopAsync();

    /// <summary>
    /// Sends a StellaNow event message asynchronously to the sink.
    /// </summary>
    /// <param name="message">The event message wrapper containing payload and metadata. Must not be null.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    /// <exception cref="MqttConnectionException">Thrown if the sink is not connected or the message cannot be sent.</exception>
    /// <exception cref="StellaNowException">Thrown for unexpected errors during the send process.</exception>
    Task SendMessageAsync(StellaNowEventWrapper message);
}