// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpClientHints;

/// <summary>
/// Provides methods to interpret HTTP client hints.
/// </summary>
/// <remarks>
/// Client Hints are HTTP Structured Fields (RFC 9651). The methods ignore leading and trailing spaces and tabs of the
/// header value and return <see langword="null"/> if the header value is <see langword="null"/> or malformed.
/// </remarks>
public static class HttpClientHintsInterpreter
{
    /// <summary>
    /// Determines if the provided hint indicates a mobile device.
    /// </summary>
    /// <param name="mobileHeaderValue">The value of the <c>Sec-CH-UA-Mobile</c> header: <c>?1</c> for mobile, <c>?0</c> for non-mobile.</param>
    /// <returns>The same result as <see cref="DecodeStructuredBoolean"/>.</returns>
    public static bool? IsMobile(string? mobileHeaderValue)
        => DecodeStructuredBoolean(mobileHeaderValue);

    /// <summary>
    /// Decodes a header value that is an HTTP Structured Field boolean, such as <c>Sec-CH-UA-Mobile: ?1</c> or
    /// <c>Sec-CH-UA-WoW64: ?0</c>.
    /// </summary>
    /// <param name="headerValue">The header value as sent.</param>
    /// <returns>
    /// <see langword="true"/> for <c>?1</c>, <see langword="false"/> for <c>?0</c>, or <see langword="null"/> if
    /// <paramref name="headerValue"/> is <see langword="null"/>, another value, or has parameters.
    /// </returns>
    /// <remarks>Follows RFC 9651, section 4.2.8.</remarks>
    public static bool? DecodeStructuredBoolean(string? headerValue)
    {
        if (headerValue is null)
        {
            return null;
        }

        StructuredFieldReader reader = new(headerValue);
        return reader.TryReadBoolean(out bool value) && reader.IsAtEnd ? value : null;
    }

    /// <summary>
    /// Decodes a header value that is an HTTP Structured Field string, such as <c>Sec-CH-UA-Platform: "Windows"</c>.
    /// </summary>
    /// <param name="headerValue">The header value as sent, for example <c>"Windows"</c> including the quotes.</param>
    /// <returns>
    /// The decoded value:
    /// <list type="bullet">
    /// <item>The content of a quoted value with the escapes <c>\"</c> and <c>\\</c> resolved, for example <c>Windows</c>.</item>
    /// <item>The value as sent if it is not quoted.</item>
    /// <item><c>null</c> if <paramref name="headerValue"/> is <c>null</c>, the quoted value is malformed, or the value has parameters.</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Follows RFC 9651, section 4.2.5. A quoted value allocates a new string unless it is empty.
    /// </remarks>
    public static string? DecodeStructuredString(string? headerValue)
    {
        if (headerValue is null)
        {
            return null;
        }

        StructuredFieldReader reader = new(headerValue);
        if (!reader.IsAtString)
        {
            return headerValue;
        }

        return reader.TryReadString(out string? value) && reader.IsAtEnd ? value : null;
    }

    /// <summary>
    /// Decodes a header value that is an HTTP Structured Field list of strings, such as
    /// <c>Sec-CH-UA-Form-Factors: "Desktop", "XR"</c>.
    /// </summary>
    /// <param name="headerValue">The header value as sent.</param>
    /// <returns>
    /// The decoded strings in the order of the header, an empty list for an empty header value, or <see langword="null"/>
    /// if <paramref name="headerValue"/> is <see langword="null"/> or malformed, a member is not a string, or a member has
    /// parameters.
    /// </returns>
    /// <remarks>
    /// Follows RFC 9651, sections 4.2.1 and 4.2.5. Every call returns a new array and the strings; an empty header value
    /// returns an empty array without allocating.
    /// </remarks>
    public static IReadOnlyList<string>? DecodeStructuredStringList(string? headerValue)
    {
        if (headerValue is null)
        {
            return null;
        }

        StructuredFieldReader reader = new(headerValue);
        if (reader.IsAtEnd)
        {
            return [];
        }

        string[] values = new string[GetMaximumListLength(headerValue)];
        int count = 0;
        bool isEnd = false;
        while (!isEnd)
        {
            if (!reader.TryReadString(out string? value) || reader.HasParameters || !reader.TryReadListSeparator(out isEnd))
            {
                return null;
            }

            values[count++] = value;
        }

        return count == values.Length ? values : values[..count];
    }

    /// <summary>
    /// Decodes a brand list such as <c>Sec-CH-UA</c> or <c>Sec-CH-UA-Full-Version-List</c>, for example
    /// <c>"Chromium";v="142", "Google Chrome";v="142", "Not_A Brand";v="99"</c>.
    /// </summary>
    /// <param name="headerValue">The header value as sent.</param>
    /// <returns>
    /// The brands in the order of the header, an empty list for an empty header value, or <see langword="null"/> if
    /// <paramref name="headerValue"/> is <see langword="null"/> or malformed, or a member is not a string.
    /// </returns>
    /// <remarks>
    /// Follows RFC 9651, sections 4.2.1 and 4.2.3. The list keeps GREASE brands; <see cref="HttpClientHintsBrand.IsGrease"/>
    /// identifies them. Parameters other than <c>v</c> are ignored. Every call returns a new array and the strings; an
    /// empty header value returns an empty array without allocating.
    /// </remarks>
    public static IReadOnlyList<HttpClientHintsBrand>? DecodeBrandList(string? headerValue)
    {
        if (headerValue is null)
        {
            return null;
        }

        StructuredFieldReader reader = new(headerValue);
        if (reader.IsAtEnd)
        {
            return [];
        }

        HttpClientHintsBrand[] brands = new HttpClientHintsBrand[GetMaximumListLength(headerValue)];
        int count = 0;
        bool isEnd = false;
        while (!isEnd)
        {
            if (!reader.TryReadString(out string? brand)
                || !reader.TryReadParameters("v", out string? version)
                || !reader.TryReadListSeparator(out isEnd))
            {
                return null;
            }

            brands[count++] = new HttpClientHintsBrand(brand, version);
        }

        return count == brands.Length ? brands : brands[..count];
    }

    // Commas separate the members of a list, so a list has at most one member more than commas.
    // Commas inside strings or parameters only make the result larger than needed.
    private static int GetMaximumListLength(string headerValue)
        => headerValue.AsSpan().Count(',') + 1;
}
