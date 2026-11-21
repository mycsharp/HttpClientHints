// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Represents HTTP Client Hints retrieved from the request headers.
/// </summary>
/// <param name="UserAgent">The User-Agent string from the request headers.</param>
/// <param name="Platform">The Sec-CH-UA-Platform header value as sent, including quotes. <see cref="DecodedPlatform"/> returns it without quotes.</param>
/// <param name="PlatformVersion">The Sec-CH-UA-Platform-Version header value as sent, including quotes. <see cref="DecodedPlatformVersion"/> returns it without quotes.</param>
/// <param name="Architecture">The Sec-CH-UA-Arch header value as sent, including quotes. <see cref="DecodedArchitecture"/> returns it without quotes.</param>
/// <param name="Model">The Sec-CH-UA-Model header value as sent, including quotes. <see cref="DecodedModel"/> returns it without quotes.</param>
/// <param name="FullVersionList">The full version list from the Sec-CH-UA-Full-Version-List header.</param>
/// <param name="UA">The value of the Sec-CH-UA header.</param>
/// <param name="Mobile">A nullable boolean indicating whether the device is mobile, derived from the Sec-CH-UA-Mobile header.</param>
/// <param name="Headers">The original headers dictionary containing all the request headers.</param>
/// <remarks>
/// The <c>Decoded*</c> properties decode the matching property with <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>
/// on every access. They store nothing, so they are not part of record equality and follow <c>with</c> expressions.
/// </remarks>
public record class HttpClientHints(string? UserAgent, string? Platform, string? PlatformVersion,
    string? Architecture, string? Model, string? FullVersionList, string? UA, bool? Mobile, IDictionary<string, StringValues> Headers)
{
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
