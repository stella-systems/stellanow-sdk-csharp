using Microsoft.VisualStudio.TestTools.UnitTesting;
using MQTTnet.Client;
using StellaNowSDK.Services;

namespace MqttSdkTests.tests;

[TestClass]
public class StellaNowSdkTests
{
    [TestMethod]
    public async Task ConnectAsync_SuccessfulConnection_ConnectedEventTriggered()
    {
        // Arrange
        // var mqttSdk = new StellaNowSdk("ws://yourbrokerurl", "stella-now-sdkC#");
        //
        // var connectedEventTriggered = new TaskCompletionSource<bool>();
        // mqttSdk.OnConnectedAsync += (MqttClientConnectedEventArgs args) =>
        // {
        //     connectedEventTriggered.SetResult(true);
        //     return Task.CompletedTask;
        // };
        //
        // // Act
        // await mqttSdk.ConnectAsync();
        //
        // // Assert
        // var wasConnectedEventTriggered = await Task.WhenAny(connectedEventTriggered.Task, Task.Delay(5000)) == connectedEventTriggered.Task;
        // Assert.IsTrue(wasConnectedEventTriggered);
    }
}