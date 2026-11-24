// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Extension methods for registering and configuring HTTP Client Hints middleware.
/// </summary>
public static class HttpClientHintsRegistration
{
    /// <summary>
    /// Configures the response headers that <see cref="UseHttpClientHints"/> sets.
    /// </summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="config">An optional action to configure <see cref="HttpClientHintsOptions"/>.</param>
    /// <returns>The service collection.</returns>
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
            AddHints(hints, "Sec-CH-UA");
        }

        if (options.Platform)
        {
            AddHints(hints, "Sec-CH-UA-Platform", "Sec-CH-UA-Platform-Version");
        }

        if (options.Architecture)
        {
            AddHints(hints, "Sec-CH-UA-Arch", "Sec-CH-UA-Bitness", "Sec-CH-UA-WoW64");
        }

        if (options.Device)
        {
            AddHints(hints, "Sec-CH-UA-Model", "Sec-CH-UA-Form-Factors");
        }

        if (options.Mobile)
        {
            AddHints(hints, "Sec-CH-UA-Mobile");
        }

        if (options.FullVersionList)
        {
            AddHints(hints, "Sec-CH-UA-Full-Version-List");
        }

        AddHints(hints, options.Additional);

        List<string> critical = [];
        AddHints(critical, options.Critical);
        AddHints(hints, critical);

        string acceptCH = string.Join(", ", hints);
        string criticalCH = string.Join(", ", critical);
        string vary = options.Vary ? acceptCH : criticalCH;

#pragma warning disable CS0618 // Lifetime is obsolete, but still sends Accept-CH-Lifetime
        string? lifetime = options.Lifetime?.TotalSeconds.ToString("0.##", provider: null); // ignore decimals
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

    private static void AddHints(List<string> hints, params IEnumerable<string>? names)
    {
        foreach (string name in names ?? [])
        {
            if (!string.IsNullOrWhiteSpace(name) && !hints.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                hints.Add(name);
            }
        }
    }
}
