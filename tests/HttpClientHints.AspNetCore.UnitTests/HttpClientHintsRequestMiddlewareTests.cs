// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MyCSharp.HttpClientHints.AspNetCore.UnitTests;

public class HttpClientHintsRequestMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithConfiguredResponseHeader_AddsAcceptCHHeader()
    {
        // Arrange
        DefaultHttpContext context = new();
        IOptions<HttpClientHintsMiddlewareConfig> options = Options.Create(new HttpClientHintsMiddlewareConfig
        {
            ResponseHeader = "User-Agent, Sec-CH-UA"
        });
        HttpClientHintsRequestMiddleware middleware = new(context => Task.CompletedTask, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Accept-CH"));
        Assert.Equal("User-Agent, Sec-CH-UA", context.Response.Headers["Accept-CH"]);
    }

    [Fact]
    public async Task InvokeAsync_WithConfiguredLifetime_AddsAcceptCHLifetimeHeader()
    {
        // Arrange
        DefaultHttpContext context = new();
        IOptions<HttpClientHintsMiddlewareConfig> options = Options.Create(new HttpClientHintsMiddlewareConfig
        {
            ResponseHeader = "User-Agent",
            LifeTime = "3600"
        });
        HttpClientHintsRequestMiddleware middleware = new(context => Task.CompletedTask, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Accept-CH"));
        Assert.True(context.Response.Headers.ContainsKey("Accept-CH-Lifetime"));
        Assert.Equal("User-Agent", context.Response.Headers["Accept-CH"]);
        Assert.Equal("3600", context.Response.Headers["Accept-CH-Lifetime"]);
    }

    [Fact]
    public async Task InvokeAsync_WithoutResponseHeader_DoesNotAddHeaders()
    {
        // Arrange
        DefaultHttpContext context = new();
        IOptions<HttpClientHintsMiddlewareConfig> options = Options.Create(new HttpClientHintsMiddlewareConfig
        {
            ResponseHeader = null,
            LifeTime = "3600"
        });
        HttpClientHintsRequestMiddleware middleware = new(context => Task.CompletedTask, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.Headers.ContainsKey("Accept-CH"));
        Assert.False(context.Response.Headers.ContainsKey("Accept-CH-Lifetime"));
    }

    [Fact]
    public async Task InvokeAsync_ExplicitOptOut_SendsAnEmptyAcceptCH()
    {
        ServiceCollection services = new();
        services.AddHttpClientHints(static options => { options.UserAgent = false; options.Mobile = false; });
        using ServiceProvider provider = services.BuildServiceProvider();
        ApplicationBuilder app = new(provider);
        app.UseHttpClientHints();
        DefaultHttpContext context = new();
        await app.Build()(context);
        Assert.True(context.Response.Headers.ContainsKey("Accept-CH"));
        Assert.Equal(string.Empty, Assert.Single(context.Response.Headers["Accept-CH"].ToArray()));
    }

    [Fact]
    public async Task InvokeAsync_WithoutLifetime_DoesNotAddAcceptCHLifetimeHeader()
    {
        // Arrange
        DefaultHttpContext context = new();
        IOptions<HttpClientHintsMiddlewareConfig> options = Options.Create(new HttpClientHintsMiddlewareConfig
        {
            ResponseHeader = "User-Agent"
        });
        HttpClientHintsRequestMiddleware middleware = new(context => Task.CompletedTask, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Accept-CH"));
        Assert.Equal("User-Agent", context.Response.Headers["Accept-CH"]);
        Assert.False(context.Response.Headers.ContainsKey("Accept-CH-Lifetime"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsSynchronously_ReturnsFaultedTask()
    {
        InvalidOperationException exception = new("next failed");
        IOptions<HttpClientHintsMiddlewareConfig> options = Options.Create(new HttpClientHintsMiddlewareConfig
        {
            ResponseHeader = "User-Agent"
        });
        HttpClientHintsRequestMiddleware middleware = new(_ => throw exception, options);

        Task task = middleware.InvokeAsync(new DefaultHttpContext());

        Assert.True(task.IsFaulted);
        Assert.Same(exception, await Assert.ThrowsAsync<InvalidOperationException>(() => task));
    }

    [Fact]
    public async Task InvokeAsync_DoesNotFlowAsyncLocalChangesOfNextMiddlewareToCaller()
    {
        AsyncLocal<string?> value = new();
        IOptions<HttpClientHintsMiddlewareConfig> options = Options.Create(new HttpClientHintsMiddlewareConfig
        {
            ResponseHeader = "User-Agent"
        });
        HttpClientHintsRequestMiddleware middleware = new(_ =>
        {
            value.Value = "set by next";
            return Task.CompletedTask;
        }, options);

        Task task = middleware.InvokeAsync(new DefaultHttpContext());

        Assert.Null(value.Value);
        await task;
    }

    [Fact]
    public async Task InvokeAsync_UsesConfigurationChangedAfterConstruction()
    {
        HttpClientHintsMiddlewareConfig config = new() { ResponseHeader = string.Empty };
        HttpClientHintsRequestMiddleware middleware = new(static _ => Task.CompletedTask, Options.Create(config));
        config.ResponseHeader = "Sec-CH-UA-Mobile";
        config.LifeTime = "60";
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal("Sec-CH-UA-Mobile", context.Response.Headers["Accept-CH"]);
        Assert.Equal("60", context.Response.Headers["Accept-CH-Lifetime"]);
    }

    [Fact]
    public async Task InvokeAsync_WithCriticalAndVary_SetsCriticalCHAndAppendsToVary()
    {
        HttpClientHintsMiddlewareConfig config = new()
        {
            ResponseHeader = "Sec-CH-UA, Sec-CH-UA-Platform-Version",
            CriticalHeader = "Sec-CH-UA-Platform-Version",
            VaryHeader = "Sec-CH-UA-Platform-Version",
        };
        HttpClientHintsRequestMiddleware middleware = new(static _ => Task.CompletedTask, Options.Create(config));
        DefaultHttpContext context = new();
        context.Response.Headers.Vary = "Accept-Encoding";

        await middleware.InvokeAsync(context);

        Assert.Equal("Sec-CH-UA, Sec-CH-UA-Platform-Version", context.Response.Headers["Accept-CH"]);
        Assert.Equal("Sec-CH-UA-Platform-Version", context.Response.Headers["Critical-CH"]);
        Assert.Equal(["Accept-Encoding", "Sec-CH-UA-Platform-Version"], context.Response.Headers.Vary.ToArray());
        Assert.False(context.Response.Headers.ContainsKey("Accept-CH-Lifetime"));
    }

    [Fact]
    public async Task InvokeAsync_RunningAgainForTheSameResponse_AddsVaryOnce()
    {
        HttpClientHintsMiddlewareConfig config = new()
        {
            ResponseHeader = "Sec-CH-UA, Sec-CH-UA-Platform-Version",
            CriticalHeader = "Sec-CH-UA-Platform-Version",
            VaryHeader = "Sec-CH-UA-Platform-Version",
        };
        HttpClientHintsRequestMiddleware middleware = new(static _ => Task.CompletedTask, Options.Create(config));
        DefaultHttpContext context = new();
        context.Response.Headers.Vary = "Accept-Encoding";

        // For example UseStatusCodePagesWithReExecute runs the pipeline again for an error page
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        Assert.Equal(["Accept-Encoding", "Sec-CH-UA-Platform-Version"], context.Response.Headers.Vary.ToArray());
        Assert.Equal("Sec-CH-UA-Platform-Version", context.Response.Headers["Critical-CH"]);
    }

    [Fact]
    public async Task InvokeAsync_WithoutCriticalAndVary_SetsOnlyAcceptCH()
    {
        HttpClientHintsMiddlewareConfig config = new() { ResponseHeader = "Sec-CH-UA", CriticalHeader = string.Empty, VaryHeader = null };
        HttpClientHintsRequestMiddleware middleware = new(static _ => Task.CompletedTask, Options.Create(config));
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal("Sec-CH-UA", context.Response.Headers["Accept-CH"]);
        Assert.False(context.Response.Headers.ContainsKey("Critical-CH"));
        Assert.False(context.Response.Headers.ContainsKey("Vary"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Accept-Encoding, sec-ch-ua")]
    [InlineData("Sec-CH-UA, Sec-CH-UA-Mobile")]
    [InlineData("*")]
    public async Task InvokeAsync_RestoresHintsWhenMvcAppliesResponseCache(string? varyByHeader)
    {
        using ServiceProvider services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();
        DefaultHttpContext context = new() { RequestServices = services };
        StartingResponseFeature response = new();
        context.Features.Set<IHttpResponseFeature>(response);
        ResponseCacheAttribute attribute = new() { Duration = 60, VaryByHeader = varyByHeader };
        IActionFilter filter = (IActionFilter)attribute.CreateInstance(services);
        HttpClientHintsMiddlewareConfig config = new() { ResponseHeader = "Sec-CH-UA, Sec-CH-UA-Mobile", VaryHeader = "Sec-CH-UA, Sec-CH-UA-Mobile" };
        HttpClientHintsRequestMiddleware middleware = new(ctx =>
        {
            ActionContext action = new(ctx, new RouteData(), new ActionDescriptor());
            filter.OnActionExecuting(new ActionExecutingContext(action, [filter], new Dictionary<string, object?>(StringComparer.Ordinal), new object()));
            return Task.CompletedTask;
        }, Options.Create(config));

        await middleware.InvokeAsync(context);
        await response.StartAsync();

        Assert.Equal("public,max-age=60", context.Response.Headers.CacheControl);
        string[] expected = varyByHeader switch
        {
            null or "Sec-CH-UA, Sec-CH-UA-Mobile" => ["Sec-CH-UA", "Sec-CH-UA-Mobile"],
            "*" => ["*"],
            _ => ["Accept-Encoding", "sec-ch-ua", "Sec-CH-UA-Mobile"],
        };
        Assert.Equal(expected, context.Response.Headers.GetCommaSeparatedValues("Vary"));
    }

    [Theory]
    [InlineData("Other,\tsec-ch-ua ", 2)]
    [InlineData("\"sec-ch-ua\"", 2)]
    [InlineData("\"Other,*,Field\"", 3)]
    public void MergeVary_ChecksEveryFieldLine(string secondLine, int expectedCount)
    {
        IHeaderDictionary headers = new HeaderDictionary { ["Vary"] = new[] { "Accept-Encoding", secondLine } };
        HttpClientHintsMiddlewareConfig config = new() { VaryHeader = "Sec-CH-UA" };
        config.MergeVary(headers);
        Assert.Equal(expectedCount, headers.Vary.Count);
        Assert.Equal(secondLine, headers.Vary[1]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ResponseCaching_UsesTheHintsWhenAnEndpointReplacesVary(bool cacheFirst)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddResponseCaching();
        services.AddHttpClientHints(static options => options.Vary = true);
        using ServiceProvider provider = services.BuildServiceProvider();
        ApplicationBuilder app = new(provider);
        if (cacheFirst)
        {
            app.UseResponseCaching();
        }

        app.UseHttpClientHints();
        if (!cacheFirst)
        {
            app.UseResponseCaching();
        }

        int executions = 0;
        app.Run(async ctx =>
        {
            executions++;
            ctx.Response.Headers.CacheControl = "public,max-age=60";
            ctx.Response.Headers.Vary = "Accept-Language";
            await ctx.Response.WriteAsync(ctx.Request.Headers["Sec-CH-UA-Mobile"].ToString(), ctx.RequestAborted).ConfigureAwait(false);
        });
        RequestDelegate pipeline = app.Build();

        foreach (string hint in new[] { "?1", "?0", "?1" })
        {
            DefaultHttpContext ctx = new() { RequestServices = provider };
            ctx.Request.Method = HttpMethods.Get;
            ctx.Request.Scheme = "https";
            ctx.Request.Host = new HostString("example.test");
            ctx.Request.Path = "/variant";
            ctx.Request.Headers["Sec-CH-UA-Mobile"] = hint;
            using MemoryStream body = new();
            ctx.Response.Body = body;
            await pipeline(ctx);
            Assert.Equal(hint, System.Text.Encoding.UTF8.GetString(body.ToArray()));
            Assert.Equal(["Accept-Language", "Sec-CH-UA", "Sec-CH-UA-Mobile"], ctx.Response.Headers.GetCommaSeparatedValues("Vary"));
        }

        Assert.Equal(2, executions);
    }

    [Fact]
    public async Task VaryFeature_ForwardsResponseOperationsAndRestoresTheOriginalOnFailure()
    {
        DefaultHttpContext ctx = new();
        StartingResponseFeature original = new();
        ctx.Features.Set<IHttpResponseFeature>(original);
        using MemoryStream body = new();
        HttpClientHintsMiddlewareConfig config = new() { ResponseHeader = "Sec-CH-UA", VaryHeader = "Sec-CH-UA" };
        HttpClientHintsRequestMiddleware middleware = new(context =>
        {
            IHttpResponseFeature feature = context.Features.GetRequiredFeature<IHttpResponseFeature>();
            feature.StatusCode = 201;
            feature.ReasonPhrase = "Created";
            Assert.Equal(201, original.StatusCode);
            Assert.Equal(original.ReasonPhrase, feature.ReasonPhrase);
            Assert.Equal(original.StatusCode, feature.StatusCode);
#pragma warning disable CS0618 // The decorator must forward the legacy body feature as well.
            feature.Body = body;
            Assert.Same(body, feature.Body);
#pragma warning restore CS0618
            HeaderDictionary replacement = new();
            feature.Headers = replacement;
            Assert.Same(replacement, original.Headers);
            feature.OnCompleted(static _ => Task.CompletedTask, context);
            feature.OnStarting(static _ => Task.CompletedTask, context);
            Assert.False(feature.HasStarted);
            replacement.IsReadOnly = true;
            Assert.Same(replacement, feature.Headers);
            throw new InvalidOperationException("downstream failed");
        }, Options.Create(config));

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(ctx));
        Assert.Same(original, ctx.Features.Get<IHttpResponseFeature>());
    }

    [Fact]
    public async Task InvokeAsync_PreservesAReplacementResponseFeature()
    {
        DefaultHttpContext ctx = new();
        HttpResponseFeature replacement = new();
        HttpClientHintsMiddlewareConfig config = new() { ResponseHeader = "Sec-CH-UA", VaryHeader = "Sec-CH-UA" };
        HttpClientHintsRequestMiddleware middleware = new(context =>
        {
            context.Features.Set<IHttpResponseFeature>(replacement);
            return Task.CompletedTask;
        }, Options.Create(config));
        await middleware.InvokeAsync(ctx);
        Assert.Same(replacement, ctx.Features.Get<IHttpResponseFeature>());
        Assert.Equal("Sec-CH-UA", replacement.Headers.Vary);
    }

    private sealed class StartingResponseFeature : HttpResponseFeature
    {
        private readonly Stack<(Func<object, Task> Callback, object State)> _callbacks = new();

        public override void OnStarting(Func<object, Task> callback, object state) => _callbacks.Push((callback, state));

        public async Task StartAsync()
        {
            while (_callbacks.TryPop(out (Func<object, Task> Callback, object State) entry))
            {
                await entry.Callback(entry.State).ConfigureAwait(false);
            }
        }
    }
}
