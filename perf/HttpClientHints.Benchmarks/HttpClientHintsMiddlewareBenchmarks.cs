// Copyright © https://myCSharp.de - all rights reserved

using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpClientHints.AspNetCore;

namespace HttpClientHints.Benchmarks;

[MemoryDiagnoser]
public class HttpClientHintsMiddlewareBenchmarks
{
    private ServiceProvider _serviceProvider = null!;
    private RequestDelegate _acceptChPipeline = null!;
    private RequestDelegate _featurePipeline = null!;
    private RequestDelegate _bothPipeline = null!;
    private RequestDelegate _criticalAndVaryPipeline = null!;
    private ServiceProvider _criticalAndVaryServiceProvider = null!;
    private DefaultHttpContext _context = null!;

    [GlobalSetup]
    public void Setup()
    {
        ServiceCollection services = new();
        services.AddHttpClientHints(static options => options.Platform = true);
        _serviceProvider = services.BuildServiceProvider();

        ServiceCollection criticalAndVaryServices = new();
        criticalAndVaryServices.AddHttpClientHints(static options =>
        {
            options.Platform = true;
            options.Critical = ["Sec-CH-UA-Platform-Version"];
            options.Vary = true;
        });
        _criticalAndVaryServiceProvider = criticalAndVaryServices.BuildServiceProvider();

        _acceptChPipeline = BuildPipeline(_serviceProvider, static app => app.UseHttpClientHints());
        _featurePipeline = BuildPipeline(_serviceProvider, static app => app.UseHttpClientHintsFeature());
        _bothPipeline = BuildPipeline(_serviceProvider, static app => app.UseHttpClientHints().UseHttpClientHintsFeature());
        _criticalAndVaryPipeline = BuildPipeline(_criticalAndVaryServiceProvider, static app => app.UseHttpClientHints());

        // Every invocation reuses this context. Servers such as Kestrel allocate differently when a feature is registered.
        _context = new DefaultHttpContext();
        _context.Request.Headers["Sec-CH-UA"] = "\"Chromium\";v=\"131\", \"Not;A=Brand\";v=\"99\", \"Google Chrome\";v=\"131\"";
        _context.Request.Headers["Sec-CH-UA-Platform"] = "\"Windows\"";
        _context.Request.Headers["Sec-CH-UA-Mobile"] = "?0";
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
        _criticalAndVaryServiceProvider.Dispose();
    }

    [Benchmark]
    public Task UseHttpClientHints() => _acceptChPipeline(_context);

    [Benchmark]
    public Task UseHttpClientHintsWithCriticalAndVary()
    {
        // Vary is appended, so the response headers are reset like for a new response.
        _context.Response.Headers.Remove("Vary");
        return _criticalAndVaryPipeline(_context);
    }

    [Benchmark]
    public Task UseHttpClientHintsFeature() => _featurePipeline(_context);

    [Benchmark]
    public Task UseHttpClientHintsAndFeature() => _bothPipeline(_context);

    [Benchmark]
    public bool? UseHttpClientHintsFeatureAndReadView()
    {
        _ = _featurePipeline(_context);
        return _context.Features.Get<IHttpClientHintsFeature>()!.View.Mobile;
    }

    private static RequestDelegate BuildPipeline(ServiceProvider serviceProvider, Action<IApplicationBuilder> configure)
    {
        ApplicationBuilder app = new(serviceProvider);
        configure(app);
        app.Run(static _ => Task.CompletedTask);
        return app.Build();
    }
}
