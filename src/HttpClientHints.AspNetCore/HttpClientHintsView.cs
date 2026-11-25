// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// A view over <see cref="IHeaderDictionary"/> to access HTTP Client Hints lazily.
/// </summary>
/// <remarks>
/// Creating the view and reading a single raw field line do not allocate. The <c>Decoded*</c> properties decode the
/// value on every access and can allocate strings and lists.
/// Use <see cref="BuildSnapshot"/> to materialize a heap-allocated <see cref="HttpClientHints"/> record if needed.
/// Repeated Structured Field lines are combined with commas, preserving empty lines. Combining multiple lines allocates a string.
/// </remarks>
/// <param name="headers">The request headers.</param>
public readonly struct HttpClientHintsView(IHeaderDictionary headers)
{
    /// <summary>Gets the raw headers.</summary>
    public IHeaderDictionary Headers => headers;

    /// <summary>User-Agent</summary>
    public string? UserAgent
        => GetFirstValue("User-Agent");

    /// <summary>Sec-CH-UA as sent. <see cref="DecodedUA"/> returns the brands.</summary>
    public string? UA
        => GetStructuredValue("Sec-CH-UA");

    /// <summary>Gets <see cref="UA"/> decoded into its brands.</summary>
    /// <value>The brands, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeBrandList"/>.</value>
    public IReadOnlyList<HttpClientHintsBrand>? DecodedUA
        => HttpClientHintsInterpreter.DecodeBrandList(UA);

    /// <summary>Sec-CH-UA-Platform as sent, including quotes. <see cref="DecodedPlatform"/> returns it without quotes.</summary>
    public string? Platform
        => GetStructuredValue("Sec-CH-UA-Platform");

    /// <summary>Gets <see cref="Platform"/> decoded, for example <c>Windows</c> instead of <c>"Windows"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatform
        => HttpClientHintsInterpreter.DecodeStructuredString(Platform);

    /// <summary>Sec-CH-UA-Platform-Version as sent, including quotes. <see cref="DecodedPlatformVersion"/> returns it without quotes.</summary>
    public string? PlatformVersion
        => GetStructuredValue("Sec-CH-UA-Platform-Version");

    /// <summary>Gets <see cref="PlatformVersion"/> decoded, for example <c>19.0.0</c> instead of <c>"19.0.0"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatformVersion
        => HttpClientHintsInterpreter.DecodeStructuredString(PlatformVersion);

    /// <summary>Sec-CH-UA-Arch as sent, including quotes. <see cref="DecodedArchitecture"/> returns it without quotes.</summary>
    public string? Architecture
        => GetStructuredValue("Sec-CH-UA-Arch");

    /// <summary>Gets <see cref="Architecture"/> decoded, for example <c>x86</c> instead of <c>"x86"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedArchitecture
        => HttpClientHintsInterpreter.DecodeStructuredString(Architecture);

    /// <summary>Sec-CH-UA-Bitness as sent, including quotes. <see cref="DecodedBitness"/> returns it without quotes.</summary>
    public string? Bitness
        => GetStructuredValue("Sec-CH-UA-Bitness");

    /// <summary>Gets <see cref="Bitness"/> decoded, for example <c>64</c> instead of <c>"64"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedBitness
        => HttpClientHintsInterpreter.DecodeStructuredString(Bitness);

    /// <summary>Sec-CH-UA-WoW64 decoded with <see cref="HttpClientHintsInterpreter.DecodeStructuredBoolean"/>.</summary>
    /// <value>
    /// <see langword="true"/> if a 32-bit browser runs on 64-bit Windows, <see langword="false"/> if not, or
    /// <see langword="null"/> if the header is missing or malformed.
    /// </value>
    public bool? Wow64
        => HttpClientHintsInterpreter.DecodeStructuredBoolean(GetStructuredValue("Sec-CH-UA-WoW64"));

    /// <summary>Sec-CH-UA-Model as sent, including quotes. <see cref="DecodedModel"/> returns it without quotes.</summary>
    public string? Model
        => GetStructuredValue("Sec-CH-UA-Model");

    /// <summary>Gets <see cref="Model"/> decoded, for example <c>Pixel 9</c> instead of <c>"Pixel 9"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedModel
        => HttpClientHintsInterpreter.DecodeStructuredString(Model);

    /// <summary>Sec-CH-UA-Form-Factors as sent. <see cref="DecodedFormFactors"/> returns the form factors.</summary>
    public string? FormFactors
        => GetStructuredValue("Sec-CH-UA-Form-Factors");

    /// <summary>Gets <see cref="FormFactors"/> decoded, for example <c>Desktop</c> instead of <c>"Desktop"</c>.</summary>
    /// <value>The form factors, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredStringList"/>.</value>
    public IReadOnlyList<string>? DecodedFormFactors
        => HttpClientHintsInterpreter.DecodeStructuredStringList(FormFactors);

    /// <summary>Sec-CH-UA-Full-Version-List as sent. <see cref="DecodedFullVersionList"/> returns the brands.</summary>
    public string? FullVersionList
        => GetStructuredValue("Sec-CH-UA-Full-Version-List");

    /// <summary>Gets <see cref="FullVersionList"/> decoded into its brands with their full versions.</summary>
    /// <value>The brands, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeBrandList"/>.</value>
    public IReadOnlyList<HttpClientHintsBrand>? DecodedFullVersionList
        => HttpClientHintsInterpreter.DecodeBrandList(FullVersionList);

    /// <summary>Sec-CH-UA-Mobile decoded with <see cref="HttpClientHintsInterpreter.IsMobile"/>.</summary>
    public bool? Mobile
        => HttpClientHintsInterpreter.IsMobile(GetStructuredValue("Sec-CH-UA-Mobile"));

    /// <summary>
    /// Allocates a snapshot record of the current values and combines any repeated Structured Field lines.
    /// </summary>
    public HttpClientHints BuildSnapshot()
        => new(UserAgent, Platform, PlatformVersion, Architecture, Model, FullVersionList, UA, Mobile, headers)
        {
            Bitness = Bitness,
            Wow64 = Wow64,
            FormFactors = FormFactors,
        };

    private string? GetFirstValue(string key)
        => headers.TryGetValue(key, out StringValues values) && values.Count > 0 ? values[0] : null;

    // StringValues.ToString skips empty lines, which would change the syntax of a Structured Field.
    private string? GetStructuredValue(string key)
    {
        StringValues values = headers[key];
        return values.Count switch
        {
            0 => null,
            1 => values[0],
            _ => string.Join(',', values.ToArray()),
        };
    }
}
