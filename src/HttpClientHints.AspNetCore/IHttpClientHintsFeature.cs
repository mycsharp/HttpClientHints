// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Provides the HTTP Client Hints of the current request through <see cref="HttpContext.Features"/>.
/// </summary>
/// <remarks>
/// <see cref="HttpClientHintsRegistration.UseHttpClientHintsFeature"/> adds a middleware that registers this feature
/// for every request. The registration replaces an <see cref="IHttpClientHintsFeature"/> that was registered earlier
/// in the pipeline. To use your own implementation, register it after the middleware. Without registration,
/// <c>HttpContext.Features.Get&lt;IHttpClientHintsFeature&gt;()</c> returns <see langword="null"/>.
/// The extension methods in <see cref="HttpClientHintsHttpContextExtensions"/> work with and without the feature.
/// The default implementation references the <see cref="HttpContext"/>, so do not keep it beyond the request.
/// </remarks>
public interface IHttpClientHintsFeature
{
    /// <summary>
    /// Gets a view over the current request headers. Same as <see cref="HttpClientHintsHttpContextExtensions.GetClientHintsView(HttpContext)"/>.
    /// </summary>
    /// <value>A view that reads the current request headers on every property access.</value>
    HttpClientHintsView View { get; }

    /// <summary>
    /// Gets the snapshot of the Client Hints of the request. Same as <see cref="HttpClientHintsHttpContextExtensions.GetClientHints(HttpContext)"/>.
    /// </summary>
    /// <value>The record that is built on the first access and returned for every further access during the request.</value>
    /// <remarks>
    /// The properties keep the values read on the first access. <see cref="HttpClientHints.Headers"/> references the
    /// request headers themselves, so it shows later changes to the headers.
    /// </remarks>
    HttpClientHints Snapshot { get; }
}
