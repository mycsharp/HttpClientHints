// Copyright © myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Middleware for adding HTTP Client Hints headers to the response.
/// Initializes a new instance of the <see cref="HttpClientHintsRequestMiddleware"/> class.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <param name="options">The options for configuring the middleware.</param>
public class HttpClientHintsRequestMiddleware(RequestDelegate next, IOptions<HttpClientHintsMiddlewareConfig> options)
{
    private readonly HttpClientHintsMiddlewareConfig _options = options.Value;

    /// <summary>
    /// Middleware invocation method that processes the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Add Client Hints headers to the response
        if (string.IsNullOrEmpty(_options.ResponseHeader) is false)
        {
            context.Response.Headers["Accept-CH"] = _options.ResponseHeader;

            if (_options.LifeTime is not null)
            {
                context.Response.Headers["Accept-CH-Lifetime"] = _options.LifeTime;
            }
        }

        // Call the next middleware in the pipeline
        await next(context);
    }
}
