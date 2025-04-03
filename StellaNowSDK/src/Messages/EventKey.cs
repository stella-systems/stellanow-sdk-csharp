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
/// Identifies an event in StellaNow by organization, project, and entity.
/// </summary>
/// <remarks>
/// This record is used to group events logically under a specific organization,
/// project, and entity in the StellaNow platform.
/// </remarks>
/// <param name="OrganizationId">Unique identifier (UUID/string) of the organization.</param>
/// <param name="ProjectId">Unique identifier (UUID/string) of the project within the organization.</param>
/// <param name="EntityId">
/// The specific entity identifier to which this event is associated (e.g., a device or user).
/// </param>
/// <param name="EntityTypeDefinitionId">
/// The specific entity type definition representing the entity.
/// </param>

public record EventKey(string OrganizationId, string ProjectId, string EntityId, string EntityTypeDefinitionId);