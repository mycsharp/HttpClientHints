// Copyright © myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Extension methods for accessing HTTP Client Hints from the <see cref="HttpContext"/> and its headers.
/// </summary>
public static class HttpClientHintsHttpContextExtensions
{
    /// <summary>
    /// Retrieves the <see cref="HttpClientHints"/> from the current HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context containing the request headers.</param>
    /// <returns>An instance of <see cref="HttpClientHints"/> populated with the relevant header values.</returns>
    public static HttpClientHints GetClientHints(this HttpContext context)
    {
        IHeaderDictionary headers = context.Request.Headers;
        return headers.GetClientHints();
    }

    /// <summary>
    /// Retrieves the <see cref="HttpClientHints"/> from the specified header dictionary.
    /// </summary>
    /// <param name="headers">The header dictionary containing the Client Hints headers.</param>
    /// <returns>An instance of <see cref="HttpClientHints"/> populated with the relevant header values.</returns>
    public static HttpClientHints GetClientHints(this IHeaderDictionary headers)
    {
        // user agent
        string? userAgent = headers["User-Agent"].FirstOrDefault();
        string? ua = headers["Sec-CH-UA"].FirstOrDefault();

        // platform
        string? platform = headers["Sec-CH-UA-Platform"].FirstOrDefault();
        string? platformVersion = headers["Sec-CH-UA-Platform-Version"].FirstOrDefault();

        // architecture
        string? architecture = headers["Sec-CH-UA-Arch"].FirstOrDefault();

        // other
        string? fullVersionList = headers["Sec-CH-UA-Full-Version-List"].FirstOrDefault();

        // device
        string? model = headers["Sec-CH-UA-Model"].FirstOrDefault();
        bool? mobile = HttpClientHintsInterpreter.IsMobile(headers["Sec-CH-UA-Mobile"].FirstOrDefault());

        // return the HttpClientHints record
        return new(userAgent, platform, platformVersion, architecture, model, fullVersionList, ua, mobile, headers);
    }
}
