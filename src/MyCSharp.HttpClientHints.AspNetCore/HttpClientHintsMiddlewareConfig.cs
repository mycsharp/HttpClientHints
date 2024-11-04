// Copyright © myCSharp.de - all rights reserved

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Configuration options for the HTTP client hints middleware.
/// </summary>
public class HttpClientHintsMiddlewareConfig
{
    /// <summary>
    /// Gets or sets the response header used to convey HTTP client hints.
    /// </summary>
    /// <value>The name of the response header as a <see cref="string"/>.</value>
    /// <remarks>These settings are set by <see cref="HttpClientHintsRegistration"/>. Do not set these values manually.</remarks>
    public required string ResponseHeader { get; set; }

    /// <summary>
    /// Gets or sets the lifetime of the client hints in seconds.
    /// </summary>
    /// <value>A <see cref="string"/> containing an <see cref="int"/> representing the lifetime in seconds, or <c>null</c> if unspecified.</value>
    /// <remarks>These settings are set by <see cref="HttpClientHintsRegistration"/>. Do not set these values manually.</remarks>
    public string? LifeTime { get; set; }
}
