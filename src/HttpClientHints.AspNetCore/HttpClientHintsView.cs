// Copyright © https://myCSharp.de - all rights reserved

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// A view over <see cref="IHeaderDictionary"/> to access HTTP Client Hints lazily.
/// </summary>
/// <remarks>
/// Creating the view and reading raw values do not allocate. Reading a decoded quoted value can allocate a string.
/// Use <see cref="BuildSnapshot"/> to capture raw and decoded values in a <see cref="HttpClientHints"/> record.
/// </remarks>
/// <remarks>
/// Creates a new view over the given headers.
/// </remarks>
public readonly struct HttpClientHintsView(IHeaderDictionary headers)
{
    /// <summary>Gets the raw headers.</summary>
    public IHeaderDictionary Headers => headers;

    /// <summary>User-Agent</summary>
    public string? UserAgent
        => GetFirstValue("User-Agent");

    /// <summary>Sec-CH-UA</summary>
    public string? UA
        => GetFirstValue("Sec-CH-UA");

    /// <summary>Gets the decoded Sec-CH-UA-Platform value.</summary>
    /// <value>
    /// The unescaped content of a quoted value, an unquoted value as sent, or <see langword="null"/> if the header is
    /// missing, the quoted value is malformed, or the value has parameters.
    /// </value>
    public string? Platform
        => DecodeString(RawPlatform);

    /// <summary>Gets the original Sec-CH-UA-Platform value, including any quotes and escapes.</summary>
    public string? RawPlatform
        => GetFirstValue("Sec-CH-UA-Platform");

    /// <summary>Gets the decoded Sec-CH-UA-Platform-Version value.</summary>
    /// <value>
    /// The unescaped content of a quoted value, an unquoted value as sent, or <see langword="null"/> if the header is
    /// missing, the quoted value is malformed, or the value has parameters.
    /// </value>
    public string? PlatformVersion
        => DecodeString(RawPlatformVersion);

    /// <summary>Gets the original Sec-CH-UA-Platform-Version value, including any quotes and escapes.</summary>
    public string? RawPlatformVersion
        => GetFirstValue("Sec-CH-UA-Platform-Version");

    /// <summary>Gets the decoded Sec-CH-UA-Arch value.</summary>
    /// <value>
    /// The unescaped content of a quoted value, an unquoted value as sent, or <see langword="null"/> if the header is
    /// missing, the quoted value is malformed, or the value has parameters.
    /// </value>
    public string? Architecture
        => DecodeString(RawArchitecture);

    /// <summary>Gets the original Sec-CH-UA-Arch value, including any quotes and escapes.</summary>
    public string? RawArchitecture
        => GetFirstValue("Sec-CH-UA-Arch");

    /// <summary>Gets the decoded Sec-CH-UA-Model value.</summary>
    /// <value>
    /// The unescaped content of a quoted value, an unquoted value as sent, or <see langword="null"/> if the header is
    /// missing, the quoted value is malformed, or the value has parameters.
    /// </value>
    public string? Model
        => DecodeString(RawModel);

    /// <summary>Gets the original Sec-CH-UA-Model value, including any quotes and escapes.</summary>
    public string? RawModel
        => GetFirstValue("Sec-CH-UA-Model");

    /// <summary>Sec-CH-UA-Full-Version-List</summary>
    public string? FullVersionList
        => GetFirstValue("Sec-CH-UA-Full-Version-List");

    /// <summary>Sec-CH-UA-Mobile mapped to nullable bool.</summary>
    public bool? Mobile
        => HttpClientHintsInterpreter.IsMobile(GetFirstValue("Sec-CH-UA-Mobile"));

    /// <summary>
    /// Materializes a snapshot record of the current raw and decoded values.
    /// </summary>
    public HttpClientHints BuildSnapshot()
    {
        string? rawPlatform = RawPlatform;
        string? rawPlatformVersion = RawPlatformVersion;
        string? rawArchitecture = RawArchitecture;
        string? rawModel = RawModel;

        return new HttpClientHints(UserAgent, DecodeString(rawPlatform), DecodeString(rawPlatformVersion),
            DecodeString(rawArchitecture), DecodeString(rawModel), FullVersionList, UA, Mobile, headers)
        {
            RawPlatform = rawPlatform,
            RawPlatformVersion = rawPlatformVersion,
            RawArchitecture = rawArchitecture,
            RawModel = rawModel
        };
    }

    private string? GetFirstValue(string key)
        => headers.TryGetValue(key, out StringValues values) && values.Count > 0 ? values[0] : null;

    private static string? DecodeString(string? rawValue)
    {
        ReadOnlySpan<char> value = rawValue.AsSpan().Trim(" \t");
        if (value.IsEmpty || value[0] != '"')
        {
            return rawValue;
        }

        if (value.Length < 2 || value[^1] != '"')
        {
            return null;
        }

        // RFC 9651 section 4.2.5 defines the quoted string and its two permitted escapes.
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
