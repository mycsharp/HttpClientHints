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
    public void Equality_DoesNotDependOnDecodedProperties()
    {
        HttpClientHints hints = Create("\"Windows\"", null, null, null);
        HttpClientHints copy = hints with { };

        _ = hints.DecodedPlatform;

        Assert.Equal(hints, copy);
        Assert.Equal(hints.GetHashCode(), copy.GetHashCode());
    }

    [Fact]
    public void Equality_IncludesTheInitProperties()
    {
        HttpClientHints hints = Create(null, null, null, null) with { Bitness = "\"64\"", Wow64 = false, FormFactors = "\"Desktop\"" };

        Assert.Equal(hints, hints with { });
        Assert.NotEqual(hints, hints with { Bitness = "\"32\"" });
        Assert.NotEqual(hints, hints with { Wow64 = true });
        Assert.NotEqual(hints, hints with { FormFactors = "\"XR\"" });
    }

    [Fact]
    public void DecodedProperties_DecodeBitnessFormFactorsAndBrandLists()
    {
        HttpClientHints hints = new(null, null, null, null, null,
            FullVersionList: "\"Chromium\";v=\"142.0.7444.60\", \"Not_A Brand\";v=\"99.0.0.0\"",
            UA: "\"Chromium\";v=\"142\", \"Not_A Brand\";v=\"99\"",
            Mobile: null,
            Headers: new Dictionary<string, StringValues>(StringComparer.Ordinal))
        {
            Bitness = "\"64\"",
            FormFactors = "\"Desktop\", \"XR\"",
        };

        Assert.Equal("64", hints.DecodedBitness);
        Assert.Equal(["Desktop", "XR"], hints.DecodedFormFactors);
        Assert.Equal([new HttpClientHintsBrand("Chromium", "142"), new HttpClientHintsBrand("Not_A Brand", "99")], hints.DecodedUA);
        Assert.Equal(
            [new HttpClientHintsBrand("Chromium", "142.0.7444.60"), new HttpClientHintsBrand("Not_A Brand", "99.0.0.0")],
            hints.DecodedFullVersionList);
    }

    [Fact]
    public void DecodedProperties_ReturnNull_WhenNewValuesAreMissing()
    {
        HttpClientHints hints = Create(null, null, null, null);

        Assert.Null(hints.Bitness);
        Assert.Null(hints.DecodedBitness);
        Assert.Null(hints.Wow64);
        Assert.Null(hints.FormFactors);
        Assert.Null(hints.DecodedFormFactors);
        Assert.Null(hints.DecodedUA);
        Assert.Null(hints.DecodedFullVersionList);
    }
}
