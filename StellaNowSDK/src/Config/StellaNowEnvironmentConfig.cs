namespace StellaNowSDK.Config;

public abstract class StellaNowEnvironmentConfig
{
    public abstract string AuthUrl { get; }
    public abstract string AuthRefreshUrl { get; }
    public abstract string BrokerUrl { get; }
}