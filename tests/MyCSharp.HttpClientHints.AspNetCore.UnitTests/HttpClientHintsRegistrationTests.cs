// Copyright © myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;


namespace MyCSharp.HttpClientHints.AspNetCore.UnitTests;

public class HttpClientHintsRegistrationTests
{
    [Fact]
    public void AddHttpClientHints_DefaultOptions_RegistersDefaultHeaders()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddHttpClientHints();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        HttpClientHintsMiddlewareConfig options = serviceProvider.GetRequiredService<IOptions<HttpClientHintsMiddlewareConfig>>().Value;

        // Assert
        Assert.Contains("User-Agent", options.ResponseHeader);
        Assert.Contains("Sec-CH-UA", options.ResponseHeader);
        Assert.Contains("Sec-CH-UA-Platform", options.ResponseHeader);
        Assert.Contains("Sec-CH-UA-Platform-Version", options.ResponseHeader);
        Assert.Contains("Sec-CH-UA-Arch", options.ResponseHeader);
        Assert.Contains("Sec-CH-UA-Bitness", options.ResponseHeader);
        Assert.Contains("Sec-CH-UA-Model", options.ResponseHeader);
        Assert.Contains("Sec-CH-UA-Mobile", options.ResponseHeader);
        Assert.Null(options.LifeTime);
    }

    [Fact]
    public void AddHttpClientHints_CustomLifetime_RegistersLifetime()
    {
        // Arrange
        ServiceCollection services = new();
        TimeSpan lifetime = TimeSpan.FromMinutes(30);

        // Act
        services.AddHttpClientHints(options => options.Lifetime = lifetime);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        HttpClientHintsMiddlewareConfig options = serviceProvider.GetRequiredService<IOptions<HttpClientHintsMiddlewareConfig>>().Value;

        // Assert
        Assert.Equal("1800", options.LifeTime); // 1800 seconds
    }

    [Fact]
    public void AddHttpClientHints_DisablePlatformHeader_DoesNotRegisterPlatformHeaders()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddHttpClientHints(options => options.Platform = false);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        HttpClientHintsMiddlewareConfig options = serviceProvider.GetRequiredService<IOptions<HttpClientHintsMiddlewareConfig>>().Value;

        // Assert
        Assert.DoesNotContain("Sec-CH-UA-Platform", options.ResponseHeader);
        Assert.DoesNotContain("Sec-CH-UA-Platform-Version", options.ResponseHeader);
    }

    [Fact]
    public void AddHttpClientHints_CustomAdditionalHeader_RegistersAdditionalHeader()
    {
        // Arrange
        ServiceCollection services = new();
        string[] customHeaders = ["X-Custom-Header"];

        // Act
        services.AddHttpClientHints(options => options.Additional = customHeaders);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        HttpClientHintsMiddlewareConfig options = serviceProvider.GetRequiredService<IOptions<HttpClientHintsMiddlewareConfig>>().Value;

        // Assert
        Assert.Contains("X-Custom-Header", options.ResponseHeader);
    }

    [Fact]
    public void AddHttpClientHints_AllHeadersDisabled_RegistersNoHeaders()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddHttpClientHints(options =>
        {
            options.UserAgent = false;
            options.Platform = false;
            options.Architecture = false;
            options.Device = false;
            options.Mobile = false;
            options.Additional = null;
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        HttpClientHintsMiddlewareConfig options = serviceProvider.GetRequiredService<IOptions<HttpClientHintsMiddlewareConfig>>().Value;

        // Assert
        Assert.Empty(options.ResponseHeader);
    }

    [Fact]
    public void UseHttpClientHints_ReturnsApplicationBuilder()
    {
        // Arrange
        IApplicationBuilder appBuilder = Substitute.For<IApplicationBuilder>();

        // Act
        IApplicationBuilder result = appBuilder.UseHttpClientHints();

        // Assert
        Assert.NotNull(result);
        Assert.Same(appBuilder, result); // Ensure the same instance is returned
    }
}
