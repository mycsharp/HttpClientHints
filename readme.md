<div align="center">

<img src="res/httpclienthints-logo.png" alt="HttpClientHints logo: the myCSharp.de raccoon in a browser window, reading the Sec-CH-UA-Platform Client Hint with a magnifying glass" width="180" />

# MyCSharp.HttpClientHints

**Reading HTTP Client Hints with .NET**

Requests Client Hints with `Accept-CH` and reads the browser, platform, architecture, model and mobile hints in ASP.NET Core.

[![Build](https://img.shields.io/github/actions/workflow/status/mycsharp/HttpClientHints/main-build.yml?branch=main&logo=githubactions&logoColor=white&label=build)](https://github.com/mycsharp/HttpClientHints/actions/workflows/main-build.yml)
[![Coverage: 100% line and branch coverage, enforced in CI](https://img.shields.io/badge/coverage-100%25-brightgreen)](#build-and-test)
[![NuGet](https://img.shields.io/nuget/v/MyCSharp.HttpClientHints?logo=nuget&label=nuget&color=004880)](https://nubrowse.com/packages/MyCSharp.HttpClientHints)
[![NuGet downloads](https://img.shields.io/nuget/dt/MyCSharp.HttpClientHints?logo=nuget&label=downloads&color=004880)](https://nubrowse.com/packages/MyCSharp.HttpClientHints)
[![License: MIT](https://img.shields.io/github/license/mycsharp/HttpClientHints?color=blue)](LICENSE)

[![.NET 8 | 9 | 10 | 11](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010%20%7C%2011-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Native AOT compatible](https://img.shields.io/badge/Native%20AOT-compatible-512BD4?logo=dotnet&logoColor=white)](#native-aot-and-trimming)
[![Trimming compatible](https://img.shields.io/badge/trimming-compatible-512BD4?logo=dotnet&logoColor=white)](#native-aot-and-trimming)
[![Strong-named assemblies](https://img.shields.io/badge/strong--named-yes-blue)](https://learn.microsoft.com/dotnet/standard/assembly/strong-named)
[![SourceLink enabled](https://img.shields.io/badge/SourceLink-enabled-blue)](https://learn.microsoft.com/dotnet/standard/library-guidance/sourcelink)

[![GitHub stars](https://img.shields.io/github/stars/mycsharp/HttpClientHints?logo=github)](https://github.com/mycsharp/HttpClientHints/stargazers)
[![Last commit](https://img.shields.io/github/last-commit/mycsharp/HttpClientHints/main?logo=github)](https://github.com/mycsharp/HttpClientHints/commits/main)
[![Open issues](https://img.shields.io/github/issues/mycsharp/HttpClientHints?logo=github)](https://github.com/mycsharp/HttpClientHints/issues)

</div>

## NuGet

| Package | Version | Downloads | Install |
| --- | --- | --- | --- |
| **[MyCSharp.HttpClientHints](https://nubrowse.com/packages/MyCSharp.HttpClientHints)**<br><sub>`HttpClientHints` record and header decoding</sub> | [![NuGet](https://img.shields.io/nuget/v/MyCSharp.HttpClientHints?logo=nuget&label=nuget&color=004880)](https://nubrowse.com/packages/MyCSharp.HttpClientHints) | [![Downloads](https://img.shields.io/nuget/dt/MyCSharp.HttpClientHints?label=downloads&color=004880)](https://nubrowse.com/packages/MyCSharp.HttpClientHints) | `dotnet add package MyCSharp.HttpClientHints` |
| **[MyCSharp.HttpClientHints.AspNetCore](https://nubrowse.com/packages/MyCSharp.HttpClientHints.AspNetCore)**<br><sub>`Accept-CH` middleware and `HttpContext` integration</sub> | [![NuGet](https://img.shields.io/nuget/v/MyCSharp.HttpClientHints.AspNetCore?logo=nuget&label=nuget&color=004880)](https://nubrowse.com/packages/MyCSharp.HttpClientHints.AspNetCore) | [![Downloads](https://img.shields.io/nuget/dt/MyCSharp.HttpClientHints.AspNetCore?label=downloads&color=004880)](https://nubrowse.com/packages/MyCSharp.HttpClientHints.AspNetCore) | `dotnet add package MyCSharp.HttpClientHints.AspNetCore` |

The NuGet packages target .NET 8, 9, 10, and 11. Building the solution requires the .NET 11 SDK. ASP.NET Core applications only need `MyCSharp.HttpClientHints.AspNetCore`, which references the core package.

## Client Hints

User-Agent Client Hints are intended to replace the `User-Agent` header (see [mycsharp/HttpUserAgentParser](https://github.com/mycsharp/HttpUserAgentParser)) in the long term and expose less information by default. Not every browser sends them yet, so they are not a complete alternative to the `User-Agent` header today.

Unlike the `User-Agent` header, browsers send most Client Hints only after the server requests them with the `Accept-CH` response header, and only over HTTPS. A few hints, such as `Sec-CH-UA`, `Sec-CH-UA-Mobile`, and `Sec-CH-UA-Platform`, are sent with every request.

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

The client hints are not read by a separate dependency injection registration, but as an extension method to the header collection or to the HttpContext object. Optionally, a separate middleware also provides them as an `HttpContext` feature, see [HttpContext feature](#httpcontext-feature).

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

## HttpContext feature

A separate middleware registers `IHttpClientHintsFeature` in `HttpContext.Features`. Add it only if you want to read the Client Hints through the feature:

```csharp
app.UseHttpClientHints();

// Optional: provide IHttpClientHintsFeature in HttpContext.Features
app.UseHttpClientHintsFeature();
```

`UseHttpClientHintsFeature()` does not require `AddHttpClientHints()` or `UseHttpClientHints()`. Every middleware and handler after it can read the Client Hints from the feature:

```csharp
IHttpClientHintsFeature? feature = httpContext.Features.Get<IHttpClientHintsFeature>();
bool? isMobile = feature?.View.Mobile;
```

| Property | Same as |
| --- | --- |
| `View` | `httpContext.GetClientHintsView()` |
| `Snapshot` | `httpContext.GetClientHints()`, including the cached instance |

`Snapshot` keeps the values read on the first access. `Snapshot.Headers` references the request headers, so it shows later changes to them.

The middleware registers the feature for every request, whether or not `Accept-CH` is configured, because browsers send some Client Hints such as `Sec-CH-UA-Mobile` without a request. The registration replaces an `IHttpClientHintsFeature` registered earlier in the pipeline. Register your own implementation after `app.UseHttpClientHintsFeature()`. The feature references the `HttpContext`, so do not keep it beyond the request.

Without `UseHttpClientHintsFeature()`, requests have no additional cost, and `Features.Get<IHttpClientHintsFeature>()` returns `null`. The extension methods work in both cases. With it, the feature is created for every request, even if no handler reads it. In the [benchmark](#benchmark), which reuses one `DefaultHttpContext`, the feature middleware takes about 12 ns and allocates 24 B per request. Together with `UseHttpClientHints()`, the pipeline takes about 19 ns more than `UseHttpClientHints()` alone, because `HttpContext` looks up its cached features again after the registration. The cost depends on the server: in a test with Kestrel on .NET 11, the registration allocated 24 B or 112 B per request, depending on whether Kestrel reused its request object.

## Decoded values

Browsers send `Sec-CH-UA-Platform`, `Sec-CH-UA-Platform-Version`, `Sec-CH-UA-Arch`, and `Sec-CH-UA-Model` as quoted strings, for example `Sec-CH-UA-Platform: "Windows"`. `Platform`, `PlatformVersion`, `Architecture`, and `Model` return these header values as sent, including the quotes. The matching `Decoded*` properties return the value without quotes and escapes.

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

## Native AOT and trimming

Both packages are annotated with `IsAotCompatible`, so the trimming and AOT analyzers check them on every build. The Native AOT smoke test (`tests/HttpClientHints.NativeAot.Smoke`) is published and run with .NET 8, 9, 10, and 11 in every CI build of `main` and of pull requests:

```shell
dotnet publish tests/HttpClientHints.NativeAot.Smoke -c Release -f net11.0 -r linux-x64
```

The app roots both library assemblies, so the trimmer and the AOT compiler analyze all of their code, and the publish fails on any trimming or AOT warning. At runtime it checks:

- decoding of quoted header values and of `Sec-CH-UA-Mobile`
- the `Accept-CH` and `Accept-CH-Lifetime` response headers of `UseHttpClientHints()`
- `GetClientHints()` and `GetClientHintsView()` on `HttpContext` and `IHeaderDictionary`
- `IHttpClientHintsFeature` registered by `UseHttpClientHintsFeature()`

Publishing with Native AOT requires the [platform toolchain](https://learn.microsoft.com/dotnet/core/deploying/native-aot/#prerequisites): `clang` and `zlib1g-dev` on Linux, the Visual Studio workload "Desktop development with C++" on Windows.

## Build and test

Use the .NET 11 SDK to build the solution. The .NET 11 build uses `Microsoft.Extensions.Primitives` from the shared runtime. Install the corresponding runtimes to run the full test suite.

```shell
dotnet build -c Release
dotnet test -c Release --no-build
```

Line and branch coverage of both packages must stay at 100 %. CI collects coverage on every build and fails below this threshold. Run the same check locally with `just test-cov`, or call `./eng/Assert-CodeCoverage.ps1` after a test run with `--coverage --coverage-output-format cobertura`.

## Release

Every push to `main` builds and tests the solution and creates a draft GitHub release with the NuGet packages. Publishing that release runs `.github/workflows/release-publish.yml`, which pushes the packages to NuGet.org with [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing). The workflow exchanges a GitHub OIDC token for a NuGet API key that is valid for one hour, so the repository stores no NuGet API key.

To publish the packages of an existing release again, run the workflow manually with the release tag:

```shell
gh workflow run release-publish.yml -f tag=v3.2.3
```

The workflow requires a Trusted Publishing policy on NuGet.org for the `MyCSharp` organization with the repository `mycsharp/HttpClientHints` and the workflow file `release-publish.yml`, and the repository secret `NUGET_USER` with the NuGet.org profile name of the member who created the policy.

## Benchmark

Run the benchmarks with `dotnet run -c Release --project perf/HttpClientHints.Benchmarks --framework net11.0 -- --filter '*' --iterationCount 15 --iterationTime 200 --launchCount 2 --warmupCount 6`. Both tables below come from one run of this command.

```
BenchmarkDotNet v0.16.0-preview.2, Windows 10 (10.0.19045.7846/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 61,64 GB Total, 35,59 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  Job-ZRYWOI : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

IterationCount=15  IterationTime=200ms  LaunchCount=2
WarmupCount=6
```

### Reading Client Hints

Reading header values from the view does not allocate, and a snapshot allocates only the record. A `Decoded*` property allocates the decoded string of a quoted value on every access.

| Method                                | Mean      | Error     | StdDev    | Median    | Gen0   | Allocated |
|-------------------------------------- |----------:|----------:|----------:|----------:|-------:|----------:|
| ReadViewProperties                    | 18.471 ns | 0.2174 ns | 0.3118 ns | 18.466 ns |      - |         - |
| ReadDecodedViewProperties             | 36.670 ns | 0.7575 ns | 1.0113 ns | 37.392 ns | 0.0043 |      72 B |
| GetClientHintsFromHeaders             | 56.993 ns | 0.5361 ns | 0.7688 ns | 56.722 ns | 0.0051 |      88 B |
| GetClientHintsFromHeadersWithoutHints | 48.849 ns | 0.4774 ns | 0.7146 ns | 48.529 ns | 0.0052 |      88 B |
| GetClientHintsFromContextUncached     | 71.006 ns | 0.8836 ns | 1.2386 ns | 70.255 ns | 0.0049 |      88 B |
| GetClientHintsFromContextCached       |  6.196 ns | 0.2700 ns | 0.4041 ns |  6.188 ns |      - |         - |

### Middleware

The middleware benchmarks run pipelines built with `ApplicationBuilder`. They reuse one `DefaultHttpContext`, and the last handler completes synchronously. `UseHttpClientHints()` sets `Accept-CH` and `Accept-CH-Lifetime` and does not allocate in this setup. If a later middleware completes asynchronously, it allocates its async state machine. `UseHttpClientHintsFeature()` allocates the 24-byte feature and returns the task of the next middleware directly. Registering the feature changes the revision of the feature collection, so `HttpContext` looks up the features it has cached again. Other servers can allocate more for the registration, see [HttpContext feature](#httpcontext-feature).

| Method                               | Mean     | Error    | StdDev   | Gen0   | Allocated |
|------------------------------------- |---------:|---------:|---------:|-------:|----------:|
| UseHttpClientHints                   | 17.60 ns | 0.143 ns | 0.214 ns |      - |         - |
| UseHttpClientHintsFeature            | 12.25 ns | 0.312 ns | 0.458 ns | 0.0014 |      24 B |
| UseHttpClientHintsAndFeature         | 36.76 ns | 0.384 ns | 0.526 ns | 0.0013 |      24 B |
| UseHttpClientHintsFeatureAndReadView | 41.71 ns | 0.400 ns | 0.574 ns | 0.0014 |      24 B |

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
