// Copyright © myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MyCSharp.HttpClientHints.AspNetCore;

/// <summary>
/// Extension methods for registering and configuring HTTP Client Hints middleware.
/// </summary>
public static class HttpClientHintsRegistration
{
    /// <summary>
    /// Adds HTTP Client Hints configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="config">An optional action to configure <see cref="HttpClientHintsOptions"/>.</param>
    public static IServiceCollection AddHttpClientHints(this IServiceCollection services, Action<HttpClientHintsOptions>? config = null)
    {
        HttpClientHintsOptions httpClientHintsConfig = new();
        config?.Invoke(httpClientHintsConfig);

        // lifetime
        string? lifeTime;
        if (httpClientHintsConfig.Lifetime is TimeSpan lifetime)
        {
            lifeTime = lifetime.TotalSeconds.ToString("0.##"); // ignore decimals
        }
        else
        {
            lifeTime = null;
        }

        // headers
        List<string> headers = [];
        if (httpClientHintsConfig.UserAgent)
        {
            headers.Add("User-Agent");
            headers.Add("Sec-CH-UA");
        }
        if (httpClientHintsConfig.Platform)
        {
            headers.Add("Sec-CH-UA-Platform");
            headers.Add("Sec-CH-UA-Platform-Version");
        }
        if (httpClientHintsConfig.Architecture)
        {
            headers.Add("Sec-CH-UA-Arch");
            headers.Add("Sec-CH-UA-Bitness");
        }
        if (httpClientHintsConfig.Device)
        {
            headers.Add("Sec-CH-UA-Model");
        }
        if (httpClientHintsConfig.Mobile)
        {
            headers.Add("Sec-CH-UA-Mobile");
        }

        if (!string.IsNullOrEmpty(httpClientHintsConfig.Additional))
        {
            headers.Add(httpClientHintsConfig.Additional);
        }

        // register middleware config
        services.Configure<HttpClientHintsMiddlewareConfig>(o =>
        {
            o.ResponseHeader = string.Join(", ", headers);
            o.LifeTime = lifeTime;
        });

        return services;
    }

    /// <summary>
    /// Adds the <see cref="HttpClientHintsRequestMiddleware"/> to the application's request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder with the middleware added.</returns>
    public static IApplicationBuilder UseHttpClientHints(this IApplicationBuilder app)
    {
        app.UseMiddleware<HttpClientHintsRequestMiddleware>();

        return app;
    }
}
