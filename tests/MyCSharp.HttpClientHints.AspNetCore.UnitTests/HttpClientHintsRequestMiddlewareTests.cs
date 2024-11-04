// Copyright © myCSharp.de - all rights reserved

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
}
