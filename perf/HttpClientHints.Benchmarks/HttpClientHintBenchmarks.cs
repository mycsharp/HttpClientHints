// Copyright © https://myCSharp.de - all rights reserved

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using MyCSharp.HttpClientHints.AspNetCore;

namespace HttpClientHints.Benchmarks;

/// <summary>
/// Benchmarks for comparing zero-allocation view vs. snapshot materialization and legacy extension.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Declared)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class HttpClientHintBenchmarks
{
    private HeaderDictionary _headers = null!;
    private DefaultHttpContext _httpContext = null!;

    [GlobalSetup]
    public void Setup()
    {
        HeaderDictionary headers = new();
        // representative set of headers
        headers["User-Agent"] = new StringValues("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
        headers["Sec-CH-UA"] = new StringValues("\"Chromium\";v=\"127\", \"Not;A=Brand\";v=\"99\"\"Chrome\";v=\"127\"");
        headers["Sec-CH-UA-Platform"] = new StringValues("\"Windows\"");
        headers["Sec-CH-UA-Platform-Version"] = new StringValues("\"15.0.0\"");
        headers["Sec-CH-UA-Arch"] = new StringValues("\"x86\"");
        headers["Sec-CH-UA-Full-Version-List"] = new StringValues("\"Chromium\";v=\"127.0.6533.72\", \"Not;A=Brand\";v=\"99.0.0.0\", \"Chrome\";v=\"127.0.6533.72\"");
        headers["Sec-CH-UA-Model"] = new StringValues("\"\"");
        headers["Sec-CH-UA-Mobile"] = new StringValues("?0");

        _headers = headers;
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Headers.Clear();
        foreach (KeyValuePair<string, StringValues> kvp in headers)
        {
            _httpContext.Request.Headers[kvp.Key] = kvp.Value;
        }
    }

    [Benchmark(Description = "View: read few properties (no alloc)")]
    public int View_Read_Properties()
    {
        HttpClientHintsView view = _headers.GetClientHintsView();
        // touch several properties to simulate typical usage
        string ua = view.UA;
        string platform = view.Platform;
        string arch = view.Architecture;
        bool? mobile = view.Mobile;

        int hash = 17;
        hash = (hash * 31) + (ua?.Length ?? 0);
        hash = (hash * 31) + (platform?.Length ?? 0);
        hash = (hash * 31) + (arch?.Length ?? 0);
        hash = (hash * 31) + (mobile.HasValue ? (mobile.Value ? 1 : 2) : 3);
        return hash;
    }

    [Benchmark(Description = "View: BuildSnapshot (alloc 1)")]
    public int View_BuildSnapshot()
    {
        HttpClientHintsView view = _headers.GetClientHintsView();
        MyCSharp.HttpClientHints.HttpClientHints snapshot = view.BuildSnapshot();
        int hash = 17;
        hash = (hash * 31) + (snapshot.UA?.Length ?? 0);
        hash = (hash * 31) + (snapshot.Platform?.Length ?? 0);
        hash = (hash * 31) + (snapshot.Architecture?.Length ?? 0);
        hash = (hash * 31) + (snapshot.Mobile.HasValue ? (snapshot.Mobile.Value ? 1 : 2) : 3);
        return hash;
    }

    [Benchmark(Description = "Extension: GetClientHints(headers) (alloc 1)")]
    public int Extension_GetClientHints_FromHeaders()
    {
        MyCSharp.HttpClientHints.HttpClientHints hints = _headers.GetClientHints();
        int hash = 17;
        hash = (hash * 31) + (hints.UA?.Length ?? 0);
        hash = (hash * 31) + (hints.Platform?.Length ?? 0);
        hash = (hash * 31) + (hints.Architecture?.Length ?? 0);
        hash = (hash * 31) + (hints.Mobile.HasValue ? (hints.Mobile.Value ? 1 : 2) : 3);
        return hash;
    }

    [Benchmark(Description = "Extension: GetClientHints(context) (alloc 1, cached)")]
    public int Extension_GetClientHints_FromHttpContext()
    {
        // clear cache to measure allocation consistently
        _httpContext.Items.Remove("__HttpClientHints");
        MyCSharp.HttpClientHints.HttpClientHints hints = _httpContext.GetClientHints();
        int hash = 17;
        hash = (hash * 31) + (hints.UA?.Length ?? 0);
        hash = (hash * 31) + (hints.Platform?.Length ?? 0);
        hash = (hash * 31) + (hints.Architecture?.Length ?? 0);
        hash = (hash * 31) + (hints.Mobile.HasValue ? (hints.Mobile.Value ? 1 : 2) : 3);
        return hash;
    }
}
