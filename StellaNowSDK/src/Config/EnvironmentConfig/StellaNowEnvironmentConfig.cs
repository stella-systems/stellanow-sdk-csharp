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

namespace StellaNowSDK.Config.EnvironmentConfig;

/// <summary>
/// Base class for environment-specific configuration in StellaNow.
/// </summary>
/// <remarks>
/// Implementations of this abstract class specify environment-specific values
/// for the API base URL and ingestor broker URL. The <see cref="Authority"/> property
/// is derived from <see cref="ApiBaseUrl"/>.
/// </remarks>
public abstract class StellaNowEnvironmentConfig
{
    /// <summary>
    /// The base URL for StellaNow API endpoints.
    /// </summary>
    protected abstract string ApiBaseUrl { get; }

    /// <summary>
    /// The authority endpoint for authentication (e.g., Keycloak realm or OIDC issuer).
    /// </summary>
    /// <remarks>
    /// Built by appending "auth" to the <see cref="ApiBaseUrl"/>. 
    /// </remarks>
    public string Authority => BuildApiUrl("auth");
    
    /// <summary>
    /// The URL (including protocol and port) of the ingestor broker.
    /// </summary>
    public abstract string BrokerUrl { get; }

    /// <summary>
    /// Constructs a full API URL by appending the specified path to <see cref="ApiBaseUrl"/>.
    /// </summary>
    /// <param name="path">The endpoint or path to append.</param>
    /// <returns>A fully qualified URL string.</returns>
    private string BuildApiUrl(string path)
    {
        return $"{ApiBaseUrl}/{path}";
    }
}