# Instructions for MyCSharp.HttpClientHints

## Projects

- `src/HttpClientHints`: package `MyCSharp.HttpClientHints` with the `HttpClientHints` record and `HttpClientHintsInterpreter`.
- `src/HttpClientHints.AspNetCore`: package `MyCSharp.HttpClientHints.AspNetCore` with the `Accept-CH` middleware, the `HttpContext` and `IHeaderDictionary` extensions and `IHttpClientHintsFeature`.
- `tests/*.UnitTests`: xUnit v3 tests on Microsoft.Testing.Platform. FluentAssertions is blocked; use `Assert`.
- `tests/HttpClientHints.NativeAot.Smoke`: Native AOT smoke test that roots both packages.
- `perf/HttpClientHints.Benchmarks`: BenchmarkDotNet benchmarks.

## Build and test

- The packages target .NET 8, 9, 10 and 11. Build with the .NET 11 SDK.
- `dotnet build -c Release` must not produce warnings: `TreatWarningsAsErrors` is enabled, with Roslynator, Meziantou and the trimming and AOT analyzers.
- Line and branch coverage of both packages must stay at 100 %: `just test-cov`.
- CI checks the formatting: `just format-check`.
- Native AOT: `dotnet publish tests/HttpClientHints.NativeAot.Smoke -c Release -f net11.0 -r <rid>` must publish without warnings, and the executable must exit with 0.

## Code

- Every C# file starts with `// Copyright © https://myCSharp.de - all rights reserved`.
- Do not use reflection or dynamic code generation; both packages are `IsAotCompatible`.
- Reading Client Hints is on the request path: avoid allocations, measure changes with the benchmarks.
- Public API is part of the NuGet packages: removing or changing members is a breaking change and needs a major version.

## Documentation

- `readme.md` and the package readmes in `src/*/readme.md` describe the current behavior only. Do not add version history such as "Since 3.2".
- Link NuGet packages to `https://nubrowse.com/packages/<PackageId>`.

## Commits

Use Conventional Commits, for example `feat:`, `fix:`, `perf:`, `docs:`, `test:`, `ci:` and `chore:`.
