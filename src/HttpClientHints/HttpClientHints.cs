// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Represents HTTP Client Hints retrieved from the request headers.
/// </summary>
/// <param name="UserAgent">The User-Agent string from the request headers.</param>
/// <param name="Platform">The decoded platform from the Sec-CH-UA-Platform header.</param>
/// <param name="PlatformVersion">The decoded platform version from the Sec-CH-UA-Platform-Version header.</param>
/// <param name="Architecture">The decoded architecture from the Sec-CH-UA-Arch header.</param>
/// <param name="Model">The decoded device model from the Sec-CH-UA-Model header.</param>
/// <param name="FullVersionList">The full version list from the Sec-CH-UA-Full-Version-List header.</param>
/// <param name="UA">The value of the Sec-CH-UA header.</param>
/// <param name="Mobile">A nullable boolean indicating whether the device is mobile, derived from the Sec-CH-UA-Mobile header.</param>
/// <param name="Headers">The original headers dictionary containing all the request headers.</param>
/// <remarks>
/// When MyCSharp.HttpClientHints.AspNetCore creates this record, <see cref="Platform"/>, <see cref="PlatformVersion"/>,
/// <see cref="Architecture"/>, and <see cref="Model"/> contain decoded strings. They are <see langword="null"/> if the
/// quoted value is malformed or has parameters. The <c>Raw*</c> properties keep the original header values.
/// A <c>with</c> expression that changes a decoded property does not change the matching <c>Raw*</c> property.
/// </remarks>
public record class HttpClientHints(string? UserAgent, string? Platform, string? PlatformVersion,
    string? Architecture, string? Model, string? FullVersionList, string? UA, bool? Mobile, IDictionary<string, StringValues> Headers)
{
    /// <summary>Gets the original Sec-CH-UA-Platform value, including any quotes and escapes.</summary>
    public string? RawPlatform { get; init; }

    /// <summary>Gets the original Sec-CH-UA-Platform-Version value, including any quotes and escapes.</summary>
    public string? RawPlatformVersion { get; init; }

    /// <summary>Gets the original Sec-CH-UA-Arch value, including any quotes and escapes.</summary>
    public string? RawArchitecture { get; init; }

    /// <summary>Gets the original Sec-CH-UA-Model value, including any quotes and escapes.</summary>
    public string? RawModel { get; init; }
}
