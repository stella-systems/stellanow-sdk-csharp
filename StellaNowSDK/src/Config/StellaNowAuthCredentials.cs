namespace StellaNowSDK.Config;

public interface IStellaNowAuthCredentials { }

public record UserPassAuthCredentials(string username, string password);

public record OidcAuthCredentials(string username, string password) : IStellaNowAuthCredentials
{
    public static string OidcClient => "event-ingestor";
}

public record ApiKeyAuthCredentials(string ApiKey) : IStellaNowAuthCredentials;

public record NoAuthCredentials() : IStellaNowAuthCredentials;