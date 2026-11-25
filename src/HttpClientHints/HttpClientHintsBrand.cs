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
    private const string GreaseCharacters = " ()-./:;=?_\"\\";
    private const string GreaseLetters = "NotABrand";

    /// <summary>
    /// Gets a value indicating whether <see cref="Brand"/> is a GREASE brand of a Chromium-based browser,
    /// for example <c>Not_A Brand</c>, <c>Not)A;Brand</c> or <c> Not A;Brand</c>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the brand consists of the letters <c>NotABrand</c> in this order and any of the
    /// characters <c> ()-./:;=?_"\</c>; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Browsers add a brand with a changing name to the brand lists so that servers do not depend on a fixed list of
    /// brands (GREASE). This brand does not identify the browser. Chromium-based browsers insert two or three of these
    /// characters around <c>Not</c> and <c>A</c>, depending on their version.
    /// </remarks>
    public bool IsGrease
    {
        get
        {
            int matched = 0;
            foreach (char character in Brand.AsSpan())
            {
                if (GreaseCharacters.Contains(character, StringComparison.Ordinal))
                {
                    continue;
                }

                if (matched == GreaseLetters.Length || character != GreaseLetters[matched])
                {
                    return false;
                }

                matched++;
            }

            return matched == GreaseLetters.Length;
        }
    }
}
