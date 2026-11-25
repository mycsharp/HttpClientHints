// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
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
            ResponseHeader = string.Empty,
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
        HttpClientHintsMiddlewareConfig config = new() { ResponseHeader = "Sec-CH-UA", CriticalHeader = string.Empty, VaryHeader = string.Empty };
        HttpClientHintsRequestMiddleware middleware = new(static _ => Task.CompletedTask, Options.Create(config));
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal("Sec-CH-UA", context.Response.Headers["Accept-CH"]);
        Assert.False(context.Response.Headers.ContainsKey("Critical-CH"));
        Assert.False(context.Response.Headers.ContainsKey("Vary"));
    }
}
