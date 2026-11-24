// Copyright © https://myCSharp.de - all rights reserved

// Native AOT smoke test: exercises the public API of both packages in a trimmed, natively compiled app.
// Exit code 0 means success; every failed check is printed and makes the exit code non-zero.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpClientHints;
using MyCSharp.HttpClientHints.AspNetCore;

const string FullVersionList = "\"Chromium\";v=\"142.0.7444.60\", \"Google Chrome\";v=\"142.0.7444.60\", \"Not_A Brand\";v=\"99.0.0.0\"";

int failures = 0;

void Check(bool condition, string description)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} {description}");
    if (!condition)
    {
        failures++;
    }
}

// Core package
Check(string.Equals(HttpClientHintsInterpreter.DecodeStructuredString("\"Windows\""), "Windows", StringComparison.Ordinal), "decode structured string");
Check(string.Equals(HttpClientHintsInterpreter.DecodeStructuredString("\"a\\\"b\""), "a\"b", StringComparison.Ordinal), "decode escapes");
Check(HttpClientHintsInterpreter.DecodeStructuredString("\"Windows\";a=1") is null, "reject parameters");
Check(HttpClientHintsInterpreter.IsMobile("?1") is true && HttpClientHintsInterpreter.IsMobile("?0") is false, "interpret Sec-CH-UA-Mobile");

HttpClientHints record = new(UserAgent: null, Platform: "\"macOS\"", PlatformVersion: "\"15.5.0\"", Architecture: "\"arm\"", Model: null,
    FullVersionList: null, UA: null, Mobile: false, Headers: new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(StringComparer.OrdinalIgnoreCase));
Check(record is { DecodedPlatform: "macOS", DecodedPlatformVersion: "15.5.0", DecodedArchitecture: "arm", DecodedModel: null }, "decoded properties of the record");
Check(record.Equals(record with { }) && !record.Equals(record with { Platform = "\"Linux\"" }), "record equality");

// ASP.NET Core package: options, Accept-CH middleware and feature middleware in one pipeline
ServiceCollection services = new();
services.AddHttpClientHints(static options =>
{
    options.Additional = ["Sec-CH-UA-Full-Version-List"];
    options.Lifetime = TimeSpan.FromDays(31);
});

using ServiceProvider provider = services.BuildServiceProvider();

HttpClientHints? snapshotInHandler = null;
ApplicationBuilder app = new(provider);
app.UseHttpClientHints();
app.UseHttpClientHintsFeature();
app.Run(context =>
{
    snapshotInHandler = context.GetClientHints();
    return Task.CompletedTask;
});
RequestDelegate pipeline = app.Build();

DefaultHttpContext httpContext = new();
IHeaderDictionary requestHeaders = httpContext.Request.Headers;
requestHeaders.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36";
requestHeaders["Sec-CH-UA"] = "\"Chromium\";v=\"142\", \"Google Chrome\";v=\"142\", \"Not_A Brand\";v=\"99\"";
requestHeaders["Sec-CH-UA-Platform"] = "\"Windows\"";
requestHeaders["Sec-CH-UA-Platform-Version"] = "\"19.0.0\"";
requestHeaders["Sec-CH-UA-Arch"] = "\"x86\"";
requestHeaders["Sec-CH-UA-Bitness"] = "\"64\"";
requestHeaders["Sec-CH-UA-Model"] = "\"\"";
requestHeaders["Sec-CH-UA-Mobile"] = "?0";
requestHeaders["Sec-CH-UA-Full-Version-List"] = FullVersionList;

await pipeline(httpContext).ConfigureAwait(false);

IHeaderDictionary responseHeaders = httpContext.Response.Headers;
Check(string.Equals(responseHeaders["Accept-CH"],
    "User-Agent, Sec-CH-UA, Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version, Sec-CH-UA-Arch, Sec-CH-UA-Bitness, Sec-CH-UA-Model, Sec-CH-UA-Mobile, Sec-CH-UA-Full-Version-List",
    StringComparison.Ordinal), "Accept-CH response header");
Check(string.Equals(responseHeaders["Accept-CH-Lifetime"], "2678400", StringComparison.Ordinal), "Accept-CH-Lifetime response header");

HttpClientHints hints = httpContext.GetClientHints();
Check(ReferenceEquals(hints, snapshotInHandler), "GetClientHints caches the record per request");
Check(hints is { Platform: "\"Windows\"", DecodedPlatform: "Windows", DecodedPlatformVersion: "19.0.0", DecodedArchitecture: "x86", DecodedModel: "", Mobile: false },
    "GetClientHints on HttpContext");
Check(string.Equals(hints.FullVersionList, FullVersionList, StringComparison.Ordinal) && hints.UA is not null && hints.UserAgent is not null, "brand lists and User-Agent");
Check(string.Equals(HttpClientHintsInterpreter.DecodeStructuredString(hints.Headers["Sec-CH-UA-Bitness"]), "64", StringComparison.Ordinal), "headers without a property");

HttpClientHintsView view = httpContext.GetClientHintsView();
Check(view is { DecodedPlatform: "Windows", DecodedArchitecture: "x86", Mobile: false }, "GetClientHintsView on HttpContext");
Check(requestHeaders.GetClientHints() is { DecodedPlatform: "Windows" } && requestHeaders.GetClientHintsView() is { Mobile: false }, "extensions on IHeaderDictionary");

IHttpClientHintsFeature? feature = httpContext.Features.Get<IHttpClientHintsFeature>();
Check(feature is { View.DecodedPlatform: "Windows", View.Mobile: false }, "IHttpClientHintsFeature.View");
Check(ReferenceEquals(feature?.Snapshot, hints), "IHttpClientHintsFeature.Snapshot");

Console.WriteLine(failures == 0 ? "Native AOT smoke test passed." : $"Native AOT smoke test failed: {failures} check(s).");
return failures == 0 ? 0 : 1;
