using StellaNowSDK.Sinks.Mqtt.ConnectionStrategy;

namespace StellaNowSDK.Sinks.Mqtt;

public record StellaNowMqttSinkOptions(
    bool WithTls = true,
    IMqttConnectionStrategy ConnectionStrategy = null
    
);