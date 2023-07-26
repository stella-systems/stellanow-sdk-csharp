// Copyright (C) 2022-2023 Stella Technologies (UK) Limited.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

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
                ClientId = "StellaNowSDK",
                OrganizationId = "23bd77b6-11c1-494d-8881-f636928ccf62",
                ProjectId = "18d41262-07e5-4e8a-9b06-cc238d013d09"
            }
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