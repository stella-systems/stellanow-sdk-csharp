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
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StellaNowSDK.Messages;
using StellaNowSDK.Queue;
using StellaNowSdkTests.TestUtilities;

namespace StellaNowSdkTests.Messages;

[TestClass]
public class StellaNowMessageWrapperTests
{
        private Mock<IMessageQueueStrategy> _mockQueueStrategy; 

        [TestInitialize]
        public void SetUp()
        {
            _mockQueueStrategy = new Mock<IMessageQueueStrategy>();
        }

        [TestMethod]
        public void SendMessage_DirectlyAsJson_Success()
        {
            // Arrange
            string organizationId = "some-organization-id";
            string projectId = "some-project-id";
            var eventKey = new EventKey(organizationId, projectId);

            var messagePayload = new
            {
                firstName = "John",
                lastName = "Doe",
                dob = "1970-01-01",
                email = "john.doe@example.com"
            };

            var serializedPayload = JsonConvert.SerializeObject(messagePayload);
            var eventTypeDefinitionId = "user_update";
            var entityTypeIds = new List<EntityType> { new EntityType("punter", Guid.NewGuid().ToString()) };
            var messageWrapper = new StellaNowMessageWrapper(eventTypeDefinitionId, entityTypeIds, serializedPayload);

            // Act
            _mockQueueStrategy.Object.Enqueue(new StellaNowEventWrapper(eventKey, messageWrapper, null)); // Assuming null callback for simplicity

            // Assert
            _mockQueueStrategy.Verify(m => m.Enqueue(It.IsAny<StellaNowEventWrapper>()), Times.Once);
        }

        [TestMethod]
        public void SendMessage_UsingUserUpdateMessage_Success()
        {
            // Arrange
            string organizationId = "some-organization-id";
            string projectId = "some-project-id";
            var eventKey = new EventKey(organizationId, projectId);

            var userUpdateMessage = new UserUpdateMessage(
                Guid.NewGuid().ToString(), // PunterId
                "John", 
                "Doe", 
                "1970-01-01", 
                "john.doe@example.com");

            // Convert message to wrapper directly
            var messageWrapper = new StellaNowMessageWrapper(userUpdateMessage);

            // Act
            _mockQueueStrategy.Object.Enqueue(new StellaNowEventWrapper(eventKey, messageWrapper, null)); // Assuming null callback for simplicity

            // Assert
            _mockQueueStrategy.Verify(m => m.Enqueue(It.IsAny<StellaNowEventWrapper>()), Times.Once);
        }
}