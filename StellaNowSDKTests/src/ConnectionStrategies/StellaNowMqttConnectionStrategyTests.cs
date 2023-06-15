using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellaNowSDK.Config;
using StellaNowSDK.Enums;
using StellaNowSDK.Services;
using StellaNowSDKTests.Messages;

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
            StellaNowEnvironment.Development,
            new StellaNowConfig()
            {
                ApiKey = "username10@some.domain",
                ApiSecret = "1234567890",
                ClientId = "StellaNowCsharpSDK",
                OrganizationId = "23bd77b6-11c1-494d-8881-f636928ccf62",
                ProjectId = "18d41262-07e5-4e8a-9b06-cc238d013d09"
            }
        );

        _serviceProvider = services.BuildServiceProvider();

        _stellaSdk = _serviceProvider.GetRequiredService<IStellaNowSdk>();

        _stellaSdk.StartAsync().Wait();
        // StartAsync does not wait for establishing connection, it just starts connection monitor instead.
        Task.Delay(TimeSpan.FromSeconds(5)).Wait();
    }
    
    [TestMethod]
    public Task StellaNowMqttConnectionStrategy_Connect()
    {
        Assert.IsTrue(_stellaSdk!.IsConnected);
        return Task.CompletedTask;
    }
    
    [TestMethod]
    public async Task StellaNowMqttConnectionStrategy_SendMessage()
    {
        Assert.IsTrue(_stellaSdk!.IsConnected);

        // Create a new message
        var message = new UserUpdateMessage(
            "punter1",
            "John",
            "Doe",
            "1980-01-01",
            "john.doe@example.com"
        );

        // Send the message
        _stellaSdk.SendMessage(message);

        // Wait some time to ensure the message is delivered
        await Task.Delay(TimeSpan.FromSeconds(5));

        // TODO: Add verification that message was received if possible.
    }
    
    // [TestMethod]
    // public async Task StellaNowMqttConnectionStrategy_Disconnect()
    // {
    //     await _stellaSdk!.StopAsync();
    //     
    //     Assert.IsFalse(_stellaSdk.IsConnected);
    // }
    //
    // [TestCleanup]
    // public async Task TestCleanup()
    // {
    //     _serviceProvider?.Dispose();
    // }
}