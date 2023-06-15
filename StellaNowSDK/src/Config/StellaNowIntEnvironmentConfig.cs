namespace StellaNowSDK.Config;

public class StellaNowIntEnvironmentConfig : StellaNowEnvironmentConfig
{
    protected override string ApiBaseUrl => "https://api.int.stella.cloud";
    public override string BrokerUrl => "wss://ingestor.int.stella.cloud:8443/mqtt";
}