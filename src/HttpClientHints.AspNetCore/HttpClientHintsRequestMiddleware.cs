// Copyright � https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Middleware for processing HTTP Client Hints in ASP.NET Core requests.
/// This middleware adds Client Hints headers to HTTP responses based on configuration.
/// </summary>
/// <remarks>
/// This middleware adds the 'Accept-CH' header and optionally the 'Accept-CH-Lifetime' header
/// to responses when configured appropriately.
/// </remarks>
public class HttpClientHintsRequestMiddleware(RequestDelegate next, IOptions<HttpClientHintsMiddlewareConfig> options)
{
    // Cache the options value and pre-compute conditions to avoid repeated checks
    private readonly HttpClientHintsMiddlewareConfig _options = options.Value;

    /// <summary>
    /// Middleware invocation method that processes the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.HasResponseHeaders)
        {
            // Set headers directly without additional checks
            context.Response.Headers["Accept-CH"] = _options.ResponseHeader;

            if (_options.HasLifetime)
            {
                context.Response.Headers["Accept-CH-Lifetime"] = _options.LifeTime;
            }
        }

        // Call the next middleware in the pipeline
        await next(context).ConfigureAwait(false);
    }
}
