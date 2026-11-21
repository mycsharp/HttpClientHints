// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// A view over <see cref="IHeaderDictionary"/> to access HTTP Client Hints lazily.
/// </summary>
/// <remarks>
/// Creating the view and reading header values do not allocate. The <c>Decoded*</c> properties decode the value on
/// every access and allocate the decoded string for a quoted value.
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

    /// <summary>Sec-CH-UA-Platform as sent, including quotes. <see cref="DecodedPlatform"/> returns it without quotes.</summary>
    public string? Platform
        => GetFirstValue("Sec-CH-UA-Platform");

    /// <summary>Gets <see cref="Platform"/> decoded, for example <c>Windows</c> instead of <c>"Windows"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatform
        => HttpClientHintsInterpreter.DecodeStructuredString(Platform);

    /// <summary>Sec-CH-UA-Platform-Version as sent, including quotes. <see cref="DecodedPlatformVersion"/> returns it without quotes.</summary>
    public string? PlatformVersion
        => GetFirstValue("Sec-CH-UA-Platform-Version");

    /// <summary>Gets <see cref="PlatformVersion"/> decoded, for example <c>19.0.0</c> instead of <c>"19.0.0"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatformVersion
        => HttpClientHintsInterpreter.DecodeStructuredString(PlatformVersion);

    /// <summary>Sec-CH-UA-Arch as sent, including quotes. <see cref="DecodedArchitecture"/> returns it without quotes.</summary>
    public string? Architecture
        => GetFirstValue("Sec-CH-UA-Arch");

    /// <summary>Gets <see cref="Architecture"/> decoded, for example <c>x86</c> instead of <c>"x86"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedArchitecture
        => HttpClientHintsInterpreter.DecodeStructuredString(Architecture);

    /// <summary>Sec-CH-UA-Model as sent, including quotes. <see cref="DecodedModel"/> returns it without quotes.</summary>
    public string? Model
        => GetFirstValue("Sec-CH-UA-Model");

    /// <summary>Gets <see cref="Model"/> decoded, for example <c>Pixel 9</c> instead of <c>"Pixel 9"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedModel
        => HttpClientHintsInterpreter.DecodeStructuredString(Model);

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
