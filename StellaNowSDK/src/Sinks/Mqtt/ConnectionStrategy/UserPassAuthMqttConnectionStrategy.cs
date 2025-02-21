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

using MQTTnet.Client;
using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvirnmentConfig;

namespace StellaNowSDK.Sinks.Mqtt.ConnectionStrategy;

public class UserPassAuthMqttConnectionStrategy : IMqttConnectionStrategy
{
    private readonly StellaNowEnvironmentConfig _envConfig;
    private readonly UserPassAuthCredentials _credentials;
    
    public UserPassAuthMqttConnectionStrategy(
        UserPassAuthCredentials credentials,
        StellaNowEnvironmentConfig envConfig)
    {
        _envConfig = envConfig;
        _credentials = credentials;
    }

    public async void ConnectAsync(IMqttClient client, string clientId)
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithConnectionUri(_envConfig.BrokerUrl)
            .WithCredentials(_credentials.username, _credentials.password)
            .Build();

        await client.ConnectAsync(options);
    }
}