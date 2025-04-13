// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Represents HTTP Client Hints retrieved from the request headers.
/// </summary>
/// <param name="UserAgent">The User-Agent string from the request headers.</param>
/// <param name="Platform">The platform information from the Sec-CH-UA-Platform header.</param>
/// <param name="PlatformVersion">The version of the platform from the Sec-CH-UA-Platform-Version header.</param>
/// <param name="Architecture">The architecture from the Sec-CH-UA-Arch header.</param>
/// <param name="Model">The device model from the Sec-CH-UA-Model header.</param>
/// <param name="FullVersionList">The full version list from the Sec-CH-UA-Full-Version-List header.</param>
/// <param name="UA">The value of the Sec-CH-UA header.</param>
/// <param name="Mobile">A nullable boolean indicating whether the device is mobile, derived from the Sec-CH-UA-Mobile header.</param>
/// <param name="Headers">The original headers dictionary containing all the request headers.</param>
public record class HttpClientHints(string? UserAgent, string? Platform, string? PlatformVersion,
    string? Architecture, string? Model, string? FullVersionList, string? UA, bool? Mobile, IDictionary<string, StringValues> Headers);
