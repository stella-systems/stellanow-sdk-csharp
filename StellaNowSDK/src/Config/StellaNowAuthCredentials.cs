namespace StellaNowSDK.Config;

/// <summary>
/// Marker interface for different authentication credential types used by StellaNow.
/// </summary>
public interface IStellaNowAuthCredentials { }

/// <summary>
/// Represents username/password credentials for basic authentication.
/// </summary>
/// <param name="username">The username for authentication.</param>
/// <param name="password">The password for authentication.</param>
public record UserPassAuthCredentials(string username, string password);

/// <summary>
/// Represents credentials for OIDC (OpenID Connect) authentication.
/// </summary>
/// <remarks>
/// This record includes a static property <see cref="OidcClient"/> to identify
/// the OIDC client name used by the StellaNow event-ingestor service.
/// </remarks>
/// <param name="username">The username for OIDC authentication.</param>
/// <param name="password">The password for OIDC authentication.</param>
public record OidcAuthCredentials(string username, string password) : IStellaNowAuthCredentials
{
    /// <summary>
    /// The client identifier used by the StellaNow event-ingestor service for OIDC flows.
    /// </summary>
    public static string OidcClient => "event-ingestor";
}

/// <summary>
/// Represents an API key credential for StellaNow-based authentication.
/// </summary>
/// <param name="ApiKey">The API key string used for authenticating requests.</param>
public record ApiKeyAuthCredentials(string ApiKey) : IStellaNowAuthCredentials;

/// <summary>
/// Represents a no-authentication credential type, often used for local or development scenarios.
/// </summary>
public record NoAuthCredentials() : IStellaNowAuthCredentials;