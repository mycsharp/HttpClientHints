// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Mvc.Filters;

namespace MyCSharp.HttpClientHints.AspNetCore;

// ResponseCachingMiddleware captures Vary on the first body write, before OnStarting.
// Restore hints after MVC's ResponseCache filter and before the action or its result writes a body.
internal sealed class HttpClientHintsVaryFilterAttribute : ActionFilterAttribute
{
    public HttpClientHintsVaryFilterAttribute() => Order = int.MaxValue;

    public override void OnActionExecuting(ActionExecutingContext context) => MergeVary(context);

    public override void OnResultExecuting(ResultExecutingContext context) => MergeVary(context);

    private static void MergeVary(FilterContext context)
        => context.HttpContext.Features.Get<HttpClientHintsMiddlewareConfig>()?.MergeVary(context.HttpContext.Response.Headers);
}
