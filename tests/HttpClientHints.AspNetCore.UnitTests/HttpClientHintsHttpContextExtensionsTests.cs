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
            { "Sec-CH-UA", new StringValues("TestUA") },
            { "Sec-CH-UA-Platform", new StringValues("TestPlatform") },
            { "Sec-CH-UA-Platform-Version", new StringValues("1.0") },
            { "Sec-CH-UA-Arch", new StringValues("x64") },
            { "Sec-CH-UA-Bitness", new StringValues("64") },
            { "Sec-CH-UA-Full-Version-List", new StringValues("1.0.0.0") },
            { "Sec-CH-UA-Model", new StringValues("TestModel") },
            { "Sec-CH-UA-Mobile", new StringValues("?1") }
        };

        // Act
        HttpClientHints clientHints = headers.GetClientHints();

        // Assert
        Assert.Equal("TestUserAgent", clientHints.UserAgent);
        Assert.Equal("TestPlatform", clientHints.Platform);
        Assert.Equal("1.0", clientHints.PlatformVersion);
        Assert.Equal("x64", clientHints.Architecture);
        Assert.Equal("TestModel", clientHints.Model);
        Assert.Equal("1.0.0.0", clientHints.FullVersionList);
        Assert.Equal("TestUA", clientHints.UA);
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
        Assert.Null(clientHints.PlatformVersion);
        Assert.Null(clientHints.Architecture);
        Assert.Null(clientHints.Model);
        Assert.Null(clientHints.FullVersionList);
        Assert.Null(clientHints.UA);
        Assert.Null(clientHints.Mobile);
        Assert.Equal(headers, clientHints.Headers);
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
}
