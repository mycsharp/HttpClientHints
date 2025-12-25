// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// A zero-allocation view over <see cref="IHeaderDictionary"/> to access HTTP Client Hints lazily.
/// </summary>
/// <remarks>
/// This struct does not allocate and reads values directly from the underlying request headers on demand.
/// Use <see cref="BuildSnapshot"/> to materialize a heap-allocated <see cref="HttpClientHints"/> record if needed.
/// </remarks>
/// <remarks>
/// Creates a new view over the given headers.
/// </remarks>
public readonly struct HttpClientHintsView(IHeaderDictionary headers)
{
    /// <summary>Gets the raw headers.</summary>
    public IHeaderDictionary Headers => headers;

    /// <summary>User-Agent</summary>
    public string? UserAgent
        => headers.TryGetValue("User-Agent", out StringValues v) && v.Count > 0 ? v[0] : null;

    /// <summary>Sec-CH-UA</summary>
    public string? UA
        => headers.TryGetValue("Sec-CH-UA", out StringValues v) && v.Count > 0 ? v[0] : null;

    /// <summary>Sec-CH-UA-Platform</summary>
    public string? Platform
        => headers.TryGetValue("Sec-CH-UA-Platform", out StringValues v) && v.Count > 0 ? v[0] : null;

    /// <summary>Sec-CH-UA-Platform-Version</summary>
    public string? PlatformVersion
        => headers.TryGetValue("Sec-CH-UA-Platform-Version", out StringValues v) && v.Count > 0 ? v[0] : null;

    /// <summary>Sec-CH-UA-Arch</summary>
    public string? Architecture
        => headers.TryGetValue("Sec-CH-UA-Arch", out StringValues v) && v.Count > 0 ? v[0] : null;

    /// <summary>Sec-CH-UA-Model</summary>
    public string? Model
        => headers.TryGetValue("Sec-CH-UA-Model", out StringValues v) && v.Count > 0 ? v[0] : null;

    /// <summary>Sec-CH-UA-Full-Version-List</summary>
    public string? FullVersionList
        => headers.TryGetValue("Sec-CH-UA-Full-Version-List", out StringValues v) && v.Count > 0 ? v[0] : null;

    /// <summary>Sec-CH-UA-Mobile mapped to nullable bool.</summary>
    public bool? Mobile
        => headers.TryGetValue("Sec-CH-UA-Mobile", out StringValues v) && v.Count > 0
            ? HttpClientHintsInterpreter.IsMobile(v[0])
            : null;

    /// <summary>
    /// Materializes a snapshot record of the current values. This allocates one object.
    /// </summary>
    public HttpClientHints BuildSnapshot()
        => new(UserAgent, Platform, PlatformVersion, Architecture, Model, FullVersionList, UA, Mobile, headers);
}
