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
}
