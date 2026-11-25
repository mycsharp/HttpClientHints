// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
        if (_options.ResponseHeader is string acceptCH)
        {
            IHeaderDictionary headers = context.Response.Headers;
            headers["Accept-CH"] = acceptCH;

            if (_options.HasCriticalHeader)
            {
                headers["Critical-CH"] = _options.CriticalHeader;
            }

            if (_options.HasLifetime)
            {
                headers["Accept-CH-Lifetime"] = _options.LifeTime;
            }

            if (_options.HasVaryHeader)
            {
                IHttpResponseFeature original = context.Features.GetRequiredFeature<IHttpResponseFeature>();
                VaryResponseFeature feature = new(original, _options);
                _options.MergeVary(headers);
                context.Response.OnStarting(_restoreVary, context.Response);
                context.Features.Set<IHttpResponseFeature>(feature);
                try
                {
                    await _next(context).ConfigureAwait(false);
                }
                finally
                {
                    if (ReferenceEquals(context.Features.Get<IHttpResponseFeature>(), feature))
                    {
                        context.Features.Set(original);
                    }
                }

                // A cache outside this middleware can finalize a bodyless response after we return.
                if (!context.Response.HasStarted)
                {
                    _options.MergeVary(context.Response.Headers);
                }

                return;
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
