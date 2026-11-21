// Copyright © https://myCSharp.de - all rights reserved

using System.Text;

namespace MyCSharp.HttpClientHints;

/// <summary>
/// Provides methods to interpret HTTP client hints.
/// </summary>
public static class HttpClientHintsInterpreter
{
    /// <summary>
    /// Determines if the provided hint indicates a mobile device.
    /// </summary>
    /// <param name="mobileHeaderValue">The client hint for mobile status, expected to be "?1" for mobile, "?0" for non-mobile, or null if unknown.</param>
    /// <returns>
    /// A nullable boolean value indicating the mobile status:
    /// <list type="bullet">
    /// <item><c>true</c> if the hint indicates a mobile device ("?1").</item>
    /// <item><c>false</c> if the hint indicates a non-mobile device ("?0").</item>
    /// <item><c>null</c> if the hint does not indicate a specific status.</item>
    /// </list>
    /// </returns>
    public static bool? IsMobile(string? mobileHeaderValue)
    {
        if (mobileHeaderValue is not ['?', char flag])
        {
            return null;
        }

        return flag switch
        {
            '1' => true,
            '0' => false,
            _ => null
        };
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
    /// Follows RFC 9651, section 4.2.5. Leading and trailing spaces and tabs are ignored.
    /// A quoted value allocates a new string unless it is empty.
    /// </remarks>
    public static string? DecodeStructuredString(string? headerValue)
    {
        ReadOnlySpan<char> value = headerValue.AsSpan().Trim(" \t");
        if (value.IsEmpty || value[0] != '"')
        {
            return headerValue;
        }

        if (value.Length < 2 || value[^1] != '"')
        {
            return null;
        }

        ReadOnlySpan<char> content = value[1..^1];
        StringBuilder? decoded = null;
        for (int index = 0; index < content.Length; index++)
        {
            char character = content[index];
            if (character == '\\')
            {
                int escapeIndex = index++;
                if (index == content.Length || content[index] is not ('"' or '\\'))
                {
                    return null;
                }

                decoded ??= new StringBuilder(content.Length).Append(content[..escapeIndex]);
                character = content[index];
            }
            else if (character is '"' or < ' ' or > '~')
            {
                return null;
            }

            decoded?.Append(character);
        }

        return decoded?.ToString() ?? content.ToString();
    }
}
