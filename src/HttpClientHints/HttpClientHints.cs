// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Represents HTTP Client Hints retrieved from the request headers.
/// </summary>
/// <param name="UserAgent">The User-Agent request header.</param>
/// <param name="Platform">The Sec-CH-UA-Platform header value as sent, including quotes. <see cref="DecodedPlatform"/> returns it without quotes.</param>
/// <param name="PlatformVersion">The Sec-CH-UA-Platform-Version header value as sent, including quotes. <see cref="DecodedPlatformVersion"/> returns it without quotes.</param>
/// <param name="Architecture">The Sec-CH-UA-Arch header value as sent, including quotes. <see cref="DecodedArchitecture"/> returns it without quotes.</param>
/// <param name="Model">The Sec-CH-UA-Model header value as sent, including quotes. <see cref="DecodedModel"/> returns it without quotes.</param>
/// <param name="FullVersionList">The Sec-CH-UA-Full-Version-List header value as sent. <see cref="DecodedFullVersionList"/> returns the brands.</param>
/// <param name="UA">The Sec-CH-UA header value as sent. <see cref="DecodedUA"/> returns the brands.</param>
/// <param name="Mobile">The Sec-CH-UA-Mobile header value decoded with <see cref="HttpClientHintsInterpreter.IsMobile"/>.</param>
/// <param name="Headers">The original headers dictionary containing all the request headers.</param>
/// <remarks>
/// The <c>Decoded*</c> properties decode the matching property with the methods of <see cref="HttpClientHintsInterpreter"/>
/// on the first access and return the same result on every further access. The decoded values are not part of record
/// equality, and a record created with a <c>with</c> expression decodes its own values. Concurrent first accesses can
/// decode a value more than once; they return equal results.
/// </remarks>
public record class HttpClientHints(string? UserAgent, string? Platform, string? PlatformVersion,
    string? Architecture, string? Model, string? FullVersionList, string? UA, bool? Mobile, IDictionary<string, StringValues> Headers)
{
    private static readonly Func<string?, string?> s_decodeString = HttpClientHintsInterpreter.DecodeStructuredString;
    private static readonly Func<string?, IReadOnlyList<string>?> s_decodeStringList = HttpClientHintsInterpreter.DecodeStructuredStringList;
    private static readonly Func<string?, IReadOnlyList<HttpClientHintsBrand>?> s_decodeBrandList = HttpClientHintsInterpreter.DecodeBrandList;

    private DecodedValuesField _decoded;

    /// <summary>Copies the header values into a snapshot with an empty decoder cache.</summary>
    protected HttpClientHints(HttpClientHints original)
    {
        UserAgent = original.UserAgent;
        Platform = original.Platform;
        PlatformVersion = original.PlatformVersion;
        Architecture = original.Architecture;
        Model = original.Model;
        FullVersionList = original.FullVersionList;
        UA = original.UA;
        Mobile = original.Mobile;
        Headers = original.Headers;
        Bitness = original.Bitness;
        Wow64 = original.Wow64;
        FormFactors = original.FormFactors;
    }

    /// <summary>Gets the Sec-CH-UA-Bitness header value as sent, including quotes, for example <c>"64"</c>.</summary>
    /// <value>The header value, or <see langword="null"/> if the header is missing. <see cref="DecodedBitness"/> returns it without quotes.</value>
    public string? Bitness { get; init; }

    /// <summary>Gets the Sec-CH-UA-WoW64 header value decoded with <see cref="HttpClientHintsInterpreter.DecodeStructuredBoolean"/>.</summary>
    /// <value>
    /// <see langword="true"/> if a 32-bit browser runs on 64-bit Windows, <see langword="false"/> if not, or
    /// <see langword="null"/> if the header is missing or malformed.
    /// </value>
    public bool? Wow64 { get; init; }

    /// <summary>Gets the Sec-CH-UA-Form-Factors header value as sent, for example <c>"Desktop"</c>.</summary>
    /// <value>The header value, or <see langword="null"/> if the header is missing. <see cref="DecodedFormFactors"/> returns the form factors.</value>
    public string? FormFactors { get; init; }

    /// <summary>Gets <see cref="UA"/> decoded into its brands.</summary>
    /// <value>The brands, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeBrandList"/>.</value>
    public IReadOnlyList<HttpClientHintsBrand>? DecodedUA
        => DecodedValues.Get(ref Decoded.UA, UA, s_decodeBrandList);

    /// <summary>Gets <see cref="FullVersionList"/> decoded into its brands with their full versions.</summary>
    /// <value>The brands, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeBrandList"/>.</value>
    public IReadOnlyList<HttpClientHintsBrand>? DecodedFullVersionList
        => DecodedValues.Get(ref Decoded.FullVersionList, FullVersionList, s_decodeBrandList);

    /// <summary>Gets <see cref="Bitness"/> decoded, for example <c>64</c> instead of <c>"64"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedBitness
        => DecodedValues.Get(ref Decoded.Bitness, Bitness, s_decodeString);

    /// <summary>Gets <see cref="FormFactors"/> decoded, for example <c>Desktop</c> instead of <c>"Desktop"</c>.</summary>
    /// <value>The form factors, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredStringList"/>.</value>
    public IReadOnlyList<string>? DecodedFormFactors
        => DecodedValues.Get(ref Decoded.FormFactors, FormFactors, s_decodeStringList);

    /// <summary>Gets <see cref="Platform"/> decoded, for example <c>Windows</c> instead of <c>"Windows"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatform
        => DecodedValues.Get(ref Decoded.Platform, Platform, s_decodeString);

    /// <summary>Gets <see cref="PlatformVersion"/> decoded, for example <c>19.0.0</c> instead of <c>"19.0.0"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedPlatformVersion
        => DecodedValues.Get(ref Decoded.PlatformVersion, PlatformVersion, s_decodeString);

    /// <summary>Gets <see cref="Architecture"/> decoded, for example <c>x86</c> instead of <c>"x86"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedArchitecture
        => DecodedValues.Get(ref Decoded.Architecture, Architecture, s_decodeString);

    /// <summary>Gets <see cref="Model"/> decoded, for example <c>Pixel 9</c> instead of <c>"Pixel 9"</c>.</summary>
    /// <value>The decoded value, or <see langword="null"/> as described in <see cref="HttpClientHintsInterpreter.DecodeStructuredString"/>.</value>
    public string? DecodedModel
        => DecodedValues.Get(ref Decoded.Model, Model, s_decodeString);

    private DecodedValues Decoded => LazyInitializer.EnsureInitialized(ref _decoded.Values);

    /// <summary>
    /// Holds the decoded values. Its equality is always <see langword="true"/>, so it does not change the equality of the record.
    /// </summary>
    private struct DecodedValuesField : IEquatable<DecodedValuesField>
    {
        public DecodedValues? Values;

        public readonly bool Equals(DecodedValuesField other) => true;

        [ExcludeFromCodeCoverage(Justification = "The record compares the field with Equals(DecodedValuesField).")]
        public override readonly bool Equals(object? obj) => obj is DecodedValuesField;

        public override readonly int GetHashCode() => 0;
    }

    /// <summary>
    /// The decoded values of one record. A field is <see langword="null"/> before the first access, and
    /// <see cref="s_null"/> if the decoded value is <see langword="null"/>.
    /// </summary>
    private sealed class DecodedValues
    {
        private static readonly object s_null = new();

        public object? UA;
        public object? FullVersionList;
        public object? Bitness;
        public object? FormFactors;
        public object? Platform;
        public object? PlatformVersion;
        public object? Architecture;
        public object? Model;

        public static T? Get<T>(ref object? field, string? headerValue, Func<string?, T?> decode)
            where T : class
        {
            object? value = Volatile.Read(ref field);
            if (value is null)
            {
                value = decode(headerValue) ?? s_null;
                Volatile.Write(ref field, value);
            }

            return ReferenceEquals(value, s_null) ? null : (T)value;
        }
    }
}
