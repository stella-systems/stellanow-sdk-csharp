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
using System.Globalization;

namespace StellaNowSDK.Converters;

/// <summary>
/// A JSON converter for <see cref="decimal"/>, formatting values with 2 decimal places ("F2").
/// The converter will also parse numeric strings back into decimal values.
/// </summary>
public class DecimalConverter : JsonConverter<decimal>
{
    /// <summary>
    /// Serializes a <see cref="decimal"/> as a string with 2 decimal places.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="decimal"/> value to serialize.</param>
    /// <param name="serializer">The calling serializer instance.</param>
    public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
    {
        // Format the decimal value as a string with 2 decimal places
        writer.WriteValue(value.ToString("F8", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Deserializes a numeric string into a <see cref="decimal"/>.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> providing the JSON data.</param>
    /// <param name="objectType">The type of object to deserialize to (ignored).</param>
    /// <param name="existingValue">The existing <see cref="decimal"/> value, if any (ignored).</param>
    /// <param name="hasExistingValue">Indicates if there is an existing value (ignored).</param>
    /// <param name="serializer">The calling serializer instance.</param>
    /// <returns>A parsed <see cref="decimal"/> value.</returns>
    /// <exception cref="JsonSerializationException">
    /// Thrown if the string cannot be parsed as a decimal.
    /// </exception>
    public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Attempt to parse the decimal value from the reader
        var strValue = (string)reader.Value!;
        if (decimal.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        throw new JsonSerializationException($"Error parsing '{strValue}' as decimal.");
    }
}