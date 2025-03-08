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
using Newtonsoft.Json.Serialization;
using StellaNowSDK.Types;

namespace StellaNowSDK.Messages;

/// <summary>
/// Wraps a StellaNow event, combining a key (organization/project/entity) with a
/// message and an optional callback to be invoked upon successful dispatch.
/// </summary>
/// <remarks>
/// The <see cref="ToString"/> method serializes the event to JSON (with camel-case property names),
/// making it suitable for sending.
/// </remarks>
public sealed class StellaNowEventWrapper
{
    /// <summary>
    /// Gets or sets the <see cref="EventKey"/> used to associate this event with
    /// an organization, project, and entity.
    /// </summary>
    public EventKey Key { get; set; }
    
    /// <summary>
    /// Gets or sets the wrapped <see cref="StellaNowMessageWrapper"/>, which contains
    /// message metadata and the JSON payload.
    /// </summary>
    public StellaNowMessageWrapper Value { get; set; }

    /// <summary>
    /// An optional callback that is invoked after the message has been successfully
    /// published to the broker.
    /// </summary>
    /// <remarks>
    /// This field is read-only and ignored by JSON serialization.
    /// </remarks>
    [JsonIgnore] public readonly OnMessageSent? Callback;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StellaNowEventWrapper"/> class.
    /// </summary>
    /// <param name="key">The <see cref="EventKey"/> identifying the organization/project/entity.</param>
    /// <param name="value">The <see cref="StellaNowMessageWrapper"/> containing the message payload.</param>
    /// <param name="callback">
    /// An optional callback to invoke once the message has been dispatched.
    /// </param>
    public StellaNowEventWrapper(EventKey key, StellaNowMessageWrapper value, OnMessageSent? callback)
    {
        Key = key;
        Value = value;
        Callback = callback;
    }

    /// <summary>
    /// Serializes this instance to a JSON string using camel-case property names.
    /// </summary>
    /// <returns>A JSON string representing this event.</returns>
    public override string ToString()
    {
        var settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        return JsonConvert.SerializeObject(this, settings);
    }
}