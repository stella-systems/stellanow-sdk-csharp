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

using Duende.IdentityModel.Client;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using System.Text.Json; // For parsing refresh_expires_in

using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvironmentConfig;
using StellaNowSDK.Exceptions;
using StellaNowSDK.Exceptions.Oidc;
using StellaNowSDK.Exceptions.Sinks.Mqtt;

namespace StellaNowSDK.Sinks.Mqtt.AuthStrategy;

/// <summary>
/// Provides a connection strategy for MQTT using OIDC (OpenID Connect) authentication.
/// </summary>
/// <remarks>
/// This class relies on OIDC authentication to acquire an access token, which is then used as the MQTT username and password.
/// </remarks>
public class OidcMqttAuthStrategy : IMqttAuthStrategy
{
    private readonly ILogger<OidcMqttAuthStrategy> _logger;
    private readonly StellaNowEnvironmentConfig _envConfig;
    private readonly OidcAuthCredentials _authCredentials;
    private readonly HttpClient _httpClient;
    private readonly string _discoveryDocumentUrl;

    private DiscoveryDocumentResponse? _discoveryDocumentResponse;
    private TokenResponse? _tokenResponse;
    private DateTime _tokenIssuedAt;
    private TimeSpan? _refreshTokenLifetime;

    private static readonly TimeSpan DefaultRefreshTokenLifetime = TimeSpan.FromHours(24); // Fallback if refresh_expires_in is missing

    /// <summary>
    /// Initializes a new instance of the <see cref="OidcMqttAuthStrategy"/> class.
    /// </summary>
    /// <param name="logger">Used for logging information, warnings, or errors.</param>
    /// <param name="envConfig">Specifies environment details (e.g., base URLs).</param>
    /// <param name="stellaNowConfig">Contains organization and project IDs for the StellaNow platform.</param>
    /// <param name="authCredentials">OIDC username/password credentials.</param>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance for making OIDC requests.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public OidcMqttAuthStrategy(
        ILogger<OidcMqttAuthStrategy> logger,
        StellaNowEnvironmentConfig envConfig,
        StellaNowConfig stellaNowConfig,
        OidcAuthCredentials authCredentials,
        HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(envConfig);
        ArgumentNullException.ThrowIfNull(stellaNowConfig);
        ArgumentNullException.ThrowIfNull(authCredentials);
        ArgumentNullException.ThrowIfNull(httpClient);

        var orgId = stellaNowConfig.organizationId;
        ArgumentException.ThrowIfNullOrWhiteSpace(orgId, nameof(stellaNowConfig.organizationId));

        _logger = logger;
        _envConfig = envConfig;
        _authCredentials = authCredentials;
        _httpClient = httpClient;
        
        _discoveryDocumentUrl = $"{envConfig.Authority}/realms/{orgId}";
        _tokenIssuedAt = DateTime.MinValue; // Initialize to a default value
        _refreshTokenLifetime = null; // Will be set after token response
    }

    private async Task OidcAuthenticateAsync()
    {
        if (!await RefreshTokensAsync())
            await LoginAsync();
    }

    /// <summary>
    /// Retrieves the latest authentication token data.
    /// </summary>
    /// <returns>The access token if authentication succeeded; otherwise, throws an exception.</returns>
    /// <exception cref="AuthenticationFailedException">
    /// Thrown if no valid token is available or if the token has expired.
    /// </exception>
    private string GetAuthenticationToken()
    {
        if (_tokenResponse?.AccessToken == null)
        {
            throw new AuthenticationFailedException(
                "No valid authentication token available",
                "NoToken"
            );
        }

        // Check if the access token has expired
        if (IsAccessTokenExpired())
        {
            _logger.LogWarning("Access token has expired");
            throw new AuthenticationFailedException(
                "Access token has expired",
                "TokenExpired"
            );
        }

        return _tokenResponse.AccessToken;
    }

    /// <summary>
    /// Checks if the access token has expired based on its issuance time and expiration.
    /// </summary>
    /// <returns><c>true</c> if the token has expired; otherwise, <c>false</c>.</returns>
    private bool IsAccessTokenExpired()
    {
        if (_tokenResponse == null || _tokenResponse.ExpiresIn <= 0)
            return true;

        var expiresIn = TimeSpan.FromSeconds(_tokenResponse.ExpiresIn);
        var expirationTime = _tokenIssuedAt.Add(expiresIn);
        return DateTime.UtcNow >= expirationTime;
    }

    /// <summary>
    /// Checks if the refresh token has expired based on its issuance time and lifetime.
    /// </summary>
    /// <returns><c>true</c> if the refresh token has expired; otherwise, <c>false</c>.</returns>
    private bool IsRefreshTokenExpired()
    {
        if (_tokenResponse == null || string.IsNullOrEmpty(_tokenResponse.RefreshToken))
            return true;

        var lifetime = _refreshTokenLifetime ?? DefaultRefreshTokenLifetime;
        var expirationTime = _tokenIssuedAt.Add(lifetime);
        return DateTime.UtcNow >= expirationTime;
    }

    /// <summary>
    /// Updates the token issuance timestamp and refresh token lifetime when a new token is received.
    /// </summary>
    private void UpdateTokenMetadata()
    {
        _tokenIssuedAt = DateTime.UtcNow;

        // Parse refresh_expires_in from TokenResponse.Raw
        if (_tokenResponse?.Raw != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(_tokenResponse.Raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("refresh_expires_in", out var refreshExpiresElement) &&
                    refreshExpiresElement.TryGetInt32(out var refreshExpiresIn))
                {
                    _refreshTokenLifetime = TimeSpan.FromSeconds(refreshExpiresIn);
                    _logger.LogDebug("Refresh token lifetime set to {RefreshExpiresIn} seconds", refreshExpiresIn);
                }
                else
                {
                    _refreshTokenLifetime = DefaultRefreshTokenLifetime;
                    _logger.LogWarning("refresh_expires_in not found in token response, using default lifetime of {DefaultHours} hours", DefaultRefreshTokenLifetime.TotalHours);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse refresh_expires_in from token response, using default lifetime of {DefaultHours} hours", DefaultRefreshTokenLifetime.TotalHours);
                _refreshTokenLifetime = DefaultRefreshTokenLifetime;
            }
        }
        else
        {
            _refreshTokenLifetime = DefaultRefreshTokenLifetime;
            _logger.LogWarning("Token response raw data is null, using default refresh token lifetime of {DefaultHours} hours", DefaultRefreshTokenLifetime.TotalHours);
        }
    }

    /// <summary>
    /// Validates the current token response to ensure authentication succeeded.
    /// </summary>
    /// <exception cref="AuthenticationFailedException">
    /// Thrown if the token response indicates an error or is missing valid token data.
    /// </exception>
    private void ValidateTokenResponse()
    {
        if (_tokenResponse is { IsError: false }) return;

        _logger.LogError("Failed to authenticate: {Error}", _tokenResponse?.Error);
        
        throw new AuthenticationFailedException(
            "Authentication failed",
            _tokenResponse?.Error ?? "Unknown error"
        );
    }
    
    /// <summary>
    /// Retrieves the OIDC discovery document for the configured realm and caches the result.
    /// </summary>
    /// <returns>
    /// A <see cref="DiscoveryDocumentResponse"/> with the realm’s endpoints (token, authorization, etc.).
    /// </returns>
    /// <exception cref="DiscoveryDocumentRetrievalException">
    /// Thrown if the discovery document cannot be retrieved.
    /// </exception>
    private async Task<DiscoveryDocumentResponse> GetDiscoveryDocumentResponse()
    {
        if (_discoveryDocumentResponse is { IsError: false })
        {
            return _discoveryDocumentResponse;
        }

        _discoveryDocumentResponse = await _httpClient.GetDiscoveryDocumentAsync(_discoveryDocumentUrl);

        if (!_discoveryDocumentResponse.IsError) return _discoveryDocumentResponse;

        _logger.LogError("Error retrieving discovery document: {Error}", _discoveryDocumentResponse.Error);
        
        throw new DiscoveryDocumentRetrievalException(
            $"Failed to retrieve discovery document: {_discoveryDocumentResponse.Error}",
            _discoveryDocumentUrl
        );
    }
    
    /// <summary>
    /// Performs a full login using the direct grant (resource owner password) flow.
    /// </summary>
    /// <remarks>
    /// This method is only called if <see cref="RefreshTokensAsync"/> fails or if no tokens exist.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous login operation.</returns>
    /// <exception cref="AuthenticationFailedException">
    /// Thrown if the login attempt fails due to invalid credentials or server errors.
    /// </exception>
    private async Task LoginAsync()
    {
        _logger.LogInformation("Attempting authentication");
        
        var discoveryDocumentResponse = await GetDiscoveryDocumentResponse();

        try
        {
            _tokenResponse = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                ClientId = OidcAuthCredentials.OidcClient,
                UserName = _authCredentials.username,
                Password = _authCredentials.password
            });

            UpdateTokenMetadata();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed due to an unexpected error");
            throw new AuthenticationFailedException("Login failed due to an unexpected error", "LoginError", ex);
        }

        ValidateTokenResponse();
    
        _logger.LogInformation("Authentication successful");
    }

    /// <summary>
    /// Attempts to refresh the existing access token if one is available and valid.
    /// </summary>
    /// <remarks>
    /// If the current token response is null, invalid, or the access token hasn’t expired, this method may skip the refresh.
    /// </remarks>
    /// <returns>
    /// <c>true</c> if the refresh was successful or the token is still valid; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="AuthenticationFailedException">
    /// Thrown if the refresh token is invalid or the refresh attempt fails.
    /// </exception>
    private async Task<bool> RefreshTokensAsync()
    {
        _logger.LogInformation("Attempting token refresh");

        // No token or token is invalid
        if (_tokenResponse is null or { IsError: true })
        {
            _logger.LogWarning("No valid token response available, skipping refresh");
            return false;
        }

        // Check if the access token is still valid
        if (!IsAccessTokenExpired())
        {
            _logger.LogInformation("Access token is still valid, skipping refresh");
            return true; // Token is still good, no need to refresh
        }

        // Check if the refresh token has expired
        if (IsRefreshTokenExpired())
        {
            _logger.LogWarning("Refresh token has expired, requiring full login");
            return false; // Trigger a full login
        }

        if (string.IsNullOrEmpty(_tokenResponse.RefreshToken))
        {
            _logger.LogWarning("No refresh token available, skipping refresh");
            return false;
        }

        var discoveryDocumentResponse = await GetDiscoveryDocumentResponse();

        try
        {
            _tokenResponse = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                ClientId = OidcAuthCredentials.OidcClient,
                RefreshToken = _tokenResponse.RefreshToken
            });

            UpdateTokenMetadata();
        }
        catch (Exception ex)
        {
            // Check for specific refresh token expiration error
            if (ex.Message.Contains("invalid_grant") || ex.Message.Contains("expired"))
            {
                _logger.LogWarning("Refresh token is likely expired or invalid, requiring full login");
                return false; // Trigger a full login
            }

            _logger.LogError(ex, "Token refresh failed due to an unexpected error");
            throw new AuthenticationFailedException("Token refresh failed", "RefreshError", ex);
        }

        ValidateTokenResponse();
    
        _logger.LogInformation("Token refresh successful");
        
        return true;
    }

    /// <inheritdoc />
    public async Task ConnectAsync(IMqttClient client, string clientId)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        try
        {
            await OidcAuthenticateAsync();
            string accessToken = GetAuthenticationToken();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithConnectionUri(_envConfig.BrokerUrl)
                .WithCredentials(accessToken, accessToken)
                .Build();

            await client.ConnectAsync(options);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Authentication failed during MQTT connection");
            throw;
        }
        catch (DiscoveryDocumentRetrievalException ex)
        {
            _logger.LogError(ex, "Failed to retrieve discovery document during MQTT connection");
            throw;
        }
        catch (MqttCommunicationException ex)
        {
            _logger.LogError(ex, "MQTT connection failed due to a communication error");
            throw new MqttConnectionException(
                "Failed to connect to the MQTT broker",
                _envConfig.BrokerUrl,
                ex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while connecting to the MQTT broker");
            throw new StellaNowException("An unexpected error occurred while connecting to the MQTT broker", ex);
        }
    }
}