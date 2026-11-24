// Copyright © https://myCSharp.de - all rights reserved

using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using MyCSharp.HttpClientHints.AspNetCore;
using ClientHints = MyCSharp.HttpClientHints.HttpClientHints;

namespace HttpClientHints.Benchmarks;

[MemoryDiagnoser]
public class HttpClientHintBenchmarks
{
    private readonly HeaderDictionary _headersWithoutHints = new() { ["User-Agent"] = "Mozilla/5.0" };
    private HeaderDictionary _headers = null!;
    private DefaultHttpContext _uncachedContext = null!;
    private DefaultHttpContext _cachedContext = null!;

    [GlobalSetup]
    public void Setup()
    {
        _headers = new HeaderDictionary
        {
            ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
            ["Sec-CH-UA"] = "\"Chromium\";v=\"131\", \"Not;A=Brand\";v=\"99\", \"Google Chrome\";v=\"131\"",
            ["Sec-CH-UA-Platform"] = "\"Windows\"",
            ["Sec-CH-UA-Platform-Version"] = "\"15.0.0\"",
            ["Sec-CH-UA-Arch"] = "\"x86\"",
            ["Sec-CH-UA-Bitness"] = "\"64\"",
            ["Sec-CH-UA-WoW64"] = "?0",
            ["Sec-CH-UA-Form-Factors"] = "\"Desktop\"",
            ["Sec-CH-UA-Full-Version-List"] = "\"Chromium\";v=\"131.0.6778.85\", \"Not;A=Brand\";v=\"99.0.0.0\", \"Google Chrome\";v=\"131.0.6778.85\"",
            ["Sec-CH-UA-Model"] = "\"\"",
            ["Sec-CH-UA-Mobile"] = "?0"
        };

        _uncachedContext = CreateContext();
        _cachedContext = CreateContext();
    }

    [Benchmark]
    public int ReadViewProperties()
    {
        HttpClientHintsView view = _headers.GetClientHintsView();
        return (view.UA?.Length ?? 0) + (view.Platform?.Length ?? 0)
            + (view.Architecture?.Length ?? 0) + (view.Mobile == false ? 1 : 0);
    }

    [Benchmark]
    public int ReadDecodedViewProperties()
    {
        HttpClientHintsView view = _headers.GetClientHintsView();
        return (view.UA?.Length ?? 0) + (view.DecodedPlatform?.Length ?? 0)
            + (view.DecodedArchitecture?.Length ?? 0) + (view.Mobile == false ? 1 : 0);
    }

    [Benchmark]
    public int ReadDecodedBrandLists()
    {
        HttpClientHintsView view = _headers.GetClientHintsView();
        return (view.DecodedUA?.Count ?? 0) + (view.DecodedFullVersionList?.Count ?? 0);
    }

    [Benchmark]
    public ClientHints GetClientHintsFromHeaders() => _headers.GetClientHints();

    [Benchmark]
    public ClientHints GetClientHintsFromHeadersWithoutHints() => _headersWithoutHints.GetClientHints();

    [Benchmark]
    public ClientHints GetClientHintsFromContextUncached()
    {
        _uncachedContext.Items.Clear();
        return _uncachedContext.GetClientHints();
    }

    [Benchmark]
    public ClientHints GetClientHintsFromContextCached() => _cachedContext.GetClientHints();

    private DefaultHttpContext CreateContext()
    {
        FeatureCollection features = new();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature { Headers = _headers });
        DefaultHttpContext context = new(features);
        _ = context.GetClientHints();
        return context;
    }
}
