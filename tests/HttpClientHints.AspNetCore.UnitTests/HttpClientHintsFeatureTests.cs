// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpClientHints.AspNetCore.UnitTests;

public class HttpClientHintsFeatureTests
{
    private static readonly IServiceProvider s_emptyServices = new ServiceCollection().BuildServiceProvider();

    [Fact]
    public void UseHttpClientHintsFeature_ReturnsApplicationBuilder()
    {
        ApplicationBuilder appBuilder = new(s_emptyServices);

        IApplicationBuilder result = appBuilder.UseHttpClientHintsFeature();

        Assert.Same(appBuilder, result);
    }

    [Fact]
    public async Task UseHttpClientHintsFeature_WithoutAddHttpClientHints_ProvidesFeatureToHandlers()
    {
        DefaultHttpContext context = new();
        context.Request.Headers["Sec-CH-UA-Mobile"] = "?1";
        IHttpClientHintsFeature? featureInHandler = null;
        RequestDelegate pipeline = BuildFeaturePipeline(ctx =>
        {
            featureInHandler = ctx.Features.Get<IHttpClientHintsFeature>();
            return Task.CompletedTask;
        });

        await pipeline(context);

        Assert.NotNull(featureInHandler);
        Assert.True(featureInHandler.View.Mobile);
        Assert.False(context.Response.Headers.ContainsKey("Accept-CH"));
    }

    [Fact]
    public async Task UseHttpClientHintsFeature_WithUseHttpClientHints_ProvidesFeatureAndAcceptCH()
    {
        ServiceCollection services = new();
        services.AddHttpClientHints();
        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ApplicationBuilder appBuilder = new(serviceProvider);
        bool? mobile = null;

        appBuilder.UseHttpClientHints();
        appBuilder.UseHttpClientHintsFeature();
        appBuilder.Run(ctx =>
        {
            mobile = ctx.Features.Get<IHttpClientHintsFeature>()?.View.Mobile;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new();
        context.Request.Headers["Sec-CH-UA-Mobile"] = "?0";
        await appBuilder.Build()(context);

        Assert.False(mobile);
        Assert.Contains("Sec-CH-UA-Mobile", context.Response.Headers["Accept-CH"].ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task UseHttpClientHints_WithoutUseHttpClientHintsFeature_DoesNotRegisterFeature()
    {
        ServiceCollection services = new();
        services.AddHttpClientHints();
        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ApplicationBuilder appBuilder = new(serviceProvider);

        appBuilder.UseHttpClientHints();
        appBuilder.Run(static _ => Task.CompletedTask);
        DefaultHttpContext context = new();
        await appBuilder.Build()(context);

        Assert.Null(context.Features.Get<IHttpClientHintsFeature>());
        Assert.True(context.Response.Headers.ContainsKey("Accept-CH"));
    }

    [Fact]
    public void UseHttpClientHintsFeature_ReturnsTaskOfNextMiddleware()
    {
        Task nextTask = new TaskCompletionSource().Task;
        RequestDelegate pipeline = BuildFeaturePipeline(_ => nextTask);

        Task result = pipeline(new DefaultHttpContext());

        Assert.Same(nextTask, result);
    }

    [Fact]
    public async Task UseHttpClientHintsFeature_ReplacesFeatureRegisteredBefore()
    {
        DefaultHttpContext context = new();
        IHttpClientHintsFeature existing = new StubFeature();
        context.Features.Set(existing);

        IHttpClientHintsFeature feature = await RegisterFeatureAsync(context);

        Assert.NotSame(existing, feature);
    }

    [Fact]
    public async Task Feature_View_ReadsCurrentRequestHeaders()
    {
        DefaultHttpContext context = new();
        context.Request.Headers["Sec-CH-UA-Platform"] = "\"Windows\"";
        IHttpClientHintsFeature feature = await RegisterFeatureAsync(context);

        Assert.Equal("Windows", feature.View.DecodedPlatform);

        context.Request.Headers["Sec-CH-UA-Platform"] = "\"Linux\"";

        Assert.Same(context.Request.Headers, feature.View.Headers);
        Assert.Equal("\"Linux\"", feature.View.Platform);
        Assert.Equal("Linux", feature.View.DecodedPlatform);
    }

    [Fact]
    public async Task Feature_Snapshot_IsSharedWithGetClientHints()
    {
        DefaultHttpContext context = new();
        context.Request.Headers["Sec-CH-UA-Model"] = "\"Pixel 9\"";
        IHttpClientHintsFeature feature = await RegisterFeatureAsync(context);

        HttpClientHints snapshot = feature.Snapshot;
        context.Request.Headers["Sec-CH-UA-Model"] = "\"Pixel 10\"";

        Assert.Same(snapshot, feature.Snapshot);
        Assert.Same(snapshot, context.GetClientHints());
        Assert.Equal("Pixel 9", snapshot.DecodedModel);
        Assert.Equal("\"Pixel 10\"", feature.View.Model);
    }

    [Fact]
    public async Task Feature_Snapshot_KeepsValuesButHeadersReferenceRequestHeaders()
    {
        DefaultHttpContext context = new();
        context.Request.Headers["Sec-CH-UA-Platform"] = "\"Windows\"";
        IHttpClientHintsFeature feature = await RegisterFeatureAsync(context);

        HttpClientHints snapshot = feature.Snapshot;
        context.Request.Headers["Sec-CH-UA-Platform"] = "\"Linux\"";

        Assert.Equal("\"Windows\"", snapshot.Platform);
        Assert.Same(context.Request.Headers, snapshot.Headers);
        Assert.Equal("\"Linux\"", snapshot.Headers["Sec-CH-UA-Platform"].ToString());
    }

    [Fact]
    public async Task Feature_Snapshot_ReturnsSnapshotCreatedBeforeRegistration()
    {
        DefaultHttpContext context = new();
        context.Request.Headers["Sec-CH-UA"] = "before";
        HttpClientHints snapshot = context.GetClientHints();

        IHttpClientHintsFeature feature = await RegisterFeatureAsync(context);

        Assert.Same(snapshot, feature.Snapshot);
    }

    private static RequestDelegate BuildFeaturePipeline(RequestDelegate handler)
    {
        ApplicationBuilder appBuilder = new(s_emptyServices);
        appBuilder.UseHttpClientHintsFeature();
        appBuilder.Run(handler);
        return appBuilder.Build();
    }

    private static async Task<IHttpClientHintsFeature> RegisterFeatureAsync(HttpContext context)
    {
        RequestDelegate pipeline = BuildFeaturePipeline(static _ => Task.CompletedTask);
        await pipeline(context).ConfigureAwait(false);

        IHttpClientHintsFeature? feature = context.Features.Get<IHttpClientHintsFeature>();
        Assert.NotNull(feature);
        return feature;
    }

    private sealed class StubFeature : IHttpClientHintsFeature
    {
        public HttpClientHintsView View => default;

        public HttpClientHints Snapshot => throw new NotSupportedException();
    }
}
