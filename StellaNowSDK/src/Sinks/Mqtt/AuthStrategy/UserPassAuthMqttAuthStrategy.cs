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
using MQTTnet.Exceptions;
using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvironmentConfig;
using StellaNowSDK.Exceptions;

namespace StellaNowSDK.Sinks.Mqtt.AuthStrategy;

using Microsoft.Extensions.Logging;
using StellaNowSDK.Exceptions.Sinks.Mqtt;

public class UserPassAuthMqttAuthStrategy : IMqttAuthStrategy
{
    private readonly ILogger<UserPassAuthMqttAuthStrategy> _logger;
    private readonly StellaNowEnvironmentConfig _envConfig;
    private readonly UserPassAuthCredentials _credentials;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPassAuthMqttAuthStrategy"/> class.
    /// </summary>
    /// <param name="logger">Used for logging information, warnings, or errors.</param>
    /// <param name="credentials">Contains the username and password for MQTT authentication. Must not be null.</param>
    /// <param name="envConfig">Environment configuration containing broker URLs. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="credentials"/>, <paramref name="envConfig"/>, or <paramref name="logger"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="credentials.username"/> or <paramref name="envConfig.BrokerUrl"/> is null or whitespace.</exception>
    public UserPassAuthMqttAuthStrategy(
        ILogger<UserPassAuthMqttAuthStrategy> logger,
        UserPassAuthCredentials credentials,
        StellaNowEnvironmentConfig envConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(envConfig);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentials.username, nameof(credentials.username));
        ArgumentException.ThrowIfNullOrWhiteSpace(envConfig.BrokerUrl, nameof(envConfig.BrokerUrl));

        _logger = logger;
        _envConfig = envConfig;
        _credentials = credentials;
    }

    /// <inheritdoc />
    public async Task ConnectAsync(IMqttClient client, string clientId)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        var options = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithConnectionUri(_envConfig.BrokerUrl)
            .WithCredentials(_credentials.username, _credentials.password)
            .Build();

        try
        {
            _logger.LogInformation("Attempting to connect to MQTT broker at {BrokerUrl} with client ID {ClientId} using username {Username}", 
                _envConfig.BrokerUrl, clientId, _credentials.username);
            await client.ConnectAsync(options);
            _logger.LogInformation("Successfully connected to MQTT broker");
        }
        catch (MqttCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker at {BrokerUrl} with username {Username}", 
                _envConfig.BrokerUrl, _credentials.username);
            throw new MqttConnectionException(
                "Failed to connect to the MQTT broker",
                _envConfig.BrokerUrl,
                ex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while connecting to the MQTT broker at {BrokerUrl}", _envConfig.BrokerUrl);
            throw new StellaNowException("An unexpected error occurred while connecting to the MQTT broker", ex);
        }
    }
}