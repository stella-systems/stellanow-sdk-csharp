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

using Newtonsoft.Json;
using StellaNowSDK.Converters;

namespace StellaNowSDK.Messages;

public class StellaNowMessageWrapper
{
    public Metadata Metadata { get; }
    public string Payload { get; private set; }
    
    private static readonly JsonSerializerSettings SerializationSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter>
        {
            new DateTimeConverter(),
            new DateConverter(),
            new BooleanConverter(),
            new DecimalConverter()
        }
    };

    public StellaNowMessageWrapper(StellaNowMessageBase stellaNowMessage)
        : this(
            stellaNowMessage.EventTypeDefinitionId, 
            stellaNowMessage.EntityTypeIds, 
            JsonConvert.SerializeObject(stellaNowMessage, SerializationSettings)
            )
    {
    }
    
    public StellaNowMessageWrapper(string eventTypeDefinitionId, List<EntityType> entityTypeIds, string messageJson)
    {
        Metadata = new Metadata(
            Guid.NewGuid().ToString(),
            DateTime.UtcNow,
            eventTypeDefinitionId,
            entityTypeIds
        );

        Payload = messageJson;
    }
}