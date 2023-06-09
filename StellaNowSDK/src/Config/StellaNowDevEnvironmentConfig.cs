namespace StellaNowSDK.Config;

public class StellaNowDevEnvironmentConfig : StellaNowEnvironmentConfig
{
    public override string AuthUrl => "https://api.dev-aws.stella.cloud/ipm/login";
    public override string AuthRefreshUrl => "https://api.dev-aws.stella.cloud/ipm/refresh";
    public override string BrokerUrl => "wss://ingestor.dev-aws.stella.cloud:8443/mqtt";
}