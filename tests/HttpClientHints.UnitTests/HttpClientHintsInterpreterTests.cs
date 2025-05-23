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
    public void IsMobile_ShouldReturnExpectedResult_ForGivenInput(string? input, bool? expectedResult)
    {
        // Act
        bool? result = HttpClientHintsInterpreter.IsMobile(input);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
