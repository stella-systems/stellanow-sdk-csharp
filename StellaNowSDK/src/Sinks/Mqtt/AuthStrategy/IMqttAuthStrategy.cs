// Copyright (C) 2023-2025 Stella Technologies (UK) Limited.
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

using MQTTnet.Client;

namespace StellaNowSDK.Sinks.Mqtt.AuthStrategy;

/// <summary>
/// Defines a strategy for connecting an MQTT client, supporting various authentication mechanisms
/// (e.g., no-auth, username/password, OIDC).
/// </summary>
public interface IMqttAuthStrategy
{
    /// <summary>
    /// Connects the specified <paramref name="client"/> using a particular authentication strategy.
    /// </summary>
    /// <param name="client">The MQTT client to connect. Must not be null.</param>
    /// <param name="clientId">The unique client ID for the MQTT session. Must not be null or whitespace.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous connect operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="clientId"/> is null or whitespace.</exception>
    /// <exception cref="MqttCommunicationException">Thrown if the MQTT connection fails due to a communication error from the MQTT broker.</exception>
    /// <exception cref="MqttConnectionException">Thrown if the MQTT connection fails, wrapping lower-level communication errors with additional context.</exception>
    /// <exception cref="StellaNowException">Thrown for unexpected errors during the connection process that are not specific to MQTT communication.</exception>
    /// <exception cref="AuthenticationFailedException">Thrown if authentication (e.g., OIDC) fails during the connection process (specific to OIDC-based strategies).</exception>
    /// <exception cref="DiscoveryDocumentRetrievalException">Thrown if the OIDC discovery document cannot be retrieved (specific to OIDC-based strategies).</exception>
    Task ConnectAsync(IMqttClient client, string clientId);
}