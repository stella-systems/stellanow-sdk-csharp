namespace StellaNowSDK.Config;

public class StellaNowDevEnvironmentConfig : StellaNowEnvironmentConfig
{
    protected override string ApiBaseUrl => "https://api.dev-aws.stella.cloud";
    public override string BrokerUrl => "wss://ingestor.dev-aws.stella.cloud:8443/mqtt";
}