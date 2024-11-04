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

## Samples

- [ASP.NET Core MVC Sample](./samples/MyCSharp.HttpClientHints.Samples.AspNetCoreMvc/)

## Maintained

by [@BenjaminAbt](https://github.com/BenjaminAbt) and [@gfoidl](https://github.com/gfoidl)

## License

MIT License

Copyright (c) 2024 MyCSharp

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
