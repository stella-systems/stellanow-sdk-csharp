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

namespace StellaNowSDK.Config.EnvironmentConfig;

/// <summary>
/// Development environment configuration pointing to dev StellaNow endpoints.
/// </summary>
/// <remarks>
/// Uses a dev API base URL and a WebSocket-secured MQTT broker URL on the dev environment.
/// </remarks>
public class StellaNowDevEnvironmentConfig : StellaNowEnvironmentConfig
{
    /// <inheritdoc/>
    protected override string ApiBaseUrl => "https://api.dev.stella.cloud";
    
    /// <inheritdoc/>
    public override string BrokerUrl => "wss://ingestor.dev.stella.cloud:8083/mqtt";
}