// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpClientHints.UnitTests;

public class HttpClientHintsBrandTests
{
    [Theory]
    [InlineData("Not_A Brand")]
    [InlineData("Not)A;Brand")]
    [InlineData("Not/A)Brand")]
    [InlineData("Not A(Brand")]
    [InlineData("Not.A/Brand")]
    [InlineData("Not:A-Brand")]
    [InlineData("Not=A?Brand")]
    [InlineData(" Not A;Brand")]
    [InlineData(" Not;A Brand")]
    [InlineData(";Not A Brand")]
    [InlineData("\"Not\\A;Brand")]
    [InlineData("\\Not\"A;Brand")]
    public void IsGrease_IsTrue_ForGreaseBrands(string brand)
    {
        Assert.True(new HttpClientHintsBrand(brand, "99").IsGrease);
    }

    [Theory]
    [InlineData("Google Chrome")]
    [InlineData("Chromium")]
    [InlineData("Microsoft Edge")]
    [InlineData("")]
    [InlineData("Not_A Brands")]
    [InlineData("Nor_A Brand")]
    [InlineData("Not_B Brand")]
    [InlineData("Not_A Brant")]
    [InlineData("Not_A Bran")]
    [InlineData("NotxA Brand")]
    [InlineData("Not_AxBrand")]
    [InlineData("Not_A Brand!")]
    [InlineData("Not A Brand 2")]
    public void IsGrease_IsFalse_ForOtherBrands(string brand)
    {
        Assert.False(new HttpClientHintsBrand(brand, "142").IsGrease);
    }

    [Fact]
    public void IsGrease_IsFalse_ForTheDefaultValue()
    {
        Assert.False(default(HttpClientHintsBrand).IsGrease);
    }

    [Fact]
    public void Equality_ComparesBrandAndVersion()
    {
        Assert.Equal(new HttpClientHintsBrand("Chromium", "142"), new HttpClientHintsBrand("Chromium", "142"));
        Assert.NotEqual(new HttpClientHintsBrand("Chromium", "142"), new HttpClientHintsBrand("Chromium", "143"));
    }
}
