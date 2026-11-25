// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpClientHints.UnitTests;

public class HttpClientHintsInterpreterTests
{
    [Theory]
    [InlineData("?1", true)]
    [InlineData("?0", false)]
    [InlineData("1", null)]
    [InlineData("0", null)]
    [InlineData(null, null)]
    [InlineData("unknown", null)]
    [InlineData("", null)]
    [InlineData("?", null)]
    [InlineData("?2", null)]
    [InlineData("?10", null)]
    [InlineData("!1", null)]
    public void IsMobile_ShouldReturnExpectedResult_ForGivenInput(string? input, bool? expectedResult)
    {
        // Act
        bool? result = HttpClientHintsInterpreter.IsMobile(input);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData("legacy unquoted value", "legacy unquoted value")]
    [InlineData("\"\"", "")]
    [InlineData("\"Windows\"", "Windows")]
    [InlineData(" \t\"model\"\t ", "model")]
    [InlineData("\"  model  \"", "  model  ")]
    [InlineData("\"Model \\\"Pro\\\" \\\\ Edition\"", "Model \"Pro\" \\ Edition")]
    [InlineData("\"\\\\\"", "\\")]
    [InlineData("\" !#[]~\"", " !#[]~")]
    [InlineData("\"", null)]
    [InlineData("\"Model", null)]
    [InlineData("\"Model\" suffix", null)]
    [InlineData("\"Model\\\"", null)]
    [InlineData("\"Model\\q\"", null)]
    [InlineData("\"Model\" \"Pro\"", null)]
    [InlineData("\"Model\tPro\"", null)]
    [InlineData("\"Model\u007F\"", null)]
    [InlineData("\"Modèle\"", null)]
    [InlineData("\"Model\";v=1", null)]
    public void DecodeStructuredString_ShouldReturnExpectedResult_ForGivenInput(string? input, string? expectedResult)
    {
        string? result = HttpClientHintsInterpreter.DecodeStructuredString(input);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void DecodeStructuredString_ReturnsTheSameInstance_ForUnquotedValues()
    {
        string unquoted = "Windows";

        Assert.Same(unquoted, HttpClientHintsInterpreter.DecodeStructuredString(unquoted));
    }

    [Theory]
    [InlineData("?1", true)]
    [InlineData("?0", false)]
    [InlineData(" \t?1\t ", true)]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("?", null)]
    [InlineData("?2", null)]
    [InlineData("?/", null)]
    [InlineData("?1;a=1", null)]
    [InlineData("1", null)]
    [InlineData("\"?1\"", null)]
    public void DecodeStructuredBoolean_ShouldReturnExpectedResult_ForGivenInput(string? input, bool? expectedResult)
    {
        Assert.Equal(expectedResult, HttpClientHintsInterpreter.DecodeStructuredBoolean(input));
    }

    [Fact]
    public void IsMobile_IgnoresSurroundingWhitespace()
    {
        Assert.True(HttpClientHintsInterpreter.IsMobile(" ?1 "));
    }

    [Theory]
    [InlineData("", new string[0])]
    [InlineData("   ", new string[0])]
    [InlineData("\"Desktop\"", new[] { "Desktop" })]
    [InlineData("\"Desktop\", \"XR\"", new[] { "Desktop", "XR" })]
    [InlineData("\"Desktop\",\t\"XR\" ,\"Tablet\"", new[] { "Desktop", "XR", "Tablet" })]
    [InlineData("\"\", \"a\\\"b\"", new[] { "", "a\"b" })]
    public void DecodeStructuredStringList_ReturnsTheStrings(string input, string[] expected)
    {
        Assert.Equal(expected, HttpClientHintsInterpreter.DecodeStructuredStringList(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Desktop")]
    [InlineData("\"Desktop\";a=1")]
    [InlineData("\"Desktop\",")]
    [InlineData("\"Desktop\" \"XR\"")]
    [InlineData("\"Desktop\", ")]
    [InlineData("\"Desk")]
    [InlineData("(\"Desktop\")")]
    public void DecodeStructuredStringList_ReturnsNull_ForMissingOrMalformedValues(string? input)
    {
        Assert.Null(HttpClientHintsInterpreter.DecodeStructuredStringList(input));
    }

    [Fact]
    public void DecodeBrandList_ReturnsBrandsAndVersions()
    {
        IReadOnlyList<HttpClientHintsBrand>? brands = HttpClientHintsInterpreter.DecodeBrandList(
            "\"Chromium\";v=\"142\", \"Google Chrome\";v=\"142\", \"Not_A Brand\";v=\"99\"");

        Assert.Equal(
            [new HttpClientHintsBrand("Chromium", "142"), new HttpClientHintsBrand("Google Chrome", "142"), new HttpClientHintsBrand("Not_A Brand", "99")],
            brands);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData(" \t ", 0)]
    [InlineData("\"Edge\";v=\"142.0.3595.53\"", 1)]
    [InlineData("\"Edge\";v=\"142\";v=\"143\"", 1)]
    [InlineData("\"Edge\"", 1)]
    [InlineData("\"Edge\";v", 1)]
    [InlineData("\"Edge\";v=142", 1)]
    [InlineData("\"Edge\";a=1;v=\"142\"", 1)]
    [InlineData("\"Edge\";v=\"142\";a=1", 1)]
    [InlineData("\"Edge\";  v=\"142\"", 1)]
    [InlineData("\"A\";v=\"1\",\"B\";v=\"2\"", 2)]
    public void DecodeBrandList_ReadsTheMembers(string input, int count)
    {
        Assert.Equal(count, HttpClientHintsInterpreter.DecodeBrandList(input)?.Count);
    }

    [Theory]
    [InlineData("\"Edge\";v=\"142\"", "142")]
    [InlineData("\"Edge\";v=\"142\";v=\"143\"", "143")]
    [InlineData("\"Edge\";v=\"142\";v=143", null)]
    [InlineData("\"Edge\";v", null)]
    [InlineData("\"Edge\";a=\"1\"", null)]
    [InlineData("\"Edge\";a=\"1\";v=\"142\";b=\"2\"", "142")]
    public void DecodeBrandList_UsesTheLastStringValueOfTheVParameter(string input, string? version)
    {
        Assert.Equal(version, Assert.Single(HttpClientHintsInterpreter.DecodeBrandList(input)!).Version);
    }

    [Theory]
    [InlineData("\"Edge\";a=1")]
    [InlineData("\"Edge\";a=-1")]
    [InlineData("\"Edge\";a=1.5")]
    [InlineData("\"Edge\";a=-1.25")]
    [InlineData("\"Edge\";a=?1")]
    [InlineData("\"Edge\";a=?0")]
    [InlineData("\"Edge\";a=token")]
    [InlineData("\"Edge\";a=*token/1:2")]
    [InlineData("\"Edge\";a=:cHJldGVuZA==:")]
    [InlineData("\"Edge\";a=::")]
    [InlineData("\"Edge\";a=@1659578233")]
    [InlineData("\"Edge\";a=%\"f%c3%bc\"")]
    [InlineData("\"Edge\";a=%\"\"")]
    [InlineData("\"Edge\";*a.b-c_1")]
    public void DecodeBrandList_SkipsParametersOfOtherTypes(string input)
    {
        Assert.Equal(new HttpClientHintsBrand("Edge", null), Assert.Single(HttpClientHintsInterpreter.DecodeBrandList(input)!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Edge")]
    [InlineData("\"Edge\";v=\"142\",")]
    [InlineData("\"Edge\";v=\"142\", ")]
    [InlineData("\"Edge\";v=\"142\" \"Chrome\"")]
    [InlineData("(\"Edge\")")]
    [InlineData("\"Edge\";")]
    [InlineData("\"Edge\";V=\"142\"")]
    [InlineData("\"Edge\";1=\"142\"")]
    [InlineData("\"Edge\";v=")]
    [InlineData("\"Edge\";v=\"142")]
    [InlineData("\"Edge\";v=-")]
    [InlineData("\"Edge\";v=-x")]
    [InlineData("\"Edge\";v=1.")]
    [InlineData("\"Edge\";v=?")]
    [InlineData("\"Edge\";v=?2")]
    [InlineData("\"Edge\";v=:abc")]
    [InlineData("\"Edge\";v=:a!:")]
    [InlineData("\"Edge\";v=@")]
    [InlineData("\"Edge\";v=%")]
    [InlineData("\"Edge\";v=%x")]
    [InlineData("\"Edge\";v=%\"abc")]
    [InlineData("\"Edge\";v=%\"ü\"")]
    [InlineData("\"Edge\";v=!")]
    public void DecodeBrandList_ReturnsNull_ForMissingOrMalformedValues(string? input)
    {
        Assert.Null(HttpClientHintsInterpreter.DecodeBrandList(input));
    }

    [Fact]
    public void DecodeBrandList_KeepsGreaseBrands()
    {
        IReadOnlyList<HttpClientHintsBrand> brands = HttpClientHintsInterpreter.DecodeBrandList(
            "\"Google Chrome\";v=\"142\", \"Not_A Brand\";v=\"99\", \"Chromium\";v=\"142\"")!;

        Assert.Equal(["Google Chrome", "Chromium"], brands.Where(static brand => !brand.IsGrease).Select(static brand => brand.Brand), StringComparer.Ordinal);
        Assert.True(brands[1].IsGrease);
    }

    [Fact]
    public void DecodeLists_ReturnOnlyTheMembers_WhenStringsContainCommas()
    {
        IReadOnlyList<HttpClientHintsBrand>? brands = HttpClientHintsInterpreter.DecodeBrandList("\"A, B\";v=\"1,2\", \"C\"");
        IReadOnlyList<string>? strings = HttpClientHintsInterpreter.DecodeStructuredStringList("\"A, B\", \"C,D\"");

        Assert.Equal([new HttpClientHintsBrand("A, B", "1,2"), new HttpClientHintsBrand("C", null)], brands);
        Assert.Equal(["A, B", "C,D"], strings);
    }

    [Theory]
    [InlineData("\"a\\\\é\"")]
    [InlineData("\"a\\\"\tb\"")]
    [InlineData("\"a\\")]
    [InlineData("\"é\\\"\"")]
    public void DecodeStructuredString_ReturnsNull_ForInvalidCharactersAfterAnEscape(string input)
    {
        Assert.Null(HttpClientHintsInterpreter.DecodeStructuredString(input));
    }

    [Fact]
    public void DecodeLists_AllocateLinearly_ForManyStringsWithEscapes()
    {
        // 2,730 members of 6 characters: a header of about 16 KB, half of Kestrel's default limit for all headers
        string header = string.Join(", ", Enumerable.Repeat("\"\\\"\"", 2_730));

        long before = GC.GetAllocatedBytesForCurrentThread();
        IReadOnlyList<HttpClientHintsBrand>? brands = HttpClientHintsInterpreter.DecodeBrandList(header);
        IReadOnlyList<string>? strings = HttpClientHintsInterpreter.DecodeStructuredStringList(header);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(2_730, brands?.Count);
        Assert.Equal(2_730, strings?.Count);
        Assert.All(strings!, static value => Assert.Equal("\"", value));
        Assert.InRange(allocated, 0, 2_000_000);
    }
}
