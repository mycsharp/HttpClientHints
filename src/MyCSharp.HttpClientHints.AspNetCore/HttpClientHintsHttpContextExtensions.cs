// Copyright © myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Extension methods for accessing HTTP Client Hints from the <see cref="HttpContext"/> and its headers.
/// </summary>
public static class HttpClientHintsHttpContextExtensions
{
    /// <summary>
    /// The cache key used to store the client hints in the HttpContext.Items dictionary.
    /// </summary>
    private const string ClientHintsCacheKey = "__HttpClientHints";

    /// <summary>
    /// Retrieves the <see cref="HttpClientHints"/> from the current HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context containing the request headers.</param>
    /// <returns>An instance of <see cref="HttpClientHints"/> populated with the relevant header values.</returns>
    public static HttpClientHints GetClientHints(this HttpContext context)
    {
        // Check if client hints are already cached for this request
        if (context.Items.TryGetValue(ClientHintsCacheKey, out var cached) && cached is HttpClientHints hints)
        {
            return hints;
        }
        
        // Create and cache new client hints
        var newHints = context.Request.Headers.GetClientHints();
        context.Items[ClientHintsCacheKey] = newHints;
        return newHints;
    }

    /// <summary>
    /// Retrieves the <see cref="HttpClientHints"/> from the specified header dictionary.
    /// </summary>
    /// <param name="headers">The header dictionary containing the Client Hints headers.</param>
    /// <returns>An instance of <see cref="HttpClientHints"/> populated with the relevant header values.</returns>
    public static HttpClientHints GetClientHints(this IHeaderDictionary headers)
    {
        // User Agent
        headers.TryGetValue("User-Agent", out StringValues userAgentValues);
        string? userAgent = userAgentValues.Count > 0 ? userAgentValues[0] : null;
        
        headers.TryGetValue("Sec-CH-UA", out StringValues uaValues);
        string? ua = uaValues.Count > 0 ? uaValues[0] : null;

        // Platform
        headers.TryGetValue("Sec-CH-UA-Platform", out StringValues platformValues);
        string? platform = platformValues.Count > 0 ? platformValues[0] : null;
        
        headers.TryGetValue("Sec-CH-UA-Platform-Version", out StringValues platformVersionValues);
        string? platformVersion = platformVersionValues.Count > 0 ? platformVersionValues[0] : null;

        // Architecture
        headers.TryGetValue("Sec-CH-UA-Arch", out StringValues architectureValues);
        string? architecture = architectureValues.Count > 0 ? architectureValues[0] : null;

        // Other
        headers.TryGetValue("Sec-CH-UA-Full-Version-List", out StringValues fullVersionListValues);
        string? fullVersionList = fullVersionListValues.Count > 0 ? fullVersionListValues[0] : null;

        // Device
        headers.TryGetValue("Sec-CH-UA-Model", out StringValues modelValues);
        string? model = modelValues.Count > 0 ? modelValues[0] : null;
        
        headers.TryGetValue("Sec-CH-UA-Mobile", out StringValues mobileValues);
        bool? mobile = HttpClientHintsInterpreter.IsMobile(mobileValues.Count > 0 ? mobileValues[0] : null);

        // Return the HttpClientHints record
        return new(userAgent, platform, platformVersion, architecture, model, fullVersionList, ua, mobile, headers);
    }
}
