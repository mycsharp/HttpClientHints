// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Default <see cref="IHttpClientHintsFeature"/> registered by <see cref="HttpClientHintsFeatureMiddleware"/>.
/// </summary>
/// <remarks>
/// The middleware creates one instance per request, so the type holds only the context reference and has the minimum
/// object size. <see cref="View"/> and <see cref="Snapshot"/> use the extension methods and compute nothing up front.
/// </remarks>
internal sealed class HttpClientHintsFeature(HttpContext context) : IHttpClientHintsFeature
{
    /// <inheritdoc />
    public HttpClientHintsView View => context.GetClientHintsView();

    /// <inheritdoc />
    public HttpClientHints Snapshot => context.GetClientHints();
}
