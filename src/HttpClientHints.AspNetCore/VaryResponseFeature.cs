// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace MyCSharp.HttpClientHints.AspNetCore;

// ResponseCaching reads Response.Headers before the server's OnStarting callbacks.
// Give cache readers the merged Vary even when an endpoint has just replaced it.
internal sealed class VaryResponseFeature(IHttpResponseFeature inner, HttpClientHintsMiddlewareConfig options) : IHttpResponseFeature
{
    public IHeaderDictionary Headers
    {
        get
        {
            IHeaderDictionary headers = inner.Headers;
            if (!inner.HasStarted)
            {
                options.MergeVary(headers);
            }

            return headers;
        }
        set => inner.Headers = value;
    }

    public int StatusCode { get => inner.StatusCode; set => inner.StatusCode = value; }
    public string? ReasonPhrase { get => inner.ReasonPhrase; set => inner.ReasonPhrase = value; }
    public bool HasStarted => inner.HasStarted;

    [Obsolete("Use IHttpResponseBodyFeature.Stream instead.")]
    public Stream Body { get => inner.Body; set => inner.Body = value; }

    public void OnStarting(Func<object, Task> callback, object state) => inner.OnStarting(callback, state);
    public void OnCompleted(Func<object, Task> callback, object state) => inner.OnCompleted(callback, state);
}
