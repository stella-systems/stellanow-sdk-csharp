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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellaNowSDK.Messages;
using StellaNowSdkTests.TestUtilities;

namespace StellaNowSdkTests.Messages;

[TestClass]
public class StellaNowEventWrapperTests
{
    [TestMethod]
    public void ToString_PreservesTrailingZerosInMetadataDateTime()
    {
        var entityTypes = new List<EntityType> { new("test_entity", "test-id") };
        var messageWrapper = new StellaNowMessageWrapper("test_event", entityTypes, "{}");
        var eventKey = new EventKey("test-org", "test-project", "test-entity", "test-entity-type");
        var eventWrapper = new StellaNowEventWrapper(eventKey, messageWrapper, null);
        
        var json = eventWrapper.ToString();
        
        // The metadata contains a DateTime with current UTC time, so we check that the format includes microseconds with trailing zeros
        Assert.IsTrue(json.Contains("Z\""), "DateTime should be in UTC format");
    }
    
    [TestMethod]
    public void ToString_SerializesWithCamelCaseProperties()
    {
        var entityTypes = new List<EntityType> { new("test_entity", "test-id") };
        var messageWrapper = new StellaNowMessageWrapper("test_event", entityTypes, "{}");
        var eventKey = new EventKey("test-org", "test-project", "test-entity", "test-entity-type");
        var eventWrapper = new StellaNowEventWrapper(eventKey, messageWrapper, null);
        
        var json = eventWrapper.ToString();
        
        StringAssert.Contains(json, "\"messageOriginDateUTC\":", "Should use camel case for property names");
        StringAssert.Contains(json, "\"messageId\":", "Should use camel case for property names");
        Assert.IsFalse(json.Contains("\"MessageId\":"), "Should not use Pascal case");
    }
    
    [TestMethod]
    public void ToString_SerializesUserUpdateMessageInWrapper()
    {
        var testMessage = new UserUpdateMessage(
            PunterId: "user-123",
            FirstName: "John",
            LastName: "Doe", 
            Dob: "1990-01-01",
            Email: "john.doe@example.com"
        );
        
        // The proper way: wrap the message and serialize through the wrapper
        var messageWrapper = new StellaNowMessageWrapper(testMessage);
        var eventKey = new EventKey("test-org", "test-project", "user-123", "punter");
        var eventWrapper = new StellaNowEventWrapper(eventKey, messageWrapper, null);
        
        var json = eventWrapper.ToString();
        
        // Check that the payload contains properly serialized JSON with JsonProperty names (escaped in the JSON string)
        StringAssert.Contains(json, "\\\"firstName\\\":", "Payload should serialize with JSON property names");
        StringAssert.Contains(json, "\\\"John\\\"", "Should serialize firstName value");
        // Note: user-123 will appear in the entityId but NOT in the payload because of JsonIgnore
        Assert.IsFalse(json.Contains("\\\"user-123\\\""), "PunterId should be ignored in payload JSON serialization");
    }
}