namespace StellaNowSDK.Config;

public interface IStellaNowAuthCredentials { }

public record OidcAuthCredentials(string username, string password) : IStellaNowAuthCredentials
{
    public static string OidcClient => "event-ingestor";
}

public record ApiKeyAuthCredentials(string ApiKey) : IStellaNowAuthCredentials;

public record NoAuthCredentials() : IStellaNowAuthCredentials;