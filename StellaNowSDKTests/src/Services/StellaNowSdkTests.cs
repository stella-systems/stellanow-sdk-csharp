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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellaNowSDK.Config;
using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Services;
using StellaNowSDK.Types;
using StellaNowSDK.Messages;
using StellaNowSdkTests.TestUtilities;

namespace StellaNowSdkTests.Services
{
    [TestClass]
    public class StellaNowSdkTests
    {
        private Mock<IStellaNowMessageQueue>? _mockMessageQueue;
        private Mock<IStellaNowConnectionStrategy>? _mockConnectionStrategy;
        private StellaNowSdk? _sdk;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize a mock for the IStellaNowMessageQueue
            _mockMessageQueue = new Mock<IStellaNowMessageQueue>();

            // Initialize a mock for the IStellaNowConnectionStrategy
            _mockConnectionStrategy = new Mock<IStellaNowConnectionStrategy>();

            // Create an instance of StellaNowSdk with the mocked dependencies
            _sdk = new StellaNowSdk(
                null, // pass null for the logger for simplicity
                _mockConnectionStrategy.Object, 
                _mockMessageQueue.Object, 
                new StellaNowConfig() // use default configuration
            );
        }

        [TestMethod]
        public void Test_HasMessagesPendingForDispatch()
        {
            // Setup the mock to return true when IsQueueEmpty is called
            _mockMessageQueue.Setup(mq => mq.IsQueueEmpty()).Returns(false);

            // Call the method under test
            var result = _sdk.HasMessagesPendingForDispatch();

            // Assert that the result is true
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_MessagesPendingForDispatchCount()
        {
            // Setup the mock to return 5 when GetMessageCountOnQueue is called
            _mockMessageQueue.Setup(mq => mq.GetMessageCountOnQueue()).Returns(5);

            // Call the method under test
            var result = _sdk.MessagesPendingForDispatchCount();

            // Assert that the result is 5
            Assert.AreEqual(5, result);
        }
        
        [TestMethod]
        public async Task SendMessage_WhenMessageIsSent_CallbackIsCalled()
        {
            // Arrange
            var messageSent = false;
            var message = new UserUpdateMessage(
                Guid.NewGuid().ToString(),"John", "Doe", "1970-01-01", "john.doe@example.com"
            );
            var callback = new OnMessageSent((_) => messageSent = true);
    
            // Configure the mock MessageQueue to simulate successful message sending
            _mockMessageQueue.Setup(mq => mq.EnqueueMessage(It.IsAny<StellaNowEventWrapper>()))
                .Callback<StellaNowEventWrapper>(wrapper => wrapper.Callback?.Invoke(wrapper));
        
            // Act
            _sdk.SendMessage(message, callback);

            // Assert
            Assert.IsTrue(messageSent);
        }
    }
}