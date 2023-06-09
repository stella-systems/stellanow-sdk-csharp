using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StellaNowSDK.Config;

namespace StellaNowSDK.Authentication;

public class StellaNowAuthenticationService
{
    private readonly ILogger<StellaNowAuthenticationService> _logger;
    private readonly StellaNowConfig _config;
    private readonly StellaNowEnvironmentConfig _envConfig;

    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;

    public StellaNowAuthenticationService(
        ILogger<StellaNowAuthenticationService> logger,
        StellaNowEnvironmentConfig envConfig,
        StellaNowConfig config)
    {
        _logger = logger;
        _envConfig = envConfig;
        _config = config;
    }

    public async Task AuthenticateAsync()
    {
        if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_refreshToken))
        {
            await LoginAsync();
        }
        else
        {
            var refreshSucceeded = await RefreshTokensAsync();
            
            if (!refreshSucceeded)
            {
                await LoginAsync();
            }
        }
    }

    private async Task LoginAsync()
    {
        var loginPayload = new 
        {
            realm_id = _config.OrganizationId,
            username = _config.ApiKey,
            email = _config.ApiKey,
            password = _config.ApiSecret,
            client_id = "backoffice"
        };

        var httpClient = new HttpClient();

        var httpResponse = await httpClient.PostAsync(
            _envConfig.AuthUrl, 
            new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json"));
        
        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger?.LogError("Failed to Authenticate");
            throw new Exception("Failed to authenticate.");
        }
    
        var responseString = await httpResponse.Content.ReadAsStringAsync();
        var authResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

        _accessToken = authResponse.details.access_token;
        _refreshToken = authResponse.details.refresh_token;
    
        _logger?.LogInformation("Authentication Successful");
    }

    private async Task<bool> RefreshTokensAsync()
    {
        // Call the /ipm/refresh route with the refresh token and handle the response
        // If it succeeds, update the access token and return true
        // If it fails, return false

        await AuthenticateAsync();

        return true;
    }

    public string GetAccessToken()
    {
        return _accessToken;
    }
}