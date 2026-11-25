// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Sets the <c>Accept-CH</c> response header and, if configured, <c>Critical-CH</c>, <c>Vary</c> and the obsolete
/// <c>Accept-CH-Lifetime</c>. Added by <see cref="HttpClientHintsRegistration.UseHttpClientHints"/>.
/// </summary>
/// <remarks>
/// The middleware stays async: a synchronous exception of the next middleware results in a faulted task, and
/// <see cref="AsyncLocal{T}"/> changes of the next middleware do not flow back to the caller.
/// </remarks>
internal sealed class HttpClientHintsRequestMiddleware(RequestDelegate next, IOptions<HttpClientHintsMiddlewareConfig> options)
{
    private readonly HttpClientHintsMiddlewareConfig _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.HasResponseHeaders)
        {
            IHeaderDictionary headers = context.Response.Headers;
            headers["Accept-CH"] = _options.ResponseHeader;

            if (_options.HasCriticalHeader)
            {
                headers["Critical-CH"] = _options.CriticalHeader;
            }

            // A pipeline that runs again for the same response, for example after UseStatusCodePagesWithReExecute,
            // must not add the hints twice
            if (_options.HasVaryHeader && !Contains(headers.Vary, _options.VaryHeader!))
            {
                headers.Vary = StringValues.Concat(headers.Vary, _options.VaryHeader);
            }

            if (_options.HasLifetime)
            {
                headers["Accept-CH-Lifetime"] = _options.LifeTime;
            }
        }

        await next(context).ConfigureAwait(false);
    }

    private static bool Contains(StringValues values, string value)
    {
        foreach (string? existing in values)
        {
            if (string.Equals(existing, value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
