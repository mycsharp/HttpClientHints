// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Represents HTTP Client Hints retrieved from the request headers.
/// </summary>
/// <param name="UserAgent">The User-Agent request header.</param>
/// <param name="Platform">The Sec-CH-UA-Platform header value as sent, including quotes. <see cref="DecodedPlatform"/> returns it without quotes.</param>
/// <param name="PlatformVersion">The Sec-CH-UA-Platform-Version header value as sent, including quotes. <see cref="DecodedPlatformVersion"/> returns it without quotes.</param>
/// <param name="Architecture">The Sec-CH-UA-Arch header value as sent, including quotes. <see cref="DecodedArchitecture"/> returns it without quotes.</param>
/// <param name="Model">The Sec-CH-UA-Model header value as sent, including quotes. <see cref="DecodedModel"/> returns it without quotes.</param>
/// <param name="FullVersionList">The Sec-CH-UA-Full-Version-List header value as sent. <see cref="DecodedFullVersionList"/> returns the brands.</param>
/// <param name="UA">The Sec-CH-UA header value as sent. <see cref="DecodedUA"/> returns the brands.</param>
/// <param name="Mobile">The Sec-CH-UA-Mobile header value decoded with <see cref="HttpClientHintsInterpreter.IsMobile"/>.</param>
/// <param name="Headers">The original headers dictionary containing all the request headers.</param>
/// <remarks>
/// The <c>Decoded*</c> properties decode the matching property with the methods of <see cref="HttpClientHintsInterpreter"/>
/// on every access. They store nothing, so they are not part of record equality and follow <c>with</c> expressions.
/// </remarks>
public record class HttpClientHints(string? UserAgent, string? Platform, string? PlatformVersion,
    string? Architecture, string? Model, string? FullVersionList, string? UA, bool? Mobile, IDictionary<string, StringValues> Headers)
{
    /// <summary>Gets the Sec-CH-UA-Bitness header value as sent, including quotes, for example <c>"64"</c>.</summary>
    /// <value>The header value, or <see langword="null"/> if the header is missing. <see cref="DecodedBitness"/> returns it without quotes.</value>
    public string? Bitness { get; init; }

    /// <summary>Gets the Sec-CH-UA-WoW64 header value decoded with <see cref="HttpClientHintsInterpreter.DecodeStructuredBoolean"/>.</summary>
    /// <value>
    /// <see langword="true"/> if a 32-bit browser runs on 64-bit Windows, <see langword="false"/> if not, or
    /// <see langword="null"/> if the header is missing or malformed.
    /// </value>
    public bool? Wow64 { get; init; }

    /// <summary>Gets the Sec-CH-UA-Form-Factors header value as sent, for example <c>"Desktop"</c>.</summary>
    /// <value>The header value, or <see langword="null"/> if the header is missing. <see cref="DecodedFormFactors"/> returns the form factors.</value>
    public string? FormFactors { get; init; }

    /// <summary>Gets <see cref="UA"/> decoded into its brands.</summary>
    /// <value>The brands, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeBrandList"/>.</value>
    public IReadOnlyList<HttpClientHintsBrand>? DecodedUA => HttpClientHintsInterpreter.DecodeBrandList(UA);

    /// <summary>Gets <see cref="FullVersionList"/> decoded into its brands with their full versions.</summary>
    /// <value>The brands, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeBrandList"/>.</value>
    public IReadOnlyList<HttpClientHintsBrand>? DecodedFullVersionList => HttpClientHintsInterpreter.DecodeBrandList(FullVersionList);

    /// <summary>Gets <see cref="Bitness"/> decoded, for example <c>64</c> instead of <c>"64"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedBitness => HttpClientHintsInterpreter.DecodeStructuredString(Bitness);

    /// <summary>Gets <see cref="FormFactors"/> decoded, for example <c>Desktop</c> instead of <c>"Desktop"</c>.</summary>
    /// <value>The form factors, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredStringList"/>.</value>
    public IReadOnlyList<string>? DecodedFormFactors => HttpClientHintsInterpreter.DecodeStructuredStringList(FormFactors);

    /// <summary>Gets <see cref="Platform"/> decoded, for example <c>Windows</c> instead of <c>"Windows"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatform => HttpClientHintsInterpreter.DecodeStructuredString(Platform);

    /// <summary>Gets <see cref="PlatformVersion"/> decoded, for example <c>19.0.0</c> instead of <c>"19.0.0"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatformVersion => HttpClientHintsInterpreter.DecodeStructuredString(PlatformVersion);

    /// <summary>Gets <see cref="Architecture"/> decoded, for example <c>x86</c> instead of <c>"x86"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedArchitecture => HttpClientHintsInterpreter.DecodeStructuredString(Architecture);

    /// <summary>Gets <see cref="Model"/> decoded, for example <c>Pixel 9</c> instead of <c>"Pixel 9"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedModel => HttpClientHintsInterpreter.DecodeStructuredString(Model);
}
