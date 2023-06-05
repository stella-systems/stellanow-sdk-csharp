using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellaNowSDK.Services;

namespace StellaNowSdkTests.ConnectionStrategies;

[TestClass]
public class StellaNowMqttConnectionStrategyTests
{
    private IStellaNowSdk? _stellaSdk;
    private ServiceProvider? _serviceProvider;

    [TestInitialize]
    public async Task TestInitialize()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
            });
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddStellaNowSdk(
            "StellaNowCsharpSDK_orgId",
            "StellaNowCsharpSDK_projId",
            "wss://ingestor.dev-aws.stella.cloud:8443/mqtt",
            "StellaNowCsharpSDK",
            "StellaNowCsharpSDK",
            "test",
            "o1"
        );

        _serviceProvider = services.BuildServiceProvider();

        _stellaSdk = _serviceProvider.GetRequiredService<IStellaNowSdk>();

        await _stellaSdk.StartAsync();

        // StartAsync does not wait for establishing connection, it just starts connection monitor instead.
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
    
    [TestMethod]
    public Task StellaNowMqttConnectionStrategy_Connect()
    {
        Assert.IsTrue(_stellaSdk!.IsConnected);
        return Task.CompletedTask;
    }
    
    [TestMethod]
    public async Task StellaNowMqttConnectionStrategy_Disconnect()
    {
        await _stellaSdk!.StopAsync();
        
        Assert.IsFalse(_stellaSdk.IsConnected);
    }
    
    [TestCleanup]
    public async Task TestCleanup()
    {
        _serviceProvider?.Dispose();
    }
}