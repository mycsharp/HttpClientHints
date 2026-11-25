// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Sets the <c>Accept-CH</c> response header and, if configured, <c>Critical-CH</c>, <c>Vary</c> and the obsolete
/// <c>Accept-CH-Lifetime</c>. Added by <see cref="HttpClientHintsRegistration.UseHttpClientHints"/>.
/// </summary>
/// <remarks>
/// The middleware stays async: a synchronous exception of the next middleware results in a faulted task, and
/// <see cref="AsyncLocal{T}"/> changes of the next middleware do not flow back to the caller.
/// </remarks>
internal sealed class HttpClientHintsRequestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpClientHintsMiddlewareConfig _options;
    private readonly Func<object, Task> _restoreVary;

    public HttpClientHintsRequestMiddleware(RequestDelegate next, IOptions<HttpClientHintsMiddlewareConfig> options)
    {
        _next = next;
        _options = options.Value;
        _restoreVary = RestoreVary;
    }

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

            if (_options.HasVaryHeader)
            {
                // Make Vary available to downstream middleware, then restore hints removed by MVC cache filters.
                context.Features.Set(_options);
                _options.MergeVary(headers);
                context.Response.OnStarting(_restoreVary, context.Response);
            }

            if (_options.HasLifetime)
            {
                headers["Accept-CH-Lifetime"] = _options.LifeTime;
            }
        }

        await _next(context).ConfigureAwait(false);
    }

    private Task RestoreVary(object state)
    {
        _options.MergeVary(((HttpResponse)state).Headers);
        return Task.CompletedTask;
    }
}
