# MyCSharp.HttpClientHints

Read HTTP Client Hints with .NET.

- `HttpClientHints` record with the values of the `Sec-CH-UA` request headers and access to all received headers
- `Decoded*` properties return quoted header values such as `"Windows"` without quotes and escapes
- `HttpClientHintsInterpreter` interprets `Sec-CH-UA-Mobile` and decodes other quoted header values ([RFC 9651 § 4.2.5](https://www.rfc-editor.org/rfc/rfc9651.html#section-4.2.5))
- Trimming and Native AOT compatible

Repository:
https://github.com/mycsharp/HttpClientHints

## Install

```bash
dotnet add package MyCSharp.HttpClientHints
```

For ASP.NET Core, install [MyCSharp.HttpClientHints.AspNetCore](https://nubrowse.com/packages/MyCSharp.HttpClientHints.AspNetCore) instead. It requests the hints with `Accept-CH` and reads them from the `HttpContext`.

## Decoded values

The `HttpClientHints` record has `DecodedPlatform`, `DecodedPlatformVersion`, `DecodedArchitecture`, and `DecodedModel` properties that return the header values without quotes, and `HttpClientHintsInterpreter.DecodeStructuredString` decodes other quoted header values.

```csharp
string? bitness = HttpClientHintsInterpreter.DecodeStructuredString("\"64\""); // 64
bool? isMobile = HttpClientHintsInterpreter.IsMobile("?1"); // true
```

[Decoded values](https://github.com/mycsharp/HttpClientHints#decoded-values)
