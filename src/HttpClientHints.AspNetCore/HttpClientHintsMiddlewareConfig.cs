// Copyright © https://myCSharp.de - all rights reserved

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
    public required string ResponseHeader
    {
        get;
        set
        {
            field = value;
            HasResponseHeaders = string.IsNullOrEmpty(value) is false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the response headers have been configured.
    /// </summary>
    /// <value><c>true</c> if response headers are configured; otherwise, <c>false</c>.</value>
    public bool HasResponseHeaders { get; internal set; }

    /// <summary>
    /// Gets or sets the lifetime of the client hints in seconds.
    /// </summary>
    /// <value>A <see cref="string"/> containing an <see cref="int"/> representing the lifetime in seconds, or <c>null</c> if unspecified.</value>
    /// <remarks>These settings are set by <see cref="HttpClientHintsRegistration"/>. Do not set these values manually.</remarks>
    public string? LifeTime
    {
        get;
        set
        {
            field = value;
            HasLifetime = string.IsNullOrEmpty(value) is false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the lifetime for client hints has been configured.
    /// </summary>
    /// <value><c>true</c> if lifetime is configured; otherwise, <c>false</c>.</value>
    public bool HasLifetime { get; internal set; }
}
