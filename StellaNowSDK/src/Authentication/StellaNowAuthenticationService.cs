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
using StellaNowSDK.Config;
using StellaNowSDK.Config.EnvirnmentConfig;

namespace StellaNowSDK.Authentication;

/// <summary>
/// Provides an OIDC-based authentication service for StellaNow, handling login
/// and token refresh using a direct grant flow (resource owner password credentials).
/// </summary>
/// <remarks>
/// This service relies on <see cref="OidcAuthCredentials"/> and <see cref="IdentityModel"/> 
/// to request and refresh tokens from Keycloak or a compatible OIDC provider.
/// This is used for <see cref=IStellaNowSinks/> that are protected through authorisation 
/// </remarks>
public class StellaNowAuthenticationService: IStellaNowAuthenticationService
{
    private readonly ILogger<StellaNowAuthenticationService> _logger;
    private readonly StellaNowConfig _config;
    private readonly OidcAuthCredentials _authCredentials;
    private readonly HttpClient _httpClient;
    private readonly string _discoveryDocumentUrl;

    private DiscoveryDocumentResponse? _discoveryDocumentResponse;
    private TokenResponse? _tokenResponse;

    /// <summary>
    /// Initializes a new instance of the <see cref="StellaNowAuthenticationService"/> class.
    /// </summary>
    /// <param name="logger">Used for logging information, warnings, or errors.</param>
    /// <param name="envConfig">Specifies environment details (e.g., base URLs).</param>
    /// <param name="config">Contains organization and project IDs for the StellaNow platform.</param>
    /// <param name="authCredentials">OIDC username/password credentials.</param>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance for making OIDC requests.</param>
    public StellaNowAuthenticationService(
        ILogger<StellaNowAuthenticationService> logger,
        StellaNowEnvironmentConfig envConfig,
        StellaNowConfig config,
        OidcAuthCredentials authCredentials,
        HttpClient httpClient)
    {
        _logger = logger;
        _config = config;
        _authCredentials = authCredentials;
        _httpClient = httpClient;
        _discoveryDocumentUrl = $"{envConfig.Authority}/realms/{_config.organizationId}";
    }

    /// <inheritdoc />
    public async Task AuthenticateAsync()
    {
        if (!await RefreshTokensAsync())
            await LoginAsync();
    }

    /// <summary>
    /// Validates the current token response to ensure authentication succeeded.
    /// </summary>
    /// <exception cref="Exception">
    /// Thrown if the token response indicates an error or is missing valid token data.
    /// </exception>
    private void ValidateTokenResponse()
    {
        if (_tokenResponse is not null and not { IsError: true }) return;
        
        _logger.LogError($"Failed to authenticate: {_tokenResponse!.Error}");
        _tokenResponse = null;
        throw new Exception("Failed to authenticate.");
    }

    /// <summary>
    /// Retrieves the OIDC discovery document for the configured realm and caching the result.
    /// </summary>
    /// <returns>
    /// A <see cref="DiscoveryDocumentResponse"/> with the realm’s endpoints (token, authorization, etc.).
    /// </returns>
    /// <exception cref="Exception">
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
        
        _logger.LogError($"Error retrieving discovery document: {_discoveryDocumentResponse.Error}");
        _discoveryDocumentResponse = null;
        throw new Exception("Could not retrieve discovery document");

    }

    /// <summary>
    /// Performs a full login using the direct grant (resource owner password) flow.
    /// </summary>
    /// <remarks>
    /// This method is only called if <see cref="RefreshTokensAsync"/> fails or if no tokens exist.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous login operation.</returns>
    /// <exception cref="Exception">
    /// Thrown if the login attempt fails due to invalid credentials or server errors.
    /// </exception>
    private async Task LoginAsync()
    {
        _logger?.LogInformation("Attempting to Authentication");
        
        var discoveryDocumentResponse = await GetDiscoveryDocumentResponse();

        _tokenResponse = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = discoveryDocumentResponse.TokenEndpoint,
            ClientId = OidcAuthCredentials.OidcClient,
            UserName = _authCredentials.username,
            Password = _authCredentials.password
        });

        ValidateTokenResponse();
    
        _logger?.LogInformation("Authentication Successful");
    }

    /// <summary>
    /// Attempts to refresh the existing access token if one is available and valid.
    /// </summary>
    /// <remarks>
    /// If the current token response is null or invalid, this method will return <c>false</c> immediately,
    /// signaling that a new login is required.
    /// </remarks>
    /// <returns>
    /// <c>true</c> if the refresh was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if the refresh token is invalid or the refresh attempt fails.
    /// </exception>
    private async Task<bool> RefreshTokensAsync()
    {
        _logger?.LogInformation("Attempting Token Refresh");

        if (_tokenResponse is null or { IsError: true }) return false;
        
        var discoveryDocumentResponse = await GetDiscoveryDocumentResponse();

        _tokenResponse = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address = discoveryDocumentResponse.TokenEndpoint,
            ClientId = OidcAuthCredentials.OidcClient,
            RefreshToken = _tokenResponse!.RefreshToken!
        });

        ValidateTokenResponse();
    
        _logger?.LogInformation("Token refresh successful");
        
        return true;
    }

    /// <inheritdoc />
    public StellaNowAuthenticationResult GetAuthenticationData()
    {
        return new StellaNowAuthTokenResult(_tokenResponse?.AccessToken);
    }
}