# MyCSharp.HttpClientHints.AspNetCore

ASP.NET Core integration for MyCSharp.HttpClientHints.

- Middleware that requests Client Hints with the `Accept-CH` response header, and sets `Critical-CH` and `Vary` if configured
- `GetClientHints()` and `GetClientHintsView()` on `HttpContext` and `IHeaderDictionary`
- Optional `IHttpClientHintsFeature` in `HttpContext.Features`
- Trimming and Native AOT compatible

Repository:
https://github.com/mycsharp/HttpClientHints

## Install

```bash
dotnet add package MyCSharp.HttpClientHints.AspNetCore
```

## Quick start

Configure the hints to request and register the middleware:

```csharp
using MyCSharp.HttpClientHints;
using MyCSharp.HttpClientHints.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClientHints(options =>
{
    options.Platform = true;        // Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version
    options.FullVersionList = true; // Sec-CH-UA-Full-Version-List
});

var app = builder.Build();
app.UseHttpClientHints();
app.MapGet("/", (HttpContext context) => context.GetClientHintsView().DecodedPlatform);
app.Run();
```

Without options, `Accept-CH` requests only the low-entropy hints `Sec-CH-UA` and `Sec-CH-UA-Mobile`, which browsers send with every request. The options `Platform`, `Architecture`, `Device` and `FullVersionList` request high-entropy hints and are `false` by default. Disabling every hint sends an empty `Accept-CH` to clear an earlier opt-in. [Options](https://github.com/mycsharp/HttpClientHints#options)

Read the hints of a request:

```csharp
HttpClientHints hints = httpContext.GetClientHints(); // reads the headers once per request
HttpClientHintsView view = httpContext.GetClientHintsView(); // creating the view does not allocate
string? platform = view.DecodedPlatform; // Windows instead of "Windows"
bool? isMobile = view.Mobile;
IReadOnlyList<HttpClientHintsBrand>? brands = view.DecodedFullVersionList; // brands with full versions
```

Each hint property returns `null` when its header is absent. Reading a single raw field line does not allocate. Reading repeated Client Hint lines joins them into a string on each access through the view.

## Caching by a Client Hint

Set `options.Vary = true` to include the requested hints in the HTTP `Vary` response header. With ASP.NET Core `UseOutputCache()`, also set the endpoint's cache key, for example `.CacheOutput(policy => policy.SetVaryByHeader("Sec-CH-UA-Mobile"))` for a layout that depends on the mobile hint. [Complete Output Caching example](https://github.com/mycsharp/HttpClientHints#aspnet-core-output-caching)

## Decoded values

`HttpClientHints` and `HttpClientHintsView` return every header value as sent, and decoded: `DecodedPlatform`, `DecodedPlatformVersion`, `DecodedArchitecture`, `DecodedBitness` and `DecodedModel` without quotes, `DecodedFormFactors` as a list, `DecodedUA` and `DecodedFullVersionList` as brands, and `Mobile` and `Wow64` as booleans. [Decoded values](https://github.com/mycsharp/HttpClientHints#decoded-values)

## HttpContext feature

The optional `app.UseHttpClientHintsFeature()` middleware registers `IHttpClientHintsFeature` in `HttpContext.Features`. [HttpContext feature](https://github.com/mycsharp/HttpClientHints#httpcontext-feature)

```csharp
app.UseHttpClientHintsFeature();

IHttpClientHintsFeature? feature = httpContext.Features.Get<IHttpClientHintsFeature>();
bool? isMobile = feature?.View.Mobile;
```
