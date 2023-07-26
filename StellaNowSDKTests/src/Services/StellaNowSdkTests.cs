using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellaNowSDK.Config;
using StellaNowSDK.ConnectionStrategies;
using StellaNowSDK.Services;

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
    }
}