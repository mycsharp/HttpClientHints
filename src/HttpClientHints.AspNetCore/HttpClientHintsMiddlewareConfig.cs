// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// The response header values of <see cref="HttpClientHintsRequestMiddleware"/>, computed by
/// <see cref="HttpClientHintsRegistration.AddHttpClientHints"/>.
/// </summary>
internal sealed class HttpClientHintsMiddlewareConfig
{
    /// <summary>Gets or sets the value of the <c>Accept-CH</c> header.</summary>
    public string? ResponseHeader
    {
        get;
        set
        {
            field = value;
            HasResponseHeaders = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>Gets a value indicating whether <see cref="ResponseHeader"/> is set.</summary>
    public bool HasResponseHeaders { get; private set; }

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
            HasVaryHeader = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>Gets a value indicating whether <see cref="VaryHeader"/> is set.</summary>
    public bool HasVaryHeader { get; private set; }

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
}
