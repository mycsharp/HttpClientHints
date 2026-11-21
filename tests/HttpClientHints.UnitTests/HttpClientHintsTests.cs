// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Primitives;
using Xunit;

namespace MyCSharp.HttpClientHints.UnitTests;

public class HttpClientHintsTests
{
    private static HttpClientHints Create(string? platform, string? platformVersion, string? architecture, string? model)
        => new(null, platform, platformVersion, architecture, model, null, null, null, new Dictionary<string, StringValues>(StringComparer.Ordinal));

    [Fact]
    public void DecodedProperties_DecodeTheHeaderValues()
    {
        HttpClientHints hints = Create("\"Windows\"", "\"19.0.0\"", "\"x86\"", "\"Pixel \\\"9\\\"\"");

        Assert.Equal("\"Windows\"", hints.Platform);
        Assert.Equal("Windows", hints.DecodedPlatform);
        Assert.Equal("\"19.0.0\"", hints.PlatformVersion);
        Assert.Equal("19.0.0", hints.DecodedPlatformVersion);
        Assert.Equal("\"x86\"", hints.Architecture);
        Assert.Equal("x86", hints.DecodedArchitecture);
        Assert.Equal("\"Pixel \\\"9\\\"\"", hints.Model);
        Assert.Equal("Pixel \"9\"", hints.DecodedModel);
    }

    [Fact]
    public void DecodedProperties_ReturnNull_WhenValuesAreMissingOrMalformed()
    {
        HttpClientHints hints = Create(null, "\"19.0.0", "\"x86\";bits=64", "\"Modèle\"");

        Assert.Null(hints.DecodedPlatform);
        Assert.Null(hints.DecodedPlatformVersion);
        Assert.Null(hints.DecodedArchitecture);
        Assert.Null(hints.DecodedModel);
    }

    [Fact]
    public void DecodedProperties_FollowWithExpressions()
    {
        HttpClientHints hints = Create("\"Windows\"", null, null, null);

        HttpClientHints changed = hints with { Platform = "\"Linux\"" };

        Assert.Equal("Windows", hints.DecodedPlatform);
        Assert.Equal("Linux", changed.DecodedPlatform);
    }

    [Fact]
    public void Equality_DependsOnlyOnThePositionalValues()
    {
        HttpClientHints hints = Create("\"Windows\"", null, null, null);
        HttpClientHints copy = hints with { };

        _ = hints.DecodedPlatform;

        Assert.Equal(hints, copy);
        Assert.Equal(hints.GetHashCode(), copy.GetHashCode());
    }
}
