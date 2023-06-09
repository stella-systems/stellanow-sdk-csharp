namespace StellaNowSDK.Config;

public class StellaNowIntEnvironmentConfig : StellaNowEnvironmentConfig
{
    public override string AuthUrl => "your-dev-auth-url";
    public override string AuthRefreshUrl => "your-dev-auth-refresh-url";
    public override string BrokerUrl => "your-dev-mqtt-url";
}