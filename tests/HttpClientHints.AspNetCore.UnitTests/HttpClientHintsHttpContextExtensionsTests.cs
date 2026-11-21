// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace MyCSharp.HttpClientHints.AspNetCore.UnitTests;

public class HttpClientHintsHttpContextExtensionsTests
{
    [Fact]
    public void GetClientHints_ReturnsCorrectValues_WhenHeadersArePresent()
    {
        // Arrange
        HeaderDictionary headers = new()
        {
            { "User-Agent", new StringValues("TestUserAgent") },
            { "Sec-CH-UA", new StringValues("\"Chromium\";v=\"138\", \"Google Chrome\";v=\"138\"") },
            { "Sec-CH-UA-Platform", new StringValues("\"Windows\"") },
            { "Sec-CH-UA-Platform-Version", new StringValues("\"19.0.0\"") },
            { "Sec-CH-UA-Arch", new StringValues("\"x86\"") },
            { "Sec-CH-UA-Bitness", new StringValues("64") },
            { "Sec-CH-UA-Full-Version-List", new StringValues("\"Chromium\";v=\"138.0.7204.97\"") },
            { "Sec-CH-UA-Model", new StringValues("\"\"") },
            { "Sec-CH-UA-Mobile", new StringValues("?1") }
        };

        // Act
        HttpClientHints clientHints = headers.GetClientHints();

        // Assert
        Assert.Equal("TestUserAgent", clientHints.UserAgent);
        Assert.Equal("\"Windows\"", clientHints.Platform);
        Assert.Equal("Windows", clientHints.DecodedPlatform);
        Assert.Equal("\"19.0.0\"", clientHints.PlatformVersion);
        Assert.Equal("19.0.0", clientHints.DecodedPlatformVersion);
        Assert.Equal("\"x86\"", clientHints.Architecture);
        Assert.Equal("x86", clientHints.DecodedArchitecture);
        Assert.Equal("\"\"", clientHints.Model);
        Assert.Equal(string.Empty, clientHints.DecodedModel);
        Assert.Equal("\"Chromium\";v=\"138.0.7204.97\"", clientHints.FullVersionList);
        Assert.Equal("\"Chromium\";v=\"138\", \"Google Chrome\";v=\"138\"", clientHints.UA);
        Assert.True(clientHints.Mobile);
        Assert.Equal(headers, clientHints.Headers);
    }

    [Fact]
    public void GetClientHints_ReturnsNullValues_WhenHeadersAreMissing()
    {
        // Arrange
        HeaderDictionary headers = [];

        // Act
        HttpClientHints clientHints = headers.GetClientHints();

        // Assert
        Assert.Null(clientHints.UserAgent);
        Assert.Null(clientHints.Platform);
        Assert.Null(clientHints.DecodedPlatform);
        Assert.Null(clientHints.PlatformVersion);
        Assert.Null(clientHints.DecodedPlatformVersion);
        Assert.Null(clientHints.Architecture);
        Assert.Null(clientHints.DecodedArchitecture);
        Assert.Null(clientHints.Model);
        Assert.Null(clientHints.DecodedModel);
        Assert.Null(clientHints.FullVersionList);
        Assert.Null(clientHints.UA);
        Assert.Null(clientHints.Mobile);
        Assert.Equal(headers, clientHints.Headers);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("legacy unquoted value", "legacy unquoted value")]
    [InlineData("\"\"", "")]
    [InlineData("\"Model \\\"Pro\\\" \\\\ Edition\"", "Model \"Pro\" \\ Edition")]
    [InlineData("\"Model", null)]
    [InlineData("\"Model\";v=1", null)]
    public void GetClientHints_KeepsHeaderValueAndDecodesOnDemand(string? headerValue, string? decodedValue)
    {
        HeaderDictionary headers = [];
        if (headerValue is not null)
        {
            headers["Sec-CH-UA-Model"] = headerValue;
        }

        HttpClientHintsView view = headers.GetClientHintsView();
        HttpClientHints snapshot = headers.GetClientHints();

        Assert.Equal(headerValue, view.Model);
        Assert.Equal(decodedValue, view.DecodedModel);
        Assert.Equal(headerValue, snapshot.Model);
        Assert.Equal(decodedValue, snapshot.DecodedModel);
    }

    [Fact]
    public void GetClientHintsView_DecodesEveryStructuredStringHeader()
    {
        HeaderDictionary headers = new()
        {
            ["Sec-CH-UA-Platform"] = "\"Windows\"",
            ["Sec-CH-UA-Platform-Version"] = "\"19.0.0\"",
            ["Sec-CH-UA-Arch"] = "\"arm\"",
            ["Sec-CH-UA-Model"] = "\"Pixel \\\"9\\\"\""
        };

        HttpClientHintsView view = headers.GetClientHintsView();

        Assert.Same(headers, view.Headers);
        Assert.Equal("\"Windows\"", view.Platform);
        Assert.Equal("Windows", view.DecodedPlatform);
        Assert.Equal("\"19.0.0\"", view.PlatformVersion);
        Assert.Equal("19.0.0", view.DecodedPlatformVersion);
        Assert.Equal("\"arm\"", view.Architecture);
        Assert.Equal("arm", view.DecodedArchitecture);
        Assert.Equal("\"Pixel \\\"9\\\"\"", view.Model);
        Assert.Equal("Pixel \"9\"", view.DecodedModel);
    }

    [Fact]
    public void GetClientHints_ReturnsCorrectMobileValue_WhenMobileHeaderIsTrue()
    {
        // Arrange
        HeaderDictionary headers = new()
        {
            { "Sec-CH-UA-Mobile", new StringValues("?1") } // true
        };

        // Act
        HttpClientHints clientHints = headers.GetClientHints();

        // Assert
        Assert.True(clientHints.Mobile);
    }

    [Fact]
    public void GetClientHints_ReturnsCorrectMobileValue_WhenMobileHeaderIsFalse()
    {
        // Arrange
        HeaderDictionary headers = new()
        {
            { "Sec-CH-UA-Mobile", new StringValues("?0") } // false
        };

        // Act
        HttpClientHints clientHints = headers.GetClientHints();

        // Assert
        Assert.False(clientHints.Mobile);
    }

    [Fact]
    public void GetClientHints_ReturnsCorrectMobileValue_WhenMobileHeaderIsNull()
    {
        // Arrange
        HeaderDictionary headers = [];

        // Act
        HttpClientHints clientHints = headers.GetClientHints();

        // Assert
        Assert.Null(clientHints.Mobile);
    }

    [Fact]
    public void GetClientHintsView_UsesOnlyTheFirstValue()
    {
        HeaderDictionary headers = new()
        {
            ["Sec-CH-UA"] = new StringValues(["first", "second"]),
            ["Sec-CH-UA-Mobile"] = new StringValues(["?0", "?1"]),
            ["Sec-CH-UA-Platform"] = new StringValues([null, "fallback"]),
            ["Sec-CH-UA-Model"] = new StringValues(string.Empty)
        };

        HttpClientHintsView view = headers.GetClientHintsView();

        Assert.Equal("first", view.UA);
        Assert.False(view.Mobile);
        Assert.Null(view.Platform);
        Assert.Null(view.DecodedPlatform);
        Assert.Equal(string.Empty, view.Model);
        Assert.Equal(string.Empty, view.DecodedModel);
    }

    [Fact]
    public void GetClientHints_ReusesSnapshotForTheSameContext()
    {
        DefaultHttpContext context = new();
        context.Request.Headers["Sec-CH-UA"] = "original";
        context.Request.Headers["Sec-CH-UA-Platform"] = "\"Windows\"";
        context.Items["__HttpClientHints"] = "application data";

        HttpClientHints snapshot = context.GetClientHints();
        context.Request.Headers["Sec-CH-UA"] = "updated";
        context.Request.Headers["Sec-CH-UA-Platform"] = "\"Linux\"";

        Assert.Equal("original", snapshot.UA);
        Assert.Equal("\"Windows\"", snapshot.Platform);
        Assert.Equal("Windows", snapshot.DecodedPlatform);
        Assert.Same(snapshot, context.GetClientHints());
        HttpClientHintsView view = context.GetClientHintsView();
        Assert.Equal("updated", view.UA);
        Assert.Equal("\"Linux\"", view.Platform);
        Assert.Equal("Linux", view.DecodedPlatform);
        Assert.Equal("application data", context.Items["__HttpClientHints"]);
    }
}
