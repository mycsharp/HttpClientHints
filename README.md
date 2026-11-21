# MyCSharp.HttpClientHints

Reading HTTP Client Hints with .NET

## NuGet

| NuGet | Install |
|-|-|
| [![MyCSharp.HttpClientHints](https://img.shields.io/nuget/v/MyCSharp.HttpClientHints.svg?logo=nuget&label=MyCSharp.HttpClientHints)](https://www.nuget.org/packages/MyCSharp.HttpClientHints) | `dotnet add package MyCSharp.HttpClientHints` |
| [![MyCSharp.HttpClientHints.AspNetCore](https://img.shields.io/nuget/v/MyCSharp.HttpClientHints.AspNetCore.svg?logo=nuget&label=MyCSharp.HttpClientHints.AspNetCore)](https://www.nuget.org/packages/MyCSharp.HttpClientHints.AspNetCore) | `dotnet add package MyCSharp.HttpClientHints.AspNetCore` |


## Client Hints

HTTP client hints are intended to completely replace the user agents (see [mycsharp/HttpUserAgentParser](https://github.com/mycsharp/HttpUserAgentParser)) of the browser in the long term and pay more attention to data protection. We are currently in a phase in which client hints are generally regarded as experimental and therefore do not represent a complete alternative.

Furthermore, in contrast to the User-Agent header, not all client hints are provided automatically, but only on request after the first response from the client.

## Usage

The basic idea behind the implementation of `MyCSharp.HttpClientHints` is to have as little impact as possible on performance and middleware.
The options are therefore registered solely via a simple configuration

```csharp
// Add Http Client Hint Options
builder.Services.AddHttpClientHints(options =>
  {
      options.UserAgent = true;
      options.Platform = true;
      options.Architecture = true;
      options.Device = true;
      options.Mobile = true;
      options.Lifetime = TimeSpan.FromDays(31);
  });
```

The corresponding headers are set by middleware, which must also be registered.

```csharp
...
app.UseHttpsRedirection();

// Register Http Client Hints Middleware
app.UseHttpClientHints();

app.UseRouting();
...
```

The client hints are not read by a separate dependency injection registration, but as an extension method to the header collection or to the HttpContext object.

```csharp
using MyCSharp.HttpClientHints.AspNetCore;

HttpClientHints clientHints = HttpClientHintsHttpContextExtensions.GetClientHints(httpContext);
```

There are two ways to read the Client Hints of a request:

| API | Returns | Behavior |
| --- | --- | --- |
| `httpContext.GetClientHints()` | `HttpClientHints` record | Reads the headers on the first call and returns the same instance for every further call during the request. |
| `httpContext.GetClientHintsView()` | `HttpClientHintsView` struct | Reads the current request headers on every property access. Creating the view and reading header values do not allocate. |

`GetClientHints()` and `GetClientHintsView()` are also available on `IHeaderDictionary`. `GetClientHints()` on headers builds a new record on every call.

```csharp
HttpClientHints clientHints = httpContext.GetClientHints();
HttpClientHintsView view = httpContext.GetClientHintsView();
bool? isMobile = view.Mobile;
```

Since potentially all client hints are optional, all properties are potentially nullable, which means that the client has not provided this data.

## Decoded values

Browsers send `Sec-CH-UA-Platform`, `Sec-CH-UA-Platform-Version`, `Sec-CH-UA-Arch`, and `Sec-CH-UA-Model` as quoted strings, for example `Sec-CH-UA-Platform: "Windows"`. `Platform`, `PlatformVersion`, `Architecture`, and `Model` return these header values as sent, including the quotes. Since 3.2, the matching `Decoded*` properties return the value without quotes and escapes.

| Request header | Property | Decoded property |
| --- | --- | --- |
| `Sec-CH-UA-Platform` | `Platform` | `DecodedPlatform` |
| `Sec-CH-UA-Platform-Version` | `PlatformVersion` | `DecodedPlatformVersion` |
| `Sec-CH-UA-Arch` | `Architecture` | `DecodedArchitecture` |
| `Sec-CH-UA-Model` | `Model` | `DecodedModel` |

```csharp
HttpClientHints hints = httpContext.GetClientHints();
string? platformHeader = hints.Platform;   // "Windows", including the two quote characters
bool isWindows = hints.DecodedPlatform == "Windows";
```

The `Decoded*` properties are available on `HttpClientHints` and `HttpClientHintsView`. They decode the value on every access and store nothing, so reading `Platform` or building a snapshot costs nothing extra. Store the decoded value in a local variable if you need it more than once. `HttpClientHintsInterpreter.DecodeStructuredString` decodes any other header value of this type.

An absent header yields `null`. A quoted empty value such as `Sec-CH-UA-Model: ""` decodes to `string.Empty`. Unquoted values pass through as supplied. A malformed quoted value yields `null`. Parameters are not supported: `Sec-CH-UA-Platform: "Windows";a=1` also yields `null`. [RFC 9651 § 4.2.5](https://www.rfc-editor.org/rfc/rfc9651.html#section-4.2.5)

`UA` and `FullVersionList` expose serialized brand lists. The `Headers` collection provides access to all original header values.

## Customization

Each option requests these headers with `Accept-CH`:

| Option | Requested headers |
| --- | --- |
| `UserAgent` | `User-Agent`, `Sec-CH-UA` |
| `Platform` | `Sec-CH-UA-Platform`, `Sec-CH-UA-Platform-Version` |
| `Architecture` | `Sec-CH-UA-Arch`, `Sec-CH-UA-Bitness` |
| `Device` | `Sec-CH-UA-Model` |
| `Mobile` | `Sec-CH-UA-Mobile` |

The `Additional` property can be used to request further headers. For example, `FullVersionList` stays `null` unless you request `Sec-CH-UA-Full-Version-List`.

```csharp
// Add Http Client Hint Options
builder.Services.AddHttpClientHints(options =>
  {
      options.UserAgent = true;
      options.Platform = true;
      options.Architecture = true;
      options.Device = true;
      options.Mobile = true;
      options.Additional = ["Sec-CH-UA-Full-Version-List"];
      options.Lifetime = TimeSpan.FromDays(31);
  });
```

The `HttpClientHints` object has a reference to all headers received, making it very easy to read them out. This includes headers without a property, such as `Sec-CH-UA-Bitness`. Optionally, you can also inherit from the `HttpClientHints` class itself to simplify access.

```csharp
HttpClientHints clientHints = HttpClientHintsHttpContextExtensions.GetClientHints(httpContext);
clientHints.Headers.TryGetValue("Sec-CH-UA-Bitness", out StringValues bitness);
```

Like the platform, `Sec-CH-UA-Bitness` is sent as a quoted string such as `"64"`. `HttpClientHintsInterpreter.DecodeStructuredString` removes the quotes from any header value of this type.

```csharp
string? decodedBitness = HttpClientHintsInterpreter.DecodeStructuredString(bitness); // 64
```

## Build and test

Use the .NET 11 SDK to build the solution. Both NuGet packages target .NET 8, 9, 10, and 11. The .NET 11 build uses `Microsoft.Extensions.Primitives` from the shared runtime. Install the corresponding runtimes to run the full test suite.

```shell
dotnet build -c Release
dotnet test -c Release --no-build
```

Line and branch coverage must stay at or above 90 %. CI collects coverage on every build and fails below this threshold. Run the same check locally with `just test-cov`, or call `./eng/Assert-CodeCoverage.ps1` after a test run with `--coverage --coverage-output-format cobertura`.

## Benchmark

Reading header values from the view does not allocate, and a snapshot allocates only the record. A `Decoded*` property allocates the decoded string of a quoted value on every access. Run the benchmarks with `dotnet run -c Release --project perf/HttpClientHints.Benchmarks --framework net11.0 -- --filter '*' --iterationCount 15 --iterationTime 200 --launchCount 2 --warmupCount 6`.

```
BenchmarkDotNet v0.16.0-preview.2, Windows 10 (10.0.19045.7846/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 61,64 GB Total, 31,05 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  Job-ZRYWOI : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

IterationCount=15  IterationTime=200ms  LaunchCount=2
WarmupCount=6
```

| Method                                | Mean      | Error     | StdDev    | Gen0   | Allocated |
|-------------------------------------- |----------:|----------:|----------:|-------:|----------:|
| ReadViewProperties                    | 18.956 ns | 0.3872 ns | 0.5299 ns |      - |         - |
| ReadDecodedViewProperties             | 36.646 ns | 0.5085 ns | 0.6961 ns | 0.0042 |      72 B |
| GetClientHintsFromHeaders             | 57.352 ns | 0.4389 ns | 0.6295 ns | 0.0052 |      88 B |
| GetClientHintsFromHeadersWithoutHints | 49.915 ns | 0.7589 ns | 1.1124 ns | 0.0050 |      88 B |
| GetClientHintsFromContextUncached     | 71.118 ns | 0.5539 ns | 0.8119 ns | 0.0049 |      88 B |
| GetClientHintsFromContextCached       |  6.052 ns | 0.1073 ns | 0.1538 ns |      - |         - |

## Samples

- [ASP.NET Core MVC Sample](./samples/HttpClientHints.Samples.AspNetCoreMvc/)

## Maintained

by [@BenjaminAbt](https://github.com/BenjaminAbt) and [@gfoidl](https://github.com/gfoidl)

## License

MIT License

Copyright (c) 2024-2026 MyCSharp

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
