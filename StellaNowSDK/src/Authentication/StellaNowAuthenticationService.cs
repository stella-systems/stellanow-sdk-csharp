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

public class StellaNowAuthenticationService: IStellaNowAuthenticationService
{
    private readonly ILogger<StellaNowAuthenticationService> _logger;
    private readonly StellaNowConfig _config;
    private readonly OidcAuthCredentials _authCredentials;
    private readonly HttpClient _httpClient;
    private readonly string _discoveryDocumentUrl;

    private DiscoveryDocumentResponse? _discoveryDocumentResponse;
    private TokenResponse? _tokenResponse;

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

    public async Task AuthenticateAsync()
    {
        if (!await RefreshTokensAsync())
            await LoginAsync();
    }

    private void ValidateTokenResponse()
    {
        if (_tokenResponse is not null and not { IsError: true }) return;
        
        _logger.LogError($"Failed to authenticate: {_tokenResponse!.Error}");
        _tokenResponse = null;
        throw new Exception("Failed to authenticate.");
    }

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

    public StellaNowAuthenticationResult GetAuthenticationData()
    {
        return new StellaNowAuthTokenResult(_tokenResponse?.AccessToken);
    }
}