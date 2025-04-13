// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Provides configuration options for HTTP client hints.
/// </summary>
public class HttpClientHintsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to include the user agent in HTTP client hints.
    /// </summary>
    /// <value><c>true</c> to include the user agent; otherwise, <c>false</c>.</value>
    public bool UserAgent { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include platform information in HTTP client hints.
    /// </summary>
    /// <value><c>true</c> to include platform information; otherwise, <c>false</c>.</value>
    public bool Platform { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include architecture information in HTTP client hints.
    /// </summary>
    /// <value><c>true</c> to include architecture information; otherwise, <c>false</c>.</value>
    public bool Architecture { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include device information in HTTP client hints.
    /// </summary>
    /// <value><c>true</c> to include device information; otherwise, <c>false</c>.</value>
    public bool Device { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include mobile device indication in HTTP client hints.
    /// </summary>
    /// <value><c>true</c> to indicate a mobile device; otherwise, <c>false</c>.</value>
    public bool Mobile { get; set; } = true;

    /// <summary>
    /// Gets or sets additional client hint headers to include.
    /// </summary>
    /// <value>A string specifying additional headers, or <c>null</c> if none.</value>
    public string[]? Additional { get; set; }

    /// <summary>
    /// Gets or sets the lifetime of the HTTP client hints.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> indicating the hints' duration, or <c>null</c> if unspecified.</value>
    public TimeSpan? Lifetime { get; set; }
}
