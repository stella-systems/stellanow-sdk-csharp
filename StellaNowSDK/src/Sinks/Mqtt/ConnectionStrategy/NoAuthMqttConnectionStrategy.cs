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
using MQTTnet.Client;
using NanoidDotNet;
using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvirnmentConfig;

namespace StellaNowSDK.Sinks.Mqtt.ConnectionStrategy;

public class NoAuthMqttConnectionStrategy : IMqttConnectionStrategy
{
    private readonly StellaNowConfig _config;
    private readonly StellaNowEnvironmentConfig _envConfig;
    private readonly string _clientId;
    
    public NoAuthMqttConnectionStrategy(
        ILogger<NoAuthMqttConnectionStrategy>? logger,
        StellaNowEnvironmentConfig envConfig,
        StellaNowConfig config)
    {
        _config = config;
        _envConfig = envConfig;
        
        _clientId = $"StellaNowSDK_{Nanoid.Generate(size: 10)}";
    }

    public async void ConnectAsync(IMqttClient client)
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId(_clientId)
            .WithWebSocketServer(_envConfig.BrokerUrl)
            .Build();

        await client.ConnectAsync(options);
    }
}