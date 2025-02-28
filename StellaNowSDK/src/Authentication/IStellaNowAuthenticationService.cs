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

namespace StellaNowSDK.Authentication;

/// <summary>
/// Defines the contract for StellaNow authentication services.
/// </summary>
/// <remarks>
/// An implementation is responsible for logging in, refreshing tokens, and
/// retrieving the current authentication state (e.g., access tokens).
/// </remarks>
public interface IStellaNowAuthenticationService
{
    /// <summary>
    /// Performs an authentication flow if necessary.
    /// </summary>
    /// <remarks>
    /// Implementations may first attempt to refresh existing tokens; if that fails,
    /// they will initiate a new login process.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AuthenticateAsync();
    
    /// <summary>
    /// Retrieves the latest authentication data (e.g., an access token).
    /// </summary>
    /// <returns>A <see cref="StellaNowAuthenticationResult"/> containing token details.</returns>
    StellaNowAuthenticationResult GetAuthenticationData();
}