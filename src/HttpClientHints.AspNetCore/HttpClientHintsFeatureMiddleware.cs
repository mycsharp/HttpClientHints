// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Registers <see cref="IHttpClientHintsFeature"/> for every request.
/// Added by <see cref="HttpClientHintsRegistration.UseHttpClientHintsFeature"/>.
/// </summary>
internal sealed class HttpClientHintsFeatureMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Features.Set<IHttpClientHintsFeature>(new HttpClientHintsFeature(context));

        // Nothing runs after the next middleware, so its task is returned without an async state machine.
        return next(context);
    }
}
