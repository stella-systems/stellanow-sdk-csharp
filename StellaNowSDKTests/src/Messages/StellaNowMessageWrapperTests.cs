using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StellaNowSDK.Messages;
using StellaNowSDKTests.Messages;

[TestClass]
public class StellaNowMessageWrapperTests
{
    [TestMethod]
    public void MessageSerializationTest()
    {
        var userUpdateMessage = new UserUpdateMessage(
            Guid.NewGuid().ToString(),"John", "Doe", "1970-01-01", "john.doe@example.com"
        );
    
        var serializedMessage = userUpdateMessage.ToString();
    
        var expectedFields = new List<Field>
        {
            new Field("firstName", "John" ),
            new Field("lastName", "Doe"),
            new Field("dob", "1970-01-01"),
            new Field("email", "john.doe@example.com")
        };

        var expectedMetadata = new 
        {
            Source = "external",
            MessageId = userUpdateMessage.Metadata.MessageId,
            MessageProcessingDateUtc = userUpdateMessage.Metadata.MessageProcessingDateUtc,
            MessageOriginDateUtc = userUpdateMessage.Metadata.MessageOriginDateUtc,
            EventTypeDefinitionId = "user_update",
            EntityTypeIds = userUpdateMessage.Metadata.EntityTypeIds
        };

        var expectedMessage = JsonConvert.SerializeObject(new
            {
                metadata = expectedMetadata,
                fields = expectedFields
            },
            new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        );

        Assert.AreEqual(expectedMessage, serializedMessage);
    }
}