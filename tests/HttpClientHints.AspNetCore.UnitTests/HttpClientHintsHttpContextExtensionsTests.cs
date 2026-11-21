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
        Assert.Equal("Windows", clientHints.Platform);
        Assert.Equal("\"Windows\"", clientHints.RawPlatform);
        Assert.Equal("19.0.0", clientHints.PlatformVersion);
        Assert.Equal("\"19.0.0\"", clientHints.RawPlatformVersion);
        Assert.Equal("x86", clientHints.Architecture);
        Assert.Equal("\"x86\"", clientHints.RawArchitecture);
        Assert.Equal(string.Empty, clientHints.Model);
        Assert.Equal("\"\"", clientHints.RawModel);
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
        Assert.Null(clientHints.RawPlatform);
        Assert.Null(clientHints.PlatformVersion);
        Assert.Null(clientHints.RawPlatformVersion);
        Assert.Null(clientHints.Architecture);
        Assert.Null(clientHints.RawArchitecture);
        Assert.Null(clientHints.Model);
        Assert.Null(clientHints.RawModel);
        Assert.Null(clientHints.FullVersionList);
        Assert.Null(clientHints.UA);
        Assert.Null(clientHints.Mobile);
        Assert.Equal(headers, clientHints.Headers);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("legacy unquoted value", "legacy unquoted value")]
    [InlineData("\"\"", "")]
    [InlineData(" \t\"model\"\t ", "model")]
    [InlineData("\"  model  \"", "  model  ")]
    [InlineData("\"Model \\\"Pro\\\" \\\\ Edition\"", "Model \"Pro\" \\ Edition")]
    [InlineData("\"Model", null)]
    [InlineData("\"Model\" suffix", null)]
    [InlineData("\"Model\\\"", null)]
    [InlineData("\"Model\\q\"", null)]
    [InlineData("\"Model\" \"Pro\"", null)]
    [InlineData("\"Model\tPro\"", null)]
    [InlineData("\"Modèle\"", null)]
    [InlineData("   ", "   ")]
    [InlineData("\"", null)]
    [InlineData("\"\\\\\"", "\\")]
    [InlineData("\" !#[]~\"", " !#[]~")]
    [InlineData("\"Model\u007F\"", null)]
    [InlineData("\"Model\";v=1", null)]
    public void GetClientHints_ExposesDecodedAndRawStructuredStrings(string? rawModel, string? model)
    {
        HeaderDictionary headers = [];
        if (rawModel is not null)
        {
            headers["Sec-CH-UA-Model"] = rawModel;
        }

        HttpClientHintsView view = headers.GetClientHintsView();
        HttpClientHints snapshot = headers.GetClientHints();

        Assert.Equal(model, view.Model);
        Assert.Equal(rawModel, view.RawModel);
        Assert.Equal(model, snapshot.Model);
        Assert.Equal(rawModel, snapshot.RawModel);
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
        Assert.Equal("Windows", view.Platform);
        Assert.Equal("\"Windows\"", view.RawPlatform);
        Assert.Equal("19.0.0", view.PlatformVersion);
        Assert.Equal("\"19.0.0\"", view.RawPlatformVersion);
        Assert.Equal("arm", view.Architecture);
        Assert.Equal("\"arm\"", view.RawArchitecture);
        Assert.Equal("Pixel \"9\"", view.Model);
        Assert.Equal("\"Pixel \\\"9\\\"\"", view.RawModel);
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
        Assert.Null(view.RawPlatform);
        Assert.Equal(string.Empty, view.Model);
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
        Assert.Equal("Windows", snapshot.Platform);
        Assert.Equal("\"Windows\"", snapshot.RawPlatform);
        Assert.Same(snapshot, context.GetClientHints());
        HttpClientHintsView view = context.GetClientHintsView();
        Assert.Equal("updated", view.UA);
        Assert.Equal("Linux", view.Platform);
        Assert.Equal("\"Linux\"", view.RawPlatform);
        Assert.Equal("application data", context.Items["__HttpClientHints"]);
    }
}
