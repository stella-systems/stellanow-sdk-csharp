namespace StellaNowSDK.Config;

public class StellaNowIntEnvironmentConfig : StellaNowEnvironmentConfig
{
    public override string AuthUrl => "https://api.int.stella.cloud/ipm/login";
    public override string AuthRefreshUrl => "https://api.int.stella.cloud/ipm/refresh";
    public override string BrokerUrl => "wss://ingestor.int.stella.cloud:8443/mqtt";
}