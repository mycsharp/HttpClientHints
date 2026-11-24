# MyCSharp.HttpClientHints.AspNetCore

ASP.NET Core integration for MyCSharp.HttpClientHints.

- Middleware that requests Client Hints with the `Accept-CH` response header
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
builder.Services.AddHttpClientHints(options =>
{
    options.Platform = true;
    options.Mobile = true;
    options.Lifetime = TimeSpan.FromDays(31);
});

app.UseHttpClientHints();
```

Read the hints of a request:

```csharp
HttpClientHints hints = httpContext.GetClientHints(); // reads the headers once per request
HttpClientHintsView view = httpContext.GetClientHintsView(); // reads the headers on every access, does not allocate
string? platform = view.DecodedPlatform; // Windows instead of "Windows"
bool? isMobile = view.Mobile;
```

All properties are nullable, because the client may not send a hint.

## Decoded values

`HttpClientHints` and `HttpClientHintsView` have `DecodedPlatform`, `DecodedPlatformVersion`, `DecodedArchitecture`, and `DecodedModel` properties that return the header values without quotes. [Decoded values](https://github.com/mycsharp/HttpClientHints#decoded-values)

## HttpContext feature

The optional `app.UseHttpClientHintsFeature()` middleware registers `IHttpClientHintsFeature` in `HttpContext.Features`. [HttpContext feature](https://github.com/mycsharp/HttpClientHints#httpcontext-feature)

```csharp
app.UseHttpClientHintsFeature();

IHttpClientHintsFeature? feature = httpContext.Features.Get<IHttpClientHintsFeature>();
bool? isMobile = feature?.View.Mobile;
```
