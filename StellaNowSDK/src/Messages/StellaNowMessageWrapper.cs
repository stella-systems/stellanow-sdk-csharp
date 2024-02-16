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

namespace StellaNowSDK.Messages;

public abstract class StellaNowMessageWrapper
{
    public Metadata Metadata { get; }
    
    public List<Field> Fields { get; }
    
    protected StellaNowMessageWrapper(string eventTypeDefinitionId, List<EntityType> entityTypeIds)
    {
        Metadata = new Metadata
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageOriginDateUTC = DateTime.UtcNow,
            EventTypeDefinitionId = eventTypeDefinitionId,
            EntityTypeIds = entityTypeIds
        };
        
        Fields = new List<Field>();
    }
    
    protected void AddField(string name, string value)
    {
        Fields.Add(new Field(name, value));
    }
}