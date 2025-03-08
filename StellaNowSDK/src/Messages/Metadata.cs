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

namespace StellaNowSDK.Messages;

/// <summary>
/// Holds metadata for a message, including a unique message ID, timestamp, and references
/// to the event type and associated entities.
/// </summary>
/// <param name="MessageId">A unique identifier (UUID/string) for the message.</param>
/// <param name="MessageOriginDateUTC">The timestamp (in UTC) when the message was created.</param>
/// <param name="EventTypeDefinitionId">The StellaNow event type definition ID (e.g., "user_login").</param>
/// <param name="EntityTypeIds">
/// A collection of entity references, allowing the message to be linked to one or more entities.
/// </param>
public record Metadata(
    string MessageId,
    DateTime MessageOriginDateUTC,
    string EventTypeDefinitionId,
    List<EntityType>? EntityTypeIds
    );