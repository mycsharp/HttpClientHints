// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Configures which Client Hints the <c>Accept-CH</c> response header requests.
/// </summary>
/// <remarks>
/// Browsers send the low-entropy hints <c>Sec-CH-UA</c>, <c>Sec-CH-UA-Mobile</c> and <c>Sec-CH-UA-Platform</c> with every
/// request. All other hints have high entropy: browsers send them only after a response has requested them.
/// The options for high-entropy hints are <see langword="false"/> by default.
/// </remarks>
public class HttpClientHintsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to request the brand list <c>Sec-CH-UA</c> (low entropy).
    /// </summary>
    /// <value><see langword="true"/> by default.</value>
    public bool UserAgent { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to request <c>Sec-CH-UA-Mobile</c> (low entropy).
    /// </summary>
    /// <value><see langword="true"/> by default.</value>
    public bool Mobile { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to request <c>Sec-CH-UA-Platform</c> and the high-entropy hint
    /// <c>Sec-CH-UA-Platform-Version</c>.
    /// </summary>
    /// <value><see langword="false"/> by default.</value>
    public bool Platform { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to request the high-entropy hints <c>Sec-CH-UA-Arch</c>,
    /// <c>Sec-CH-UA-Bitness</c> and <c>Sec-CH-UA-WoW64</c>.
    /// </summary>
    /// <value><see langword="false"/> by default.</value>
    public bool Architecture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to request the high-entropy hints <c>Sec-CH-UA-Model</c> and
    /// <c>Sec-CH-UA-Form-Factors</c>.
    /// </summary>
    /// <value><see langword="false"/> by default.</value>
    public bool Device { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to request the high-entropy hint <c>Sec-CH-UA-Full-Version-List</c>.
    /// </summary>
    /// <value><see langword="false"/> by default.</value>
    public bool FullVersionList { get; set; }

    /// <summary>
    /// Gets or sets further hints to request, for example <c>Sec-CH-Prefers-Color-Scheme</c>.
    /// </summary>
    /// <value>
    /// The header names, or <see langword="null"/>. Surrounding white space is removed; empty entries and duplicates,
    /// ignoring case, are ignored. An entry that is not a valid header name makes
    /// <see cref="HttpClientHintsRegistration.AddHttpClientHints"/> throw an <see cref="ArgumentException"/>.
    /// </value>
    public string[]? Additional { get; set; }

    /// <summary>
    /// Gets or sets the hints that the response depends on already for the first request.
    /// </summary>
    /// <value>The header names, or <see langword="null"/>, with the same rules as <see cref="Additional"/>.</value>
    /// <remarks>
    /// The hints are sent in the <c>Critical-CH</c> response header and added to <c>Accept-CH</c> and <c>Vary</c>.
    /// A browser that supports <c>Critical-CH</c> and did not send one of these hints repeats the request once with the hint.
    /// </remarks>
    public string[]? Critical { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to add all requested hints to the <c>Vary</c> response header.
    /// </summary>
    /// <value><see langword="false"/> by default.</value>
    /// <remarks>
    /// Enable it if responses that depend on the hints can be cached. The middleware appends the hints to a
    /// <c>Vary</c> header set before it, once per response. Hints in <see cref="Critical"/> are always added to <c>Vary</c>.
    /// </remarks>
    public bool Vary { get; set; }

    /// <summary>
    /// Gets or sets the value of the <c>Accept-CH-Lifetime</c> response header.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/>, sent in whole seconds, or <see langword="null"/> to omit the header.</value>
    [Obsolete("Accept-CH-Lifetime was removed from the Client Hints specification, and browsers ignore it. Browsers keep the hints requested with Accept-CH without this header.")]
    public TimeSpan? Lifetime { get; set; }
}
