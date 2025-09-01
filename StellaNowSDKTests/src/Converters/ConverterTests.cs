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
using Newtonsoft.Json;
using StellaNowSDK.Converters;
using System.Globalization;

namespace StellaNowSdkTests.Converters;

[TestClass]
public class ConverterTests
{
    [TestClass]
    public class DateTimeConverterTests
    {
        private readonly DateTimeConverter _converter = new();
        private readonly JsonSerializer _serializer = new();

        [TestMethod]
        public void WriteJson_SerializesDateTimeCorrectly()
        {
            var dateTime = new DateTime(2023, 12, 25, 14, 30, 45, 123, DateTimeKind.Utc);
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, dateTime, _serializer);
            
            var result = stringWriter.ToString();
            Assert.AreEqual("\"2023-12-25T14:30:45.123000Z\"", result);
        }

        [TestMethod]
        public void WriteJson_PreservesTrailingZerosInMicroseconds()
        {
            var dateTime = new DateTime(2023, 12, 25, 14, 30, 45, 123, DateTimeKind.Utc);
            dateTime = dateTime.AddMicroseconds(456);
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, dateTime, _serializer);
            
            var result = stringWriter.ToString();
            Assert.AreEqual("\"2023-12-25T14:30:45.123456Z\"", result);
        }

        [TestMethod]
        public void WriteJson_HandlesTrailingZerosInFractionalSeconds()
        {
            var dateTime = new DateTime(2023, 12, 25, 14, 30, 45, 123, DateTimeKind.Utc);
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, dateTime, _serializer);
            
            var result = stringWriter.ToString();
            StringAssert.Contains(result, ".123000Z");
        }

        [TestMethod]
        public void ReadJson_DeserializesDateTimeCorrectly()
        {
            var jsonString = "\"2023-12-25T14:30:45.123456Z\"";
            
            using var stringReader = new StringReader(jsonString);
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var result = _converter.ReadJson(jsonReader, typeof(DateTime), default, false, _serializer);
            
            var expected = new DateTime(2023, 12, 25, 14, 30, 45, 123, DateTimeKind.Utc).AddMicroseconds(456);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ReadJson_HandlesTrailingZeros()
        {
            var jsonString = "\"2023-12-25T14:30:45.123000Z\"";
            
            using var stringReader = new StringReader(jsonString);
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var result = _converter.ReadJson(jsonReader, typeof(DateTime), default, false, _serializer);
            
            var expected = new DateTime(2023, 12, 25, 14, 30, 45, 123, DateTimeKind.Utc);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RoundTripSerialization_PreservesDateTimeValue()
        {
            var originalDateTime = new DateTime(2023, 12, 25, 14, 30, 45, 123, DateTimeKind.Utc);
            originalDateTime = originalDateTime.AddMicroseconds(456);
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            _converter.WriteJson(jsonWriter, originalDateTime, _serializer);
            
            var serialized = stringWriter.ToString();
            
            using var stringReader = new StringReader(serialized);
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var deserialized = _converter.ReadJson(jsonReader, typeof(DateTime), default, false, _serializer);
            
            Assert.AreEqual(originalDateTime, deserialized);
        }

        [TestMethod]
        public void WriteJson_ConvertsLocalTimeToUtc()
        {
            var localDateTime = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Local);
            var expectedUtc = localDateTime.ToUniversalTime();
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, localDateTime, _serializer);
            
            var result = stringWriter.ToString();
            var expectedString = "\"" + expectedUtc.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ", CultureInfo.InvariantCulture) + "\"";
            Assert.AreEqual(expectedString, result);
        }
    }

    [TestClass]
    public class DateConverterTests
    {
        private readonly DateConverter _converter = new();
        private readonly JsonSerializer _serializer = new();

        [TestMethod]
        public void WriteJson_SerializesDateOnlyCorrectly()
        {
            var date = new DateOnly(2023, 12, 25);
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, date, _serializer);
            
            var result = stringWriter.ToString();
            Assert.AreEqual("\"2023-12-25\"", result);
        }

        [TestMethod]
        public void ReadJson_DeserializesDateOnlyCorrectly()
        {
            var jsonString = "2023-12-25";
            
            using var stringReader = new StringReader($"\"{jsonString}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var result = _converter.ReadJson(jsonReader, typeof(DateOnly), default, false, _serializer);
            
            var expected = new DateOnly(2023, 12, 25);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RoundTripSerialization_PreservesDateOnlyValue()
        {
            var originalDate = new DateOnly(2023, 12, 25);
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            _converter.WriteJson(jsonWriter, originalDate, _serializer);
            
            var serialized = stringWriter.ToString();
            
            using var stringReader = new StringReader(serialized);
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var deserialized = _converter.ReadJson(jsonReader, typeof(DateOnly), default, false, _serializer);
            
            Assert.AreEqual(originalDate, deserialized);
        }
    }

    [TestClass]
    public class BooleanConverterTests
    {
        private readonly BooleanConverter _converter = new();
        private readonly JsonSerializer _serializer = new();

        [TestMethod]
        public void WriteJson_SerializesTrueCorrectly()
        {
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, true, _serializer);
            
            var result = stringWriter.ToString();
            Assert.AreEqual("\"true\"", result);
        }

        [TestMethod]
        public void WriteJson_SerializesFalseCorrectly()
        {
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, false, _serializer);
            
            var result = stringWriter.ToString();
            Assert.AreEqual("\"false\"", result);
        }

        [TestMethod]
        public void ReadJson_DeserializesTrueCorrectly()
        {
            using var stringReader = new StringReader("\"true\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var result = _converter.ReadJson(jsonReader, typeof(bool), default, false, _serializer);
            
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReadJson_DeserializesFalseCorrectly()
        {
            using var stringReader = new StringReader("\"false\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var result = _converter.ReadJson(jsonReader, typeof(bool), default, false, _serializer);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RoundTripSerialization_PreservesBooleanValues()
        {
            var testValues = new[] { true, false };
            
            foreach (var originalValue in testValues)
            {
                using var stringWriter = new StringWriter();
                using var jsonWriter = new JsonTextWriter(stringWriter);
                _converter.WriteJson(jsonWriter, originalValue, _serializer);
                
                var serialized = stringWriter.ToString();
                
                using var stringReader = new StringReader(serialized);
                using var jsonReader = new JsonTextReader(stringReader);
                jsonReader.Read();
                
                var deserialized = _converter.ReadJson(jsonReader, typeof(bool), default, false, _serializer);
                
                Assert.AreEqual(originalValue, deserialized);
            }
        }
    }

    [TestClass]
    public class DecimalConverterTests
    {
        private readonly DecimalConverter _converter = new();
        private readonly JsonSerializer _serializer = new();

        [TestMethod]
        public void WriteJson_SerializesDecimalWithEightDecimalPlaces()
        {
            var decimalValue = 123.45m;
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, decimalValue, _serializer);
            
            var result = stringWriter.ToString();
            Assert.AreEqual("\"123.45000000\"", result);
        }

        [TestMethod]
        public void WriteJson_HandlesLargeDecimalValues()
        {
            var decimalValue = 9876543210.12345678m;
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            
            _converter.WriteJson(jsonWriter, decimalValue, _serializer);
            
            var result = stringWriter.ToString();
            Assert.AreEqual("\"9876543210.12345678\"", result);
        }

        [TestMethod]
        public void ReadJson_DeserializesDecimalCorrectly()
        {
            var jsonString = "123.45000000";
            
            using var stringReader = new StringReader($"\"{jsonString}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var result = _converter.ReadJson(jsonReader, typeof(decimal), default, false, _serializer);
            
            Assert.AreEqual(123.45m, result);
        }

        [TestMethod]
        public void ReadJson_HandlesVariousPrecisions()
        {
            var testCases = new[]
            {
                ("123.45", 123.45m),
                ("123.450000", 123.45m),
                ("0.12345678", 0.12345678m),
                ("1000000.00", 1000000.00m)
            };
            
            foreach (var (jsonString, expected) in testCases)
            {
                using var stringReader = new StringReader($"\"{jsonString}\"");
                using var jsonReader = new JsonTextReader(stringReader);
                jsonReader.Read();
                
                var result = _converter.ReadJson(jsonReader, typeof(decimal), default, false, _serializer);
                
                Assert.AreEqual(expected, result, $"Failed for input: {jsonString}");
            }
        }

        [TestMethod]
        public void RoundTripSerialization_PreservesDecimalValue()
        {
            var originalDecimal = 123.45678901m;
            
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            _converter.WriteJson(jsonWriter, originalDecimal, _serializer);
            
            var serialized = stringWriter.ToString();
            
            using var stringReader = new StringReader(serialized);
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            var deserialized = _converter.ReadJson(jsonReader, typeof(decimal), default, false, _serializer);
            
            Assert.AreEqual(originalDecimal, deserialized);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonSerializationException))]
        public void ReadJson_ThrowsExceptionForInvalidDecimal()
        {
            var jsonString = "not-a-number";
            
            using var stringReader = new StringReader($"\"{jsonString}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();
            
            _converter.ReadJson(jsonReader, typeof(decimal), default, false, _serializer);
        }
    }
}