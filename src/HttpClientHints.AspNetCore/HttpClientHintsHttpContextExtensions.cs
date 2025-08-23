// Copyright © https://myCSharp.de - all rights reserved

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
        if (context.Items.TryGetValue(ClientHintsCacheKey, out object? cached) && cached is HttpClientHints hints)
        {
            return hints;
        }

        // Create and cache new client hints
        HttpClientHints newHints = context.Request.Headers.GetClientHints();
        context.Items[ClientHintsCacheKey] = newHints;
        return newHints;
    }

    /// <summary>
    /// Returns a zero-allocation view over the request headers for accessing Client Hints lazily.
    /// </summary>
    public static HttpClientHintsView GetClientHintsView(this HttpContext context)
        => new(context.Request.Headers);

    /// <summary>
    /// Returns a zero-allocation view over the provided headers for accessing Client Hints lazily.
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
        // Use the non-allocating view to gather values and build a snapshot.
    HttpClientHintsView view = new(headers);
    return view.BuildSnapshot();
    }
}
