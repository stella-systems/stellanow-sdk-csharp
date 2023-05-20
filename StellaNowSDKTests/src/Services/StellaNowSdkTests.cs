using StellaNowSDK.Services;
using StellaNowSDKTests.Messages;

namespace StellaNowSdkTests.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;
using StellaNowSDK.ConnectionStrategies;
using System.Threading.Tasks;

[TestClass]
public class StellaNowSdkTests
{
    [TestMethod]
    public async Task UserUpdateMessage_Serialize_Correctly()
    {
        // Arrange
        var connectionStrategy = new Mock<IStellaNowConnectionStrategy>();
        var messageQueueStrategy = new FifoMessageQueueStrategy();

        var sdk = new StellaNowSdk(connectionStrategy.Object, messageQueueStrategy, "myorg", "myproj");

        var userUpdateMessage = new UserUpdateMessage("123", "John", "Doe", "1980-01-01", "john.doe@example.com");

        // Enqueue the message
        sdk.Send(userUpdateMessage);

        // Manually dequeue and send the message
        if (messageQueueStrategy.TryDequeue(out var dequeuedMessage))
        {
            await connectionStrategy.Object.SendMessageAsync(dequeuedMessage);
        }

        // Verify SendMessageAsync was called with the correct message
        connectionStrategy.Verify(s => s.SendMessageAsync(It.IsAny<StellaNowEventWrapper>()), Times.Once);

        // Assert that the message was correctly serialized
        var message = JToken.Parse(dequeuedMessage.GetForDispatch());
        var actualMessageId = (string)message["value"]["metadata"]["messageId"];
        var actualFields = message["value"]["fields"].ToObject<List<Field>>();

        var expectedFields = new List<Field>
        {
            new Field("firstName", "John"),
            new Field("lastName", "Doe"),
            new Field("dob", "1980-01-01"),
            new Field("email", "john.doe@example.com")
        };

        Assert.AreEqual(userUpdateMessage.Metadata.MessageId, actualMessageId);
        CollectionAssert.AreEquivalent(expectedFields, actualFields);
    }
}