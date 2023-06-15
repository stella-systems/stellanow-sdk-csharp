namespace StellaNowSDK.Config;


public abstract class StellaNowEnvironmentConfig
{
    protected abstract string ApiBaseUrl { get; }

    public string AuthUrl => BuildApiUrl("ipm/login");
    public string AuthRefreshUrl => BuildApiUrl("ipm/refresh");
    public abstract string BrokerUrl { get; }

    protected string BuildApiUrl(string path)
    {
        return $"{ApiBaseUrl}/{path}";
    }
}