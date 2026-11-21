// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Extension methods for accessing HTTP Client Hints from the <see cref="HttpContext"/> and its headers.
/// </summary>
public static class HttpClientHintsHttpContextExtensions
{
    /// <summary>
    /// The cache key used to store the client hints in the HttpContext.Items dictionary.
    /// </summary>
    private static readonly object s_clientHintsCacheKey = new();

    /// <summary>
    /// Retrieves the <see cref="HttpClientHints"/> from the current HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context containing the request headers.</param>
    /// <returns>An instance of <see cref="HttpClientHints"/> populated with the relevant header values.</returns>
    public static HttpClientHints GetClientHints(this HttpContext context)
    {
        IDictionary<object, object?> items = context.Items;
        if (items.TryGetValue(s_clientHintsCacheKey, out object? cached) && cached is HttpClientHints hints)
        {
            return hints;
        }

        HttpClientHints newHints = context.Request.Headers.GetClientHints();
        items[s_clientHintsCacheKey] = newHints;
        return newHints;
    }

    /// <summary>
    /// Returns a view over the request headers for accessing Client Hints lazily.
    /// </summary>
    public static HttpClientHintsView GetClientHintsView(this HttpContext context)
        => new(context.Request.Headers);

    /// <summary>
    /// Returns a view over the provided headers for accessing Client Hints lazily.
    /// </summary>
    public static HttpClientHintsView GetClientHintsView(this IHeaderDictionary headers)
        => new(headers);

    /// <summary>
    /// Retrieves the <see cref="HttpClientHints"/> from the specified header dictionary.
    /// </summary>
    /// <param name="headers">The header dictionary containing the Client Hints headers.</param>
    /// <returns>An instance of <see cref="HttpClientHints"/> populated with the relevant header values.</returns>
    public static HttpClientHints GetClientHints(this IHeaderDictionary headers)
    {
        HttpClientHintsView view = new(headers);
        return view.BuildSnapshot();
    }
}
