# MyCSharp.HttpClientHints

Reading HTTP Client Hints with .NET

## NuGet

| NuGet |
|-|
| [![MyCSharp.HttpClientHints](https://img.shields.io/nuget/v/MyCSharp.HttpClientHints.svg?logo=nuget&label=MyCSharp.HttpClientHints)](https://www.nuget.org/packages/MyCSharp.HttpClientHints) |
| [![MyCSharp.HttpClientHints.AspNetCore](https://img.shields.io/nuget/v/MyCSharp.HttpClientHints.AspNetCore.svg?logo=nuget&label=MyCSharp.HttpClientHints.AspNetCore)](https://www.nuget.org/packages/MyCSharp.HttpClientHints.AspNetCore) | `dotnet add package MyCSharp.HttpClientHints.AspNetCore` |


## Client Hints

HTTP client hints are intended to completely replace the user agents (see [mycsharp/HttpUserAgentParser](https://github.com/mycsharp/HttpUserAgentParser)) of the browser in the long term and pay more attention to data protection. We are currently in a phase in which client hints are generally regarded as expiremental and therefore do not represent a complete alternative.

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

Since potentially all client hints are optional, all properties are potentially nullable, which means that the client has not provided this data.

## Upgrading to 4.0

**Breaking change.** Version 4.0 changes what four properties return when Client Hints are read through `GetClientHints()` or `GetClientHintsView()`. They now decode the HTTP Structured Field string. To keep using the original value as returned in 3.x, switch to the corresponding `Raw*` property.

| Decoded property | Raw property | Request header |
| --- | --- | --- |
| `Platform` | `RawPlatform` | `Sec-CH-UA-Platform` |
| `PlatformVersion` | `RawPlatformVersion` | `Sec-CH-UA-Platform-Version` |
| `Architecture` | `RawArchitecture` | `Sec-CH-UA-Arch` |
| `Model` | `RawModel` | `Sec-CH-UA-Model` |

For a request with `Sec-CH-UA-Platform: "Windows"`, both forms are available.

```csharp
HttpClientHints hints = httpContext.GetClientHints();
bool isWindows = hints.Platform == "Windows";
string? originalPlatformHeader = hints.RawPlatform; // Contains the two quote characters.
```

An absent header yields `null` for both properties. A quoted empty value such as `Sec-CH-UA-Model: ""` decodes to `string.Empty`. Unquoted values pass through as supplied. A malformed quoted value yields `null` in the decoded property and remains available in the raw property. Parameters are not supported: `Sec-CH-UA-Platform: "Windows";a=1` also yields `null` in the decoded property. [RFC 9651 § 4.2.5](https://www.rfc-editor.org/rfc/rfc9651.html#section-4.2.5)

`UA` and `FullVersionList` expose serialized brand lists. The `Headers` collection provides access to all original header values. If you construct an `HttpClientHints` record directly, supply the decoded values to its constructor and set the raw properties in an object initializer when you have the original values. A `with` expression that changes a decoded property, such as `hints with { Platform = "Linux" }`, leaves the matching `Raw*` property unchanged.

## Customization

The `Additional` property can be used to define your own headers that are to be requested.

```csharp
// Add Http Client Hint Options
builder.Services.AddHttpClientHints(options =>
  {
      options.UserAgent = true;
      options.Platform = true;
      options.Architecture = true;
      options.Device = true;
      options.Mobile = true;
      options.Additional = ["Sec-CH-UA-Bitness"];
      options.Lifetime = TimeSpan.FromDays(31);
  });
```

The `HttpClientHints` object has a reference to all headers received, making it very easy to read them out. Optionally, you can also inherit from the `HttpClientHints` class itself to simplify access.

```csharp
HttpClientHints clientHints = HttpClientHintsHttpContextExtensions.GetClientHints(httpContext);
clientHints.Headers.TryGetValue("Sec-CH-UA-Bitness", out StringValues bitness);
```

## Build and test

Use the .NET 11 SDK to build the solution. Both NuGet packages target .NET 8, 9, 10, and 11. The .NET 11 build uses `Microsoft.Extensions.Primitives` from the shared runtime. Install the corresponding runtimes to run the full test suite.

```shell
dotnet build -c Release
dotnet test -c Release --no-build
```

Line and branch coverage must stay at or above 90 %. CI collects coverage on every build and fails below this threshold. Run the same check locally with `just test-cov`, or call `./eng/Assert-CodeCoverage.ps1` after a test run with `--coverage --coverage-output-format cobertura`.

## Benchmark

Reading `Raw*` properties from the view does not allocate. Reading a decoded property allocates the decoded string, and a snapshot stores both forms. Run the benchmarks with `dotnet run -c Release --project perf/HttpClientHints.Benchmarks --framework net11.0 -- --filter '*' --iterationCount 15 --iterationTime 200 --launchCount 2 --warmupCount 6`.

```
BenchmarkDotNet v0.16.0-preview.2, Windows 10 (10.0.19045.7846/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 61,64 GB Total, 21,42 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  Job-ZRYWOI : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

IterationCount=15  IterationTime=200ms  LaunchCount=2
WarmupCount=6
```

| Method                                | Mean       | Error     | StdDev    | Median     | Gen0   | Allocated |
|-------------------------------------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
| ReadViewProperties                    |  38.403 ns | 1.2810 ns | 1.8777 ns |  37.723 ns | 0.0042 |      72 B |
| ReadRawViewProperties                 |  19.796 ns | 0.5348 ns | 0.7671 ns |  19.829 ns |      - |         - |
| GetClientHintsFromHeaders             |  96.731 ns | 2.1783 ns | 3.1241 ns |  96.137 ns | 0.0134 |     232 B |
| GetClientHintsFromHeadersWithoutHints |  61.318 ns | 1.8116 ns | 2.6554 ns |  59.624 ns | 0.0072 |     120 B |
| GetClientHintsFromContextUncached     | 108.202 ns | 1.5130 ns | 2.0709 ns | 108.719 ns | 0.0136 |     232 B |
| GetClientHintsFromContextCached       |   6.524 ns | 0.0837 ns | 0.1227 ns |   6.510 ns |      - |         - |

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
