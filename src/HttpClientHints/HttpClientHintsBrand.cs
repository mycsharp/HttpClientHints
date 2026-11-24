// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpClientHints;

/// <summary>
/// A member of a brand list such as <c>Sec-CH-UA</c> or <c>Sec-CH-UA-Full-Version-List</c>.
/// </summary>
/// <param name="Brand">The brand, for example <c>Google Chrome</c>.</param>
/// <param name="Version">
/// The value of the <c>v</c> parameter, for example <c>142</c> in <c>Sec-CH-UA</c> or <c>142.0.7444.60</c> in
/// <c>Sec-CH-UA-Full-Version-List</c>, or <see langword="null"/> if the member has no <c>v</c> parameter with a string value.
/// </param>
public readonly record struct HttpClientHintsBrand(string Brand, string? Version)
{
    private const string GreaseCharacters = " ()-./:;=?_";

    /// <summary>
    /// Gets a value indicating whether <see cref="Brand"/> is a GREASE brand of a Chromium-based browser,
    /// for example <c>Not_A Brand</c> or <c>Not)A;Brand</c>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the brand, without leading spaces, consists of <c>Not</c>, one of the characters
    /// <c> ()-./:;=?_</c>, <c>A</c>, one of these characters and <c>Brand</c>; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Browsers add a brand with a changing name to the brand lists so that servers do not depend on a fixed list of
    /// brands (GREASE). This brand does not identify the browser.
    /// </remarks>
    public bool IsGrease
    {
        get
        {
            ReadOnlySpan<char> brand = Brand.AsSpan().TrimStart(' ');
            return brand.Length == 11
                && brand.StartsWith("Not", StringComparison.Ordinal)
                && brand[4] == 'A'
                && brand.EndsWith("Brand", StringComparison.Ordinal)
                && GreaseCharacters.Contains(brand[3], StringComparison.Ordinal)
                && GreaseCharacters.Contains(brand[5], StringComparison.Ordinal);
        }
    }
}
