using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Queue;
using StellaNowSDK.Services;

namespace StellaNowSdkTests.ConnectionStrategies;

[TestClass]
public class StellaNowMqttConnectionStrategyTests
{
    private StellaNowSdk _stellaSdk;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _stellaSdk = new StellaNowSdk(
            new StellaNowMqttWebSocketStrategy(
                "wss://ingestor.dev-aws.stella.cloud:8443/mqtt",
                "StellaNowCsharpSDK",
                "StellaNowCsharpSDK",
                "test",
                "o1"
            ),
            new FifoMessageQueueStrategy(),
            "StellaNowCsharpSDK_orgId",
            "StellaNowCsharpSDK_projId"
        );

        await _stellaSdk.StartAsync();
        
        // StartAsync does not wait for establishing connection, it just starts connection monitor instead.
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public async Task StellaNowMqttConnectionStrategy_Connect()
    {
        Assert.IsTrue(_stellaSdk.IsConnected);
    }
    
    [TestMethod]
    public async Task StellaNowMqttConnectionStrategy_Disconnect()
    {
        await _stellaSdk.StopAsync();
        
        Assert.IsFalse(_stellaSdk.IsConnected);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await _stellaSdk.StopAsync();
    }
}