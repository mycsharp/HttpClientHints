// Copyright © https://myCSharp.de - all rights reserved

using System.Buffers;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Extension methods for registering and configuring HTTP Client Hints middleware.
/// </summary>
public static class HttpClientHintsRegistration
{
    // Characters of an HTTP header name (token, RFC 9110 section 5.6.2)
    private static readonly SearchValues<char> s_headerNameCharacters = SearchValues.Create(
        "!#$%&'*+-.^_`|~0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

    /// <summary>
    /// Configures the response headers that <see cref="UseHttpClientHints"/> sets.
    /// </summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="config">An optional action to configure <see cref="HttpClientHintsOptions"/>.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="HttpClientHintsOptions.Additional"/> or <see cref="HttpClientHintsOptions.Critical"/> contains an entry
    /// that is not a valid header name, for example one with a space or a comma.
    /// </exception>
    /// <remarks>
    /// The options are read once, when this method is called. Without <paramref name="config"/>, <c>Accept-CH</c>
    /// requests the low-entropy hints <c>Sec-CH-UA</c> and <c>Sec-CH-UA-Mobile</c>.
    /// </remarks>
    public static IServiceCollection AddHttpClientHints(this IServiceCollection services, Action<HttpClientHintsOptions>? config = null)
    {
        HttpClientHintsOptions options = new();
        config?.Invoke(options);

        List<string> hints = [];
        if (options.UserAgent)
        {
            AddHint(hints, "Sec-CH-UA");
        }

        if (options.Platform)
        {
            AddHint(hints, "Sec-CH-UA-Platform");
            AddHint(hints, "Sec-CH-UA-Platform-Version");
        }

        if (options.Architecture)
        {
            AddHint(hints, "Sec-CH-UA-Arch");
            AddHint(hints, "Sec-CH-UA-Bitness");
            AddHint(hints, "Sec-CH-UA-WoW64");
        }

        if (options.Device)
        {
            AddHint(hints, "Sec-CH-UA-Model");
            AddHint(hints, "Sec-CH-UA-Form-Factors");
        }

        if (options.Mobile)
        {
            AddHint(hints, "Sec-CH-UA-Mobile");
        }

        if (options.FullVersionList)
        {
            AddHint(hints, "Sec-CH-UA-Full-Version-List");
        }

        if (AddHints(hints, options.Additional) is string invalidAdditional)
        {
            throw new ArgumentException(
                $"'{invalidAdditional}' in {nameof(HttpClientHintsOptions)}.{nameof(HttpClientHintsOptions.Additional)} is not a valid header name.", nameof(config));
        }

        List<string> critical = [];
        if (AddHints(critical, options.Critical) is string invalidCritical)
        {
            throw new ArgumentException(
                $"'{invalidCritical}' in {nameof(HttpClientHintsOptions)}.{nameof(HttpClientHintsOptions.Critical)} is not a valid header name.", nameof(config));
        }

        foreach (string hint in critical)
        {
            AddHint(hints, hint);
        }

        string acceptCH = string.Join(", ", hints);
        string criticalCH = string.Join(", ", critical);
        string vary = options.Vary ? acceptCH : criticalCH;
        if (vary.Length > 0)
        {
            services.Configure<MvcOptions>(static mvc => mvc.Filters.Add(new HttpClientHintsVaryFilterAttribute()));
        }

#pragma warning disable CS0618 // Lifetime is obsolete, but still sends Accept-CH-Lifetime
        // Accept-CH-Lifetime is a number of whole seconds
        string? lifetime = options.Lifetime is TimeSpan value ? ((long)value.TotalSeconds).ToString(CultureInfo.InvariantCulture) : null;
#pragma warning restore CS0618

        services.Configure<HttpClientHintsMiddlewareConfig>(o =>
        {
            o.ResponseHeader = acceptCH;
            o.CriticalHeader = criticalCH;
            o.VaryHeader = vary;
            o.LifeTime = lifetime;
        });

        return services;
    }

    /// <summary>
    /// Adds a middleware that sets the <c>Accept-CH</c> response header configured with <see cref="AddHttpClientHints"/>,
    /// and <c>Critical-CH</c> and <c>Vary</c> if configured.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder with the middleware added.</returns>
    /// <remarks>Without <see cref="AddHttpClientHints"/>, the middleware sets no header.</remarks>
    public static IApplicationBuilder UseHttpClientHints(this IApplicationBuilder app)
    {
        app.UseMiddleware<HttpClientHintsRequestMiddleware>();

        return app;
    }

    /// <summary>
    /// Adds a middleware that registers <see cref="IHttpClientHintsFeature"/> in <c>HttpContext.Features</c> for every request.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder with the middleware added.</returns>
    /// <remarks>
    /// Middleware and handlers added after this call can read the feature. The middleware does not require
    /// <see cref="AddHttpClientHints"/> or <see cref="UseHttpClientHints"/>. It creates the feature for every request,
    /// even if no handler reads it, and replaces an <see cref="IHttpClientHintsFeature"/> registered earlier in the pipeline.
    /// </remarks>
    public static IApplicationBuilder UseHttpClientHintsFeature(this IApplicationBuilder app)
    {
        app.Use(static next => new HttpClientHintsFeatureMiddleware(next).InvokeAsync);

        return app;
    }

    /// <summary>
    /// Adds the trimmed names that are not in <paramref name="hints"/> yet, ignoring case and empty entries.
    /// </summary>
    /// <returns>The first entry that is not a valid header name, or <see langword="null"/>.</returns>
    private static string? AddHints(List<string> hints, string?[]? names)
    {
        foreach (string? entry in names ?? [])
        {
            string name = (entry ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                continue;
            }

            // Accept-CH is a Structured Field list of tokens: one invalid entry makes browsers ignore the whole header
            if (!char.IsAsciiLetter(name[0]) || name.AsSpan().ContainsAnyExcept(s_headerNameCharacters))
            {
                return entry;
            }

            AddHint(hints, name);
        }

        return null;
    }

    private static void AddHint(List<string> hints, string name)
    {
        if (!hints.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            hints.Add(name);
        }
    }
}
