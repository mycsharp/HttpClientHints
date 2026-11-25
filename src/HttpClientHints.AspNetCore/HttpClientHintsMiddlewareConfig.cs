// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// The response header values of <see cref="HttpClientHintsRequestMiddleware"/>, computed by
/// <see cref="HttpClientHintsRegistration.AddHttpClientHints"/>.
/// </summary>
internal sealed class HttpClientHintsMiddlewareConfig
{
    /// <summary>Gets or sets the value of the <c>Accept-CH</c> header.</summary>
    public string? ResponseHeader { get; set; }

    /// <summary>Gets or sets the value of the <c>Critical-CH</c> header.</summary>
    public string? CriticalHeader
    {
        get;
        set
        {
            field = value;
            HasCriticalHeader = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>Gets a value indicating whether <see cref="CriticalHeader"/> is set.</summary>
    public bool HasCriticalHeader { get; private set; }

    /// <summary>Gets or sets the hints that the middleware appends to the <c>Vary</c> header.</summary>
    public string? VaryHeader
    {
        get;
        set
        {
            field = value;
            VaryNames = value?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        }
    }

    /// <summary>Gets a value indicating whether <see cref="VaryHeader"/> is set.</summary>
    public bool HasVaryHeader => VaryNames.Length != 0;

    public string[] VaryNames { get; private set; } = [];

    /// <summary>Gets or sets the value of the obsolete <c>Accept-CH-Lifetime</c> header in seconds.</summary>
    public string? LifeTime
    {
        get;
        set
        {
            field = value;
            HasLifetime = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>Gets a value indicating whether <see cref="LifeTime"/> is set.</summary>
    public bool HasLifetime { get; private set; }

    public void MergeVary(IHeaderDictionary headers)
    {
        if (headers.IsReadOnly)
        {
            return;
        }

        StringValues vary = headers.Vary;
        if (StringValues.IsNullOrEmpty(vary))
        {
            headers.Vary = VaryHeader;
            return;
        }

        if (StringValues.Equals(vary, VaryHeader))
        {
            return;
        }

        if (ContainsVaryName(headers, vary, "*"))
        {
            return;
        }

        foreach (string hint in VaryNames)
        {
            if (!ContainsVaryName(headers, vary, hint))
            {
                headers.Append("Vary", hint);
            }
        }
    }

    private static bool ContainsVaryName(IHeaderDictionary headers, StringValues vary, string name)
    {
        foreach (string? line in vary)
        {
            ReadOnlySpan<char> remaining = line.AsSpan();
            if (remaining.Contains('"'))
            {
                // Preserve the framework's handling of nonstandard quoted field names and embedded commas.
                return headers.GetCommaSeparatedValues("Vary").Contains(name, StringComparer.OrdinalIgnoreCase);
            }

            while (!remaining.IsEmpty)
            {
                int comma = remaining.IndexOf(',');
                ReadOnlySpan<char> field = comma < 0 ? remaining : remaining[..comma];
                if (field.Trim(" \t").Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                remaining = comma < 0 ? default : remaining[(comma + 1)..];
            }
        }

        return false;
    }
}
