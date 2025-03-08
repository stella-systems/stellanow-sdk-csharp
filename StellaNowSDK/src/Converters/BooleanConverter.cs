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

namespace StellaNowSDK.Converters;

/// <summary>
/// A JSON converter for booleans, serializing them as the strings "true" or "false",
/// and parsing those strings back into boolean values.
/// </summary>
public class BooleanConverter : JsonConverter<bool>
{
    /// <summary>
    /// Serializes a boolean value as either "true" or "false".
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The boolean value to serialize.</param>
    /// <param name="serializer">The calling serializer instance.</param>
    public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
    {
        writer.WriteValue(value ? "true" : "false");
    }

    /// <summary>
    /// Deserializes a string ("true"/"false") back into a boolean.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> providing the JSON data.</param>
    /// <param name="objectType">The type of object to deserialize to (ignored).</param>
    /// <param name="existingValue">The existing boolean value, if any (ignored).</param>
    /// <param name="hasExistingValue">Indicates if there is an existing value (ignored).</param>
    /// <param name="serializer">The calling serializer instance.</param>
    /// <returns><c>true</c> if the string is "true", <c>false</c> if "false".</returns>
    /// <exception cref="FormatException">Thrown if the string cannot be parsed as a boolean.</exception>
    public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var strValue = (string)reader.Value!;
        return bool.Parse(strValue);
    }
}