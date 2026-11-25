// Copyright © https://myCSharp.de - all rights reserved

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MyCSharp.HttpClientHints;

/// <summary>
/// Reads the parts of an HTTP Structured Field value (RFC 9651) that Client Hints use: strings, booleans,
/// lists and parameters.
/// </summary>
/// <remarks>
/// Leading and trailing spaces and tabs of the field value are ignored. Parameter values of other types
/// (integers, decimals, tokens, byte sequences, dates and display strings) are checked for their basic form and skipped.
/// </remarks>
internal ref struct StructuredFieldReader
{
    private static readonly SearchValues<char> s_keyCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyz0123456789_-.*");
    private static readonly SearchValues<char> s_tokenCharacters = SearchValues.Create(
        "!#$%&'*+-.^_`|~0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz:/");
    private static readonly SearchValues<char> s_base64Characters = SearchValues.Create(
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=");

    private const string KeyStartCharacters = "abcdefghijklmnopqrstuvwxyz*";

    private readonly ReadOnlySpan<char> _value;
    private int _position;

    public StructuredFieldReader(string value)
    {
        _value = value.AsSpan().Trim(" \t");
        _position = 0;
    }

    public readonly bool IsAtEnd => _position == _value.Length;

    private readonly char Current => _value[_position];

    /// <summary>Gets a value indicating whether a string starts at the current position.</summary>
    public readonly bool IsAtString => !IsAtEnd && Current == '"';

    /// <summary>Reads a string such as <c>"Windows"</c> and resolves the escapes <c>\"</c> and <c>\\</c>.</summary>
    public bool TryReadString([NotNullWhen(true)] out string? value)
    {
        value = null;
        if (!IsAtString)
        {
            return false;
        }

        int start = ++_position;
        StringBuilder? decoded = null;
        while (!IsAtEnd)
        {
            char character = _value[_position++];
            if (character == '"')
            {
                value = decoded?.ToString() ?? _value[start..(_position - 1)].ToString();
                return true;
            }

            if (character == '\\')
            {
                if (IsAtEnd || Current is not ('"' or '\\'))
                {
                    return false;
                }

                // The default capacity grows with the string: sizing it to the header would allocate
                // the header length for every escaped string of a list.
                decoded ??= new StringBuilder().Append(_value[start..(_position - 1)]);
                character = _value[_position++];
            }
            else if ((uint)(character - ' ') > '~' - ' ')
            {
                return false;
            }

            decoded?.Append(character);
        }

        return false;
    }

    /// <summary>Reads a boolean, <c>?1</c> or <c>?0</c>.</summary>
    public bool TryReadBoolean(out bool value)
    {
        value = false;
        if (_value.Length - _position < 2 || Current != '?')
        {
            return false;
        }

        char flag = _value[_position + 1];
        _position += 2;
        value = flag == '1';
        return (uint)(flag - '0') <= 1;
    }

    /// <summary>Gets a value indicating whether parameters follow at the current position.</summary>
    public readonly bool HasParameters => !IsAtEnd && Current == ';';

    /// <summary>
    /// Reads the parameters at the current position and returns the string value of the parameter <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the parameter to return.</param>
    /// <param name="value">
    /// The value of the last parameter <paramref name="key"/> if it is a string; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="false"/> if the parameters are malformed.</returns>
    public bool TryReadParameters(string key, out string? value)
    {
        value = null;
        while (HasParameters)
        {
            _position++;
            SkipSpaces();
            if (!TryReadKey(out ReadOnlySpan<char> parameterKey))
            {
                return false;
            }

            string? stringValue = null;
            if (!IsAtEnd && Current == '=')
            {
                _position++;
                if (!TryReadBareItem(out stringValue))
                {
                    return false;
                }
            }

            if (parameterKey.SequenceEqual(key))
            {
                value = stringValue;
            }
        }

        return true;
    }

    /// <summary>
    /// Reads the separator after a list member: optional whitespace and a comma followed by optional whitespace, or the end.
    /// </summary>
    /// <param name="isEnd"><see langword="true"/> if the list ends after the member.</param>
    /// <returns><see langword="false"/> if the list is malformed, for example with a trailing comma.</returns>
    public bool TryReadListSeparator(out bool isEnd)
    {
        SkipWhitespace();
        isEnd = IsAtEnd;
        if (isEnd)
        {
            return true;
        }

        if (Current != ',')
        {
            return false;
        }

        _position++;
        SkipWhitespace();
        return !IsAtEnd;
    }

    private bool TryReadKey(out ReadOnlySpan<char> key)
    {
        key = default;
        if (IsAtEnd || !KeyStartCharacters.Contains(Current, StringComparison.Ordinal))
        {
            return false;
        }

        int start = _position++;
        SkipAll(s_keyCharacters);
        key = _value[start.._position];
        return true;
    }

    private bool TryReadBareItem(out string? stringValue)
    {
        stringValue = null;
        if (IsAtEnd)
        {
            return false;
        }

        switch (Current)
        {
            case '"':
                return TryReadString(out stringValue);
            case '?':
                return TryReadBoolean(out _);
            case ':':
                return TrySkipByteSequence();
            case '@':
                _position++;
                return TrySkipNumber();
            case '%':
                return TrySkipDisplayString();
        }

        if (Current == '-' || char.IsAsciiDigit(Current))
        {
            return TrySkipNumber();
        }

        if (Current == '*' || char.IsAsciiLetter(Current))
        {
            _position++;
            SkipAll(s_tokenCharacters);
            return true;
        }

        return false;
    }

    private bool TrySkipNumber()
    {
        if (!IsAtEnd && Current == '-')
        {
            _position++;
        }

        if (!TrySkipDigits())
        {
            return false;
        }

        if (!IsAtEnd && Current == '.')
        {
            _position++;
            return TrySkipDigits();
        }

        return true;
    }

    private bool TrySkipDigits()
    {
        int start = _position;
        ReadOnlySpan<char> rest = _value[_position..];
        int length = rest.IndexOfAnyExceptInRange('0', '9');
        _position += length < 0 ? rest.Length : length;
        return _position > start;
    }

    private bool TrySkipByteSequence()
    {
        ReadOnlySpan<char> rest = _value[(_position + 1)..];
        int end = rest.IndexOfAnyExcept(s_base64Characters);
        if (end < 0 || rest[end] != ':')
        {
            return false;
        }

        _position += end + 2;
        return true;
    }

    private bool TrySkipDisplayString()
    {
        ReadOnlySpan<char> rest = _value[(_position + 1)..];
        if (rest.IsEmpty || rest[0] != '"')
        {
            return false;
        }

        int end = rest[1..].IndexOf('"');
        if (end < 0 || rest.Slice(1, end).IndexOfAnyExceptInRange(' ', '~') >= 0)
        {
            return false;
        }

        _position += end + 3;
        return true;
    }

    private void SkipAll(SearchValues<char> characters)
    {
        ReadOnlySpan<char> rest = _value[_position..];
        int length = rest.IndexOfAnyExcept(characters);
        _position += length < 0 ? rest.Length : length;
    }

    private void SkipSpaces()
    {
        ReadOnlySpan<char> rest = _value[_position..];
        int length = rest.IndexOfAnyExcept(' ');
        _position += length < 0 ? rest.Length : length;
    }

    private void SkipWhitespace()
    {
        ReadOnlySpan<char> rest = _value[_position..];
        int length = rest.IndexOfAnyExcept(' ', '\t');
        _position += length < 0 ? rest.Length : length;
    }
}
