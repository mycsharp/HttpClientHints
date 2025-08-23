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

## Benchmark

```shell
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.6216/22H2/2022Update)
AMD Ryzen 9 9950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.100-preview.7.25380.108
  [Host]   : .NET 10.0.0 (10.0.25.38108), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  .NET 8.0 : .NET 8.0.19 (8.0.1925.36514), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  .NET 9.0 : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


| Method                                                 | Job      | Runtime  | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------------------------------------------- |--------- |--------- |----------:|---------:|---------:|-------:|----------:|
| 'View: read few properties (no alloc)'                 | .NET 8.0 | .NET 8.0 |  29.42 ns | 0.205 ns | 0.181 ns |      - |         - |
| 'View: BuildSnapshot (alloc 1)'                        | .NET 8.0 | .NET 8.0 |  67.37 ns | 0.624 ns | 0.584 ns | 0.0052 |      88 B |
| 'Extension: GetClientHints(headers) (alloc 1)'         | .NET 8.0 | .NET 8.0 |  66.60 ns | 0.552 ns | 0.461 ns | 0.0052 |      88 B |
| 'Extension: GetClientHints(context) (alloc 1, cached)' | .NET 8.0 | .NET 8.0 | 108.18 ns | 1.039 ns | 0.972 ns | 0.0052 |      88 B |
| 'View: read few properties (no alloc)'                 | .NET 9.0 | .NET 9.0 |  31.48 ns | 0.171 ns | 0.152 ns |      - |         - |
| 'View: BuildSnapshot (alloc 1)'                        | .NET 9.0 | .NET 9.0 |  66.71 ns | 0.606 ns | 0.473 ns |      - |         - |
| 'Extension: GetClientHints(headers) (alloc 1)'         | .NET 9.0 | .NET 9.0 |  65.92 ns | 0.270 ns | 0.239 ns |      - |         - |
| 'Extension: GetClientHints(context) (alloc 1, cached)' | .NET 9.0 | .NET 9.0 | 116.13 ns | 1.304 ns | 1.219 ns | 0.0052 |      88 B |
```

## Samples

- [ASP.NET Core MVC Sample](./samples/MyCSharp.HttpClientHints.Samples.AspNetCoreMvc/)

## Maintained

by [@BenjaminAbt](https://github.com/BenjaminAbt) and [@gfoidl](https://github.com/gfoidl)

## License

MIT License

Copyright (c) 2024-2025 MyCSharp

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
