# MyCSharp.HttpClientHints

Read HTTP Client Hints with .NET

Since 3.2, `HttpClientHints` and `HttpClientHintsView` have `DecodedPlatform`, `DecodedPlatformVersion`, `DecodedArchitecture`, and `DecodedModel` properties that return the header values without quotes. [Decoded values](https://github.com/mycsharp/HttpClientHints#decoded-values)

Since 3.3, the optional `app.UseHttpClientHintsFeature()` middleware registers `IHttpClientHintsFeature` in `HttpContext.Features`. [HttpContext feature](https://github.com/mycsharp/HttpClientHints#httpcontext-feature)

https://github.com/mycsharp/HttpClientHints
