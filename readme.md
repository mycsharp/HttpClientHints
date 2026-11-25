<div align="center">

<img src="res/httpclienthints-logo.png" alt="HttpClientHints logo: the myCSharp.de raccoon in a browser window, reading the Sec-CH-UA-Platform Client Hint with a magnifying glass" width="180" />

# MyCSharp.HttpClientHints

**Reading HTTP Client Hints with .NET**

Requests Client Hints with `Accept-CH` and reads the browser, platform, architecture, device and mobile hints in ASP.NET Core.

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

Browsers send Client Hints only to secure origins, that is over HTTPS and to `localhost`. The low-entropy hints `Sec-CH-UA`, `Sec-CH-UA-Mobile`, and `Sec-CH-UA-Platform` are part of every request. All other hints have high entropy: a browser sends them only after a response of the site has requested them with the `Accept-CH` header, so the first request to a site does not contain them. With `Critical-CH`, a browser that supports it repeats the first request with selected hints, see [Options](#options).

## Usage

Configure the hints to request and add the middleware that sets the `Accept-CH` response header:

```csharp
builder.Services.AddHttpClientHints(options =>
{
    options.Platform = true;        // Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version
    options.FullVersionList = true; // Sec-CH-UA-Full-Version-List
});
```

```csharp
app.UseHttpsRedirection();

app.UseHttpClientHints();

app.UseRouting();
```

Without options, `Accept-CH` requests only `Sec-CH-UA` and `Sec-CH-UA-Mobile`, see [Options](#options). The middleware only sets response headers. Reading the hints of a request does not depend on it.

Read the Client Hints with the extension methods for `HttpContext` and `IHeaderDictionary` in `MyCSharp.HttpClientHints.AspNetCore`:

| API | Returns | Behavior |
| --- | --- | --- |
| `httpContext.GetClientHints()` | `HttpClientHints` record | Reads the headers on the first call and returns the same instance for every further call during the request. |
| `httpContext.GetClientHintsView()` | `HttpClientHintsView` struct | Reads the current request headers on every property access. Reading one raw field line does not allocate. Reading repeated Client Hint lines joins them into a string. |

`GetClientHints()` and `GetClientHintsView()` are also available on `IHeaderDictionary`. `GetClientHints()` on headers builds a new record on every call. A separate middleware also provides the hints as an `HttpContext` feature, see [HttpContext feature](#httpcontext-feature).

```csharp
using MyCSharp.HttpClientHints;
using MyCSharp.HttpClientHints.AspNetCore;

HttpClientHints hints = httpContext.GetClientHints();
string? platform = hints.DecodedPlatform; // Windows

HttpClientHintsView view = httpContext.GetClientHintsView();
bool? isMobile = view.Mobile;
```

A property is `null` if the request does not contain the header. `HttpClientHints.Headers` references all request headers, including headers without a property.

## Options

`HttpClientHintsOptions` selects the hints that `Accept-CH` requests:

| Option | Requested hints | Entropy | Default |
| --- | --- | --- | --- |
| `UserAgent` | `Sec-CH-UA` | low | `true` |
| `Mobile` | `Sec-CH-UA-Mobile` | low | `true` |
| `Platform` | `Sec-CH-UA-Platform`, `Sec-CH-UA-Platform-Version` | low, high | `false` |
| `Architecture` | `Sec-CH-UA-Arch`, `Sec-CH-UA-Bitness`, `Sec-CH-UA-WoW64` | high | `false` |
| `Device` | `Sec-CH-UA-Model`, `Sec-CH-UA-Form-Factors` | high | `false` |
| `FullVersionList` | `Sec-CH-UA-Full-Version-List` | high | `false` |

Browsers send the low-entropy hints with every request, also if `UserAgent` or `Mobile` is `false`. The options for high-entropy hints are `false` by default, so an application requests only the hints it enables. Turning every hint off sends an empty `Accept-CH` to clear an earlier opt-in for that origin.

| Option | Effect |
| --- | --- |
| `Additional` | Further hints for `Accept-CH`, for example `Sec-CH-Prefers-Color-Scheme`. |
| `Critical` | Hints for the `Critical-CH` response header. They are also added to `Accept-CH` and `Vary`. A browser that supports `Critical-CH` and did not send one of these hints repeats the request once with the hint. |
| `Vary` | Adds all requested hints to the `Vary` response header. Enable it if responses that depend on the hints can be cached. `false` by default. |
| `Lifetime` | Obsolete. Sets the `Accept-CH-Lifetime` header, which was removed from the Client Hints specification and which browsers ignore. |

`AddHttpClientHints` reads the options once. It removes surrounding white space from the entries of `Additional` and `Critical` and ignores empty entries and duplicates; duplicates are compared ignoring case. An entry that is not a valid header name, for example `Sec CH` or two names separated by a comma, makes `AddHttpClientHints` throw an `ArgumentException`: browsers would ignore the whole `Accept-CH` header. The middleware merges missing field names into `Vary` before the response starts, including when MVC replaces the header with `[ResponseCache]`. It compares names ignoring case and preserves `Vary: *`.

```csharp
builder.Services.AddHttpClientHints(options =>
{
    options.Platform = true;
    options.Architecture = true;
    options.Device = true;
    options.FullVersionList = true;
    options.Additional = ["Sec-CH-Prefers-Color-Scheme"];
    options.Critical = ["Sec-CH-UA-Platform-Version"];
});
```

The ASP.NET Core response cache also sees the merged `Vary` when it selects a cache key, including when a Minimal API endpoint or MVC cache filter replaces the header.

The options above produce these response headers:

```http
Accept-CH: Sec-CH-UA, Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version, Sec-CH-UA-Arch, Sec-CH-UA-Bitness, Sec-CH-UA-WoW64, Sec-CH-UA-Model, Sec-CH-UA-Form-Factors, Sec-CH-UA-Mobile, Sec-CH-UA-Full-Version-List, Sec-CH-Prefers-Color-Scheme
Critical-CH: Sec-CH-UA-Platform-Version
Vary: Sec-CH-UA-Platform-Version
```

## Decoded values

Client Hints are HTTP Structured Fields ([RFC 9651](https://www.rfc-editor.org/rfc/rfc9651.html)). The properties without prefix return the header value as sent, for example `"Windows"` including the quotes. The `Decoded*` properties, `Mobile`, and `Wow64` return the decoded value:

| Request header | Property | Decoded property | Decoded type |
| --- | --- | --- | --- |
| `User-Agent` | `UserAgent` | | |
| `Sec-CH-UA` | `UA` | `DecodedUA` | `IReadOnlyList<HttpClientHintsBrand>?` |
| `Sec-CH-UA-Full-Version-List` | `FullVersionList` | `DecodedFullVersionList` | `IReadOnlyList<HttpClientHintsBrand>?` |
| `Sec-CH-UA-Platform` | `Platform` | `DecodedPlatform` | `string?` |
| `Sec-CH-UA-Platform-Version` | `PlatformVersion` | `DecodedPlatformVersion` | `string?` |
| `Sec-CH-UA-Arch` | `Architecture` | `DecodedArchitecture` | `string?` |
| `Sec-CH-UA-Bitness` | `Bitness` | `DecodedBitness` | `string?` |
| `Sec-CH-UA-Model` | `Model` | `DecodedModel` | `string?` |
| `Sec-CH-UA-Form-Factors` | `FormFactors` | `DecodedFormFactors` | `IReadOnlyList<string>?` |
| `Sec-CH-UA-Mobile` | | `Mobile` | `bool?` |
| `Sec-CH-UA-WoW64` | | `Wow64` | `bool?` |

The properties are available on `HttpClientHints` and `HttpClientHintsView`. No value is decoded before a `Decoded*` property is read, so reading the value as sent or building a snapshot costs nothing extra.

- `HttpClientHints` decodes a value on the first access and returns the same result on every further access. `httpContext.GetClientHints()` and `IHttpClientHintsFeature.Snapshot` return the same record during a request, so every value of a request is decoded at most once. The decoded values are not part of the record equality, and a record created with `with` decodes its own values.
- `HttpClientHintsView` reads the current headers, so its `Decoded*` properties decode on every access. Store the decoded value in a local variable if you need it more than once.

```csharp
HttpClientHints hints = httpContext.GetClientHints();
string? platformHeader = hints.Platform;   // "Windows", including the two quote characters
bool isWindows = hints.DecodedPlatform == "Windows";
```

`HttpClientHintsInterpreter` provides the decoders, also for other headers:

| Method | Structured Field type | Example |
| --- | --- | --- |
| `DecodeStructuredString` | String | `"Windows"` → `Windows` |
| `DecodeStructuredBoolean` | Boolean | `?1` → `true` |
| `DecodeStructuredStringList` | List of strings | `"Desktop", "XR"` → `Desktop`, `XR` |
| `DecodeBrandList` | List of strings with the parameter `v` | `"Chromium";v="142"` → `Chromium`, `142` |
| `IsMobile` | Boolean | Same as `DecodeStructuredBoolean` |

- An absent header yields `null`. An empty header yields an empty list for the list decoders.
- Leading and trailing spaces and tabs are ignored.
- Repeated `Sec-CH-*` field lines are joined with commas before decoding. Empty lines retain their position. For example, two `Sec-CH-UA-Mobile` lines containing `?1` and `?0` yield `null`. The legacy `User-Agent` property reads the first line.
- A malformed value yields `null`, for example an unterminated string, a list with a trailing comma, or a list member that is not a string.
- `DecodeStructuredString` returns an unquoted value as sent, and a quoted empty value such as `""` as `string.Empty`.
- A string, a boolean, or a member of a string list with parameters yields `null`, for example `"Windows";a=1`. Brand lists ignore all parameters except `v`.

### Brand lists

`DecodedUA` and `DecodedFullVersionList` return the brands of `Sec-CH-UA` and `Sec-CH-UA-Full-Version-List` in the order of the header. `HttpClientHintsBrand` has these properties:

| Property | Value |
| --- | --- |
| `Brand` | The brand, for example `Google Chrome` |
| `Version` | The value of the parameter `v`: the major version in `Sec-CH-UA`, the full version in `Sec-CH-UA-Full-Version-List`. `null` if the member has no `v` parameter with a string value. |
| `IsGrease` | `true` for the brand with a changing name that Chromium-based browsers add, so that servers do not depend on a fixed list of brands |

`IsGrease` recognizes brands that consist of the letters `NotABrand` in this order and any of the characters ` ()-./:;=?_"\`. This covers the forms of current Chromium-based browsers, such as `Not_A Brand` or `Not)A;Brand`, and of older versions, such as ` Not A;Brand` or `"Not\A;Brand`.

```csharp
// Sec-CH-UA-Full-Version-List: "Chromium";v="142.0.7444.60", "Google Chrome";v="142.0.7444.60", "Not_A Brand";v="99.0.0.0"
foreach (HttpClientHintsBrand brand in hints.DecodedFullVersionList ?? [])
{
    if (!brand.IsGrease)
    {
        Console.WriteLine($"{brand.Brand} {brand.Version}"); // Chromium 142.0.7444.60, Google Chrome 142.0.7444.60
    }
}
```

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

The middleware registers the feature for every request, whether or not `Accept-CH` is configured, because browsers send the low-entropy hints without a request. The registration replaces an `IHttpClientHintsFeature` registered earlier in the pipeline. Register your own implementation after `app.UseHttpClientHintsFeature()`. The feature references the `HttpContext`, so do not keep it beyond the request.

Without `UseHttpClientHintsFeature()`, requests have no additional cost, and `Features.Get<IHttpClientHintsFeature>()` returns `null`. The extension methods work in both cases. With it, the feature is created for every request, even if no handler reads it. In the [benchmark](#benchmark), which reuses one `DefaultHttpContext`, the feature middleware takes about 13 ns and allocates 24 B per request. Together with `UseHttpClientHints()`, the pipeline takes about 19 ns more than `UseHttpClientHints()` alone, because `HttpContext` looks up its cached features again after the registration. The cost depends on the server: in a test with Kestrel on .NET 11, the registration allocated 24 B or 112 B per request, depending on whether Kestrel reused its request object.

## Native AOT and trimming

Both packages are annotated with `IsAotCompatible`, so the trimming and AOT analyzers check them on every build. The Native AOT smoke test (`tests/HttpClientHints.NativeAot.Smoke`) is published and run with .NET 8, 9, 10, and 11 in every CI build of `main` and of pull requests:

```shell
dotnet publish tests/HttpClientHints.NativeAot.Smoke -c Release -f net11.0 -r linux-x64
```

The app roots both library assemblies, so the trimmer and the AOT compiler analyze all of their code, and the publish fails on any trimming or AOT warning. At runtime it checks:

- decoding of strings, booleans, string lists, and brand lists, including the detection of GREASE brands
- the `Accept-CH`, `Critical-CH`, and `Vary` response headers of `UseHttpClientHints()`
- `GetClientHints()` and `GetClientHintsView()` on `HttpContext` and `IHeaderDictionary`
- `IHttpClientHintsFeature` registered by `UseHttpClientHintsFeature()`

Publishing with Native AOT requires the [platform toolchain](https://learn.microsoft.com/dotnet/core/deploying/native-aot/#prerequisites): `clang` and `zlib1g-dev` on Linux, the Visual Studio workload "Desktop development with C++" on Windows.

## Build and test

Use the .NET 11 SDK to build the solution. The .NET 11 build uses `Microsoft.Extensions.Primitives` from the shared runtime. Install the .NET and ASP.NET Core runtimes 8, 9, and 10 to run the full test suite.

```shell
dotnet build -c Release
dotnet test -c Release --no-build
```

Line and branch coverage of both packages must stay at 100 %. CI collects coverage on every build and fails below this threshold. `./eng/Assert-CodeCoverage.ps1` reads the Cobertura report of every test project and target framework, because the report that `dotnet test` merges keeps the hits of only one test project for a package. A line of a class counts as covered if any report covers it; for branches, the highest number of covered branches of any report counts. Run the same check locally with `just test-cov`, or call the script after a test run with `--coverage --coverage-output-format cobertura` into an empty results directory: the script also reads reports of earlier runs.

CI also checks the formatting with `dotnet format --verify-no-changes`. Run the same check locally with `just format-check`.

`dotnet pack -c Release` uses Nerdbank.GitVersioning to derive package and assembly versions from `version.json` and Git history. It validates the packages against the version in `PackageValidationBaselineVersion` in `Directory.Build.props` and fails on API breaking changes. Intended breaking changes are listed in the `CompatibilitySuppressions.xml` file of the package; generate it with `dotnet pack -p:ApiCompatGenerateSuppressionFile=true`. CI packs in every build, also for pull requests.

Changes reach `main` only through pull requests. A pull request can be merged when the checks `Build and Test` and `Native AOT` for .NET 8, 9, 10, and 11 have passed.

## Release

Every push to `main` builds and tests the solution and creates a draft GitHub release with the NuGet packages. Publishing that release runs `.github/workflows/release-publish.yml`, which pushes the packages to NuGet.org with [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing). The workflow exchanges a GitHub OIDC token for a NuGet API key that is valid for one hour, so the repository stores no NuGet API key.

The draft release groups the merged pull requests by label. `.github/workflows/pr-labeler.yml` updates the managed category when a pull request title changes and preserves area labels. The category comes from the Conventional Commits type in the title, for example `feat:` for features, `fix:` for bug fixes and `chore:` or `ci:` for maintenance. A title with `!`, such as `feat!:`, marks a breaking change, which the release notes list first.

To retry a NuGet publication, run the workflow manually with the tag of a published GitHub release.

```shell
gh workflow run release-publish.yml -f tag=v3.3.5
```

The workflow requires a Trusted Publishing policy on NuGet.org for the `MyCSharp` organization with the repository `mycsharp/HttpClientHints` and the workflow file `release-publish.yml`, and the repository secret `NUGET_USER` with the NuGet.org profile name of the member who created the policy.

After a release, set `PackageValidationBaselineVersion` to the released version and delete the `CompatibilitySuppressions.xml` files.

## Benchmark

Run the benchmarks with `dotnet run -c Release --project perf/HttpClientHints.Benchmarks --framework net11.0 -- --filter '*' --iterationCount 15 --iterationTime 200 --launchCount 2 --warmupCount 6`. The tables below were measured before the response caching changes.

```
BenchmarkDotNet v0.16.0-preview.2, Windows 10 (10.0.19045.7846/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 61,64 GB Total, 30,41 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  Job-ZRYWOI : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

IterationCount=15  IterationTime=200ms  LaunchCount=2
WarmupCount=6
```

### Reading Client Hints

The benchmark uses one field line per header. For these inputs, reading raw values from the view does not allocate, and building a snapshot allocates only the record. A `Decoded*` property of the view decodes on every access and can allocate strings and arrays. `ReadDecodedBrandLists` decodes `Sec-CH-UA` and `Sec-CH-UA-Full-Version-List` with three brands each through the view. `ReadDecodedBrandListsFromCachedSnapshot` reads the values cached on first access.

| Method                                  | Mean       | Error     | StdDev    | Median     | Gen0   | Allocated |
|---------------------------------------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
| ReadViewProperties                      |  21.469 ns | 0.8572 ns | 1.2017 ns |  20.664 ns |      - |         - |
| ReadDecodedViewProperties               |  39.054 ns | 0.3240 ns | 0.4647 ns |  38.933 ns | 0.0043 |      72 B |
| ReadDecodedBrandLists                   | 157.879 ns | 2.1842 ns | 3.2015 ns | 157.766 ns | 0.0387 |     648 B |
| ReadDecodedBrandListsFromCachedSnapshot |  10.081 ns | 0.0575 ns | 0.0805 ns |  10.045 ns |      - |         - |
| GetClientHintsFromHeaders               |  82.245 ns | 0.4326 ns | 0.5922 ns |  82.206 ns | 0.0070 |     120 B |
| GetClientHintsFromHeadersWithoutHints   |  68.717 ns | 0.3436 ns | 0.5037 ns |  68.697 ns | 0.0072 |     120 B |
| GetClientHintsFromContextUncached       |  95.799 ns | 0.5178 ns | 0.7426 ns |  95.896 ns | 0.0067 |     120 B |
| GetClientHintsFromContextCached         |   5.823 ns | 0.0301 ns | 0.0432 ns |   5.818 ns |      - |         - |

### Middleware

The middleware benchmarks run pipelines built with `ApplicationBuilder`. They reuse one `DefaultHttpContext`, and the last handler completes synchronously. `UseHttpClientHints()` is configured with `Platform = true`, sets `Accept-CH`, and does not allocate in this setup. If a later middleware completes asynchronously, it allocates its async state machine. `UseHttpClientHintsWithCriticalAndVary` also sets `Critical-CH` and appends to `Vary`; it removes the `Vary` header before every call. `UseHttpClientHintsFeature()` allocates the 24-byte feature and returns the task of the next middleware directly. Registering the feature changes the revision of the feature collection, so `HttpContext` looks up the features it has cached again. Other servers can allocate more for the registration, see [HttpContext feature](#httpcontext-feature).

| Method                                | Mean     | Error    | StdDev   | Median   | Gen0   | Allocated |
|-------------------------------------- |---------:|---------:|---------:|---------:|-------:|----------:|
| UseHttpClientHints                    | 12.49 ns | 0.053 ns | 0.078 ns | 12.52 ns |      - |         - |
| UseHttpClientHintsWithCriticalAndVary | 36.32 ns | 0.225 ns | 0.323 ns | 36.33 ns |      - |         - |
| UseHttpClientHintsFeature             | 12.97 ns | 0.642 ns | 0.942 ns | 12.25 ns | 0.0014 |      24 B |
| UseHttpClientHintsAndFeature          | 31.65 ns | 0.506 ns | 0.658 ns | 31.59 ns | 0.0014 |      24 B |
| UseHttpClientHintsFeatureAndReadView  | 45.12 ns | 1.318 ns | 1.890 ns | 45.20 ns | 0.0012 |      24 B |

## Samples

- [ASP.NET Core MVC Sample](./samples/HttpClientHints.Samples.AspNetCoreMvc/): enables all options for high-entropy hints, marks two hints as critical, and shows every hint as sent and decoded, and the brands.

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
