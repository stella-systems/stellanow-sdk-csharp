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
using StellaNowSDK.Converters;

namespace StellaNowSDK.Messages;

/// <summary>
/// Static utility that provides consistent JSON serialization with all StellaNow converters applied.
/// Ensures DateTime, DateOnly, Boolean, and Decimal values are serialized with the correct format.
/// </summary>
public static class StellaNowJsonSerializable
{
    /// <summary>
    /// Gets the default JsonSerializerSettings with all StellaNow converters applied.
    /// </summary>
    public static JsonSerializerSettings GetStellaNowSerializerSettings()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new DateTimeConverter(),
                new DateConverter(),
                new BooleanConverter(),
                new DecimalConverter()
            }
        };
    }

    /// <summary>
    /// Serializes an object to a JSON string using StellaNow's standard formatting.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string representing the object with camel-case properties and consistent formatting.</returns>
    public static string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, GetStellaNowSerializerSettings());
    }

    /// <summary>
    /// Serializes an object to a JSON string with custom settings merged with StellaNow's converters.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="additionalSettings">Additional JSON serializer settings to apply.</param>
    /// <returns>A JSON string representing the object.</returns>
    public static string Serialize(object obj, JsonSerializerSettings? additionalSettings)
    {
        var settings = GetStellaNowSerializerSettings();
        
        if (additionalSettings != null)
        {
            // Merge additional settings while preserving StellaNow converters
            if (additionalSettings.ContractResolver != null)
                settings.ContractResolver = additionalSettings.ContractResolver;
                
            if (additionalSettings.Converters != null)
            {
                foreach (var converter in additionalSettings.Converters)
                {
                    settings.Converters.Add(converter);
                }
            }
            
            // Apply other settings as needed
            settings.NullValueHandling = additionalSettings.NullValueHandling;
            settings.DateTimeZoneHandling = additionalSettings.DateTimeZoneHandling;
            settings.Formatting = additionalSettings.Formatting;
        }
        
        return JsonConvert.SerializeObject(obj, settings);
    }
}