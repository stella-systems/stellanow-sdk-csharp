// Copyright (C) 2022-2024 Stella Technologies (UK) Limited.
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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StellaNowSDK.Messages;
using StellaNowSdkTests.TestUtilities;

namespace StellaNowSdkTests.Messages;

[TestClass]
public class StellaNowMessageWrapperTests
{
    [TestMethod]
    public void MessageSerializationTest()
    {
        var userUpdateMessage = new UserUpdateMessage(
            Guid.NewGuid().ToString(),"John", "Doe", "1970-01-01", "john.doe@example.com"
        );
    
        var serializedMessage = JsonConvert.SerializeObject(
            userUpdateMessage,
            new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        );
    
        // var expectedFields = new List<Field>
        // {
        //     new Field("firstName", "John" ),
        //     new Field("lastName", "Doe"),
        //     new Field("dob", "1970-01-01"),
        //     new Field("email", "john.doe@example.com")
        // };

        var expectedMetadata = new 
        {
            MessageId = userUpdateMessage.Metadata.MessageId,
            MessageOriginDateUtc = userUpdateMessage.Metadata.MessageOriginDateUTC,
            EventTypeDefinitionId = "user_update",
            EntityTypeIds = userUpdateMessage.Metadata.EntityTypeIds
        };

        var expectedMessage = JsonConvert.SerializeObject(new
            {
                metadata = expectedMetadata,
                // fields = expectedFields
            },
            new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        );

        Assert.AreEqual(expectedMessage, serializedMessage);
    }
}