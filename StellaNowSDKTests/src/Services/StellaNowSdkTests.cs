// Copyright (C) 2022-2025 Stella Technologies (UK) Limited.
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

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellaNowSDK.Config;
using StellaNowSDK.Sinks;
using StellaNowSDK.Services;
using StellaNowSDK.Types;
using StellaNowSDK.Messages;
using StellaNowSdkTests.TestUtilities;
using System;
using System.Threading.Tasks;

namespace StellaNowSdkTests.Services
{
    [TestClass]
    public class StellaNowSdkTests
    {
        private Mock<IStellaNowMessageQueue>? _mockMessageQueue;
        private Mock<IStellaNowSink>? _mockConnectionStrategy;
        private Mock<ILogger<StellaNowSdk>>? _mockLogger;
        private StellaNowSdk? _sdk;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize a mock for the ILogger<StellaNowSdk>
            _mockLogger = new Mock<ILogger<StellaNowSdk>>();

            // Initialize a mock for the IStellaNowMessageQueue
            _mockMessageQueue = new Mock<IStellaNowMessageQueue>();

            // Initialize a mock for the IStellaNowSink
            _mockConnectionStrategy = new Mock<IStellaNowSink>();
            _mockConnectionStrategy.Setup(s => s.IsConnected).Returns(true); // Default to connected

            // Create an instance of StellaNowSdk with the mocked dependencies
            _sdk = new StellaNowSdk(
                _mockLogger.Object,
                _mockConnectionStrategy.Object,
                _mockMessageQueue.Object,
                new StellaNowConfig("", "") // Use default configuration
            );
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _sdk?.Dispose();
            _sdk = null;
            _mockMessageQueue = null;
            _mockConnectionStrategy = null;
            _mockLogger = null;
        }

        [TestMethod]
        public void Test_HasMessagesPendingForDispatch_WhenQueueNotEmpty_ReturnsTrue()
        {
            // Arrange
            _mockMessageQueue!.Setup(mq => mq.IsQueueEmpty()).Returns(false);

            // Act
            var result = _sdk!.HasMessagesPendingForDispatch();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_HasMessagesPendingForDispatch_WhenQueueEmpty_ReturnsFalse()
        {
            // Arrange
            _mockMessageQueue!.Setup(mq => mq.IsQueueEmpty()).Returns(true);

            // Act
            var result = _sdk!.HasMessagesPendingForDispatch();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_MessagesPendingForDispatchCount_ReturnsCorrectCount()
        {
            // Arrange
            _mockMessageQueue!.Setup(mq => mq.GetMessageCountOnQueue()).Returns(5);

            // Act
            var result = _sdk!.MessagesPendingForDispatchCount();

            // Assert
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task SendMessage_WhenMessageIsSent_CallbackIsCalled()
        {
            // Arrange
            var messageSent = false;
            var message = new UserUpdateMessage(
                Guid.NewGuid().ToString(), "John", "Doe", "1970-01-01", "john.doe@example.com"
            );
            var callback = new OnMessageSent((_) => messageSent = true);

            _mockMessageQueue!.Setup(mq => mq.EnqueueMessage(It.IsAny<StellaNowEventWrapper>()))
                .Callback<StellaNowEventWrapper>(wrapper => wrapper.Callback?.Invoke(wrapper));

            // Simulate SDK being started
            _mockConnectionStrategy!.Setup(cs => cs.StartAsync()).Returns(Task.CompletedTask);
            _mockMessageQueue.Setup(mq => mq.StartProcessingAsync()).Returns(Task.CompletedTask);
            await _sdk!.StartAsync();

            // Act
            _sdk.SendMessage(message, callback);

            // Assert
            Assert.IsTrue(messageSent);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SendMessage_WhenNotStarted_ThrowsInvalidOperationException()
        {
            // Arrange
            var message = new UserUpdateMessage(
                Guid.NewGuid().ToString(), "John", "Doe", "1970-01-01", "john.doe@example.com"
            );

            // Act
            _sdk!.SendMessage(message);
        }

        [TestMethod]
        public async Task Test_StopAsync_WaitsUntilQueueIsEmpty()
        {
            // Arrange
            var queueResults = new Queue<bool>();
            queueResults.Enqueue(false); // Has pending messages
            queueResults.Enqueue(false); // Has pending messages
            queueResults.Enqueue(true);  // No pending messages

            _mockMessageQueue!.Setup(mq => mq.IsQueueEmpty())
                .Returns(() => queueResults.Count > 0 ? queueResults.Dequeue() : true);
            _mockMessageQueue.Setup(mq => mq.StopProcessingAsync()).Returns(Task.CompletedTask);
            _mockConnectionStrategy!.Setup(cs => cs.StopAsync()).Returns(Task.CompletedTask);

            // Act
            await _sdk!.StopAsync(waitForEmptyQueue: true, TimeSpan.FromSeconds(1));

            // Assert
            _mockMessageQueue.Verify(mq => mq.StopProcessingAsync(), Times.Once());
            _mockConnectionStrategy.Verify(cs => cs.StopAsync(), Times.Once());
        }

        [TestMethod]
        public async Task Test_StopAsync_WaitsUntilQueueIsEmptyTimeout()
        {
            // Arrange
            _mockMessageQueue!.Setup(mq => mq.IsQueueEmpty()).Returns(false);
            _mockMessageQueue.Setup(mq => mq.StopProcessingAsync()).Returns(Task.CompletedTask);
            _mockConnectionStrategy!.Setup(cs => cs.StopAsync()).Returns(Task.CompletedTask);

            // Act
            await _sdk!.StopAsync(waitForEmptyQueue: true, TimeSpan.FromSeconds(1));

            // Assert
            _mockMessageQueue.Verify(mq => mq.StopProcessingAsync(), Times.Once());
            _mockConnectionStrategy.Verify(cs => cs.StopAsync(), Times.Once());
            _mockLogger!.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Timeout exceeded while waiting for the message queue to empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        [TestMethod]
        public async Task Test_StopAsync_DoesNotWaitUntilQueueIsEmpty()
        {
            // Arrange
            _mockMessageQueue!.Setup(mq => mq.IsQueueEmpty()).Returns(false);
            _mockMessageQueue.Setup(mq => mq.StopProcessingAsync()).Returns(Task.CompletedTask);
            _mockConnectionStrategy!.Setup(cs => cs.StopAsync()).Returns(Task.CompletedTask);

            // Act
            await _sdk!.StopAsync(waitForEmptyQueue: false);

            // Assert
            _mockMessageQueue.Verify(mq => mq.StopProcessingAsync(), Times.Once());
            _mockMessageQueue.Verify(mq => mq.IsQueueEmpty(), Times.Never());
            _mockConnectionStrategy.Verify(cs => cs.StopAsync(), Times.Once());
        }

        [TestMethod]
        public async Task Test_StopAsync_WhenAlreadyStopped_DoesNotThrow()
        {
            // Arrange
            _mockMessageQueue!.Setup(mq => mq.StopProcessingAsync()).Returns(Task.CompletedTask);
            _mockConnectionStrategy!.Setup(cs => cs.StopAsync()).Returns(Task.CompletedTask);

            // Act
            await _sdk!.StopAsync(); // First call to stop
            await _sdk.StopAsync(); // Second call to stop

            // Assert
            _mockMessageQueue.Verify(mq => mq.StopProcessingAsync(), Times.Once());
            _mockConnectionStrategy.Verify(cs => cs.StopAsync(), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Test_StopAsync_WhenDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            _sdk!.Dispose();

            // Act
            _sdk.StopAsync().GetAwaiter().GetResult();
        }
    }
}