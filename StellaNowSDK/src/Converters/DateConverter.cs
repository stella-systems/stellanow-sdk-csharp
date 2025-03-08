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
/// A JSON converter for <see cref="DateOnly"/>, using the format "yyyy-MM-dd".
/// </summary>
public class DateConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Serializes a <see cref="DateOnly"/> as a string in "yyyy-MM-dd" format.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="DateOnly"/> value to serialize.</param>
    /// <param name="serializer">The calling serializer instance.</param>
    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Deserializes a string in "yyyy-MM-dd" format into a <see cref="DateOnly"/>.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> providing the JSON data.</param>
    /// <param name="objectType">The type of object to deserialize to (ignored).</param>
    /// <param name="existingValue">The existing <see cref="DateOnly"/> value, if any (ignored).</param>
    /// <param name="hasExistingValue">Indicates if there is an existing value (ignored).</param>
    /// <param name="serializer">The calling serializer instance.</param>
    /// <returns>A <see cref="DateOnly"/> parsed from the input string.</returns>
    /// <exception cref="FormatException">Thrown if the string does not match "yyyy-MM-dd".</exception>
    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var s = (string)reader.Value!;
        return DateOnly.ParseExact(s, DateFormat, CultureInfo.InvariantCulture);
    }
}