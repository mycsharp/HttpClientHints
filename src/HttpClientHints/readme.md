# MyCSharp.HttpClientHints

Read HTTP Client Hints with .NET.

- `HttpClientHints` record with the values of the `Sec-CH-UA` request headers and access to all received headers
- `Decoded*` properties return decoded values: strings without quotes and escapes, the form factors as a list, and the brands of `Sec-CH-UA` and `Sec-CH-UA-Full-Version-List`
- `HttpClientHintsInterpreter` decodes HTTP Structured Field strings, booleans, string lists and brand lists ([RFC 9651](https://www.rfc-editor.org/rfc/rfc9651.html))
- Trimming and Native AOT compatible

Repository:
https://github.com/mycsharp/HttpClientHints

## Install

```bash
dotnet add package MyCSharp.HttpClientHints
```

For ASP.NET Core, install [MyCSharp.HttpClientHints.AspNetCore](https://nubrowse.com/packages/MyCSharp.HttpClientHints.AspNetCore) instead. It requests the hints with `Accept-CH` and reads them from the `HttpContext`.

## Decode header values

```csharp
using MyCSharp.HttpClientHints;

string? platform = HttpClientHintsInterpreter.DecodeStructuredString("\"Windows\""); // Windows
string? unquoted = HttpClientHintsInterpreter.DecodeStructuredString("Windows"); // Windows, returned as received
bool? wow64 = HttpClientHintsInterpreter.DecodeStructuredBoolean("?0"); // false
IReadOnlyList<string>? formFactors = HttpClientHintsInterpreter.DecodeStructuredStringList("\"Desktop\", \"XR\""); // Desktop, XR

IReadOnlyList<HttpClientHintsBrand>? brands = HttpClientHintsInterpreter.DecodeBrandList("\"Chromium\";v=\"142\", \"Not_A Brand\";v=\"99\"");
// [0]: Brand = Chromium, Version = 142, IsGrease = false
// [1]: Brand = Not_A Brand, Version = 99, IsGrease = true
```

Missing values, malformed quoted strings and invalid boolean or list values yield `null`. `DecodeStructuredString` also accepts unquoted input and returns that input unchanged. `IsGrease` identifies the brand with a changing name that Chromium-based browsers add to the brand lists. [Decoded values](https://github.com/mycsharp/HttpClientHints#decoded-values)
