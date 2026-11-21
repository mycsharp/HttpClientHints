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
        => GetFirstValue("User-Agent");

    /// <summary>Sec-CH-UA</summary>
    public string? UA
        => GetFirstValue("Sec-CH-UA");

    /// <summary>Sec-CH-UA-Platform</summary>
    public string? Platform
        => GetFirstValue("Sec-CH-UA-Platform");

    /// <summary>Sec-CH-UA-Platform-Version</summary>
    public string? PlatformVersion
        => GetFirstValue("Sec-CH-UA-Platform-Version");

    /// <summary>Sec-CH-UA-Arch</summary>
    public string? Architecture
        => GetFirstValue("Sec-CH-UA-Arch");

    /// <summary>Sec-CH-UA-Model</summary>
    public string? Model
        => GetFirstValue("Sec-CH-UA-Model");

    /// <summary>Sec-CH-UA-Full-Version-List</summary>
    public string? FullVersionList
        => GetFirstValue("Sec-CH-UA-Full-Version-List");

    /// <summary>Sec-CH-UA-Mobile mapped to nullable bool.</summary>
    public bool? Mobile
        => HttpClientHintsInterpreter.IsMobile(GetFirstValue("Sec-CH-UA-Mobile"));

    /// <summary>
    /// Materializes a snapshot record of the current values. This allocates one object.
    /// </summary>
    public HttpClientHints BuildSnapshot()
        => new(UserAgent, Platform, PlatformVersion, Architecture, Model, FullVersionList, UA, Mobile, headers);

    private string? GetFirstValue(string key)
        => headers.TryGetValue(key, out StringValues values) && values.Count > 0 ? values[0] : null;
}
