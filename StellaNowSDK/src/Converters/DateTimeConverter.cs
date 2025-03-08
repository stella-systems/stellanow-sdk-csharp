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
/// A JSON converter for <see cref="DateTime"/>, using the format "yyyy-MM-ddTHH:mm:ss.ffffffZ" (UTC).
/// </summary>
public class DateTimeConverter : JsonConverter<DateTime>
{
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

    /// <summary>
    /// Serializes a <see cref="DateTime"/> as a string in UTC using the format "yyyy-MM-ddTHH:mm:ss.ffffffZ".
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="DateTime"/> value to serialize.</param>
    /// <param name="serializer">The calling serializer instance.</param>
    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToUniversalTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Deserializes a string in "yyyy-MM-ddTHH:mm:ss.ffffffZ" format (UTC) into a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> providing the JSON data.</param>
    /// <param name="objectType">The type of object to deserialize to (ignored).</param>
    /// <param name="existingValue">The existing <see cref="DateTime"/> value, if any (ignored).</param>
    /// <param name="hasExistingValue">Indicates if there is an existing value (ignored).</param>
    /// <param name="serializer">The calling serializer instance.</param>
    /// <returns>A <see cref="DateTime"/> in UTC.</returns>
    /// <exception cref="FormatException">Thrown if the string does not match the specified format.</exception>
    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var s = (string)reader.Value!;
        return DateTime.ParseExact(s, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }
}