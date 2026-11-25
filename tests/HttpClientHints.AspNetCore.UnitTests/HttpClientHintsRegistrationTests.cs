// Copyright © https://myCSharp.de - all rights reserved

using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MyCSharp.HttpClientHints.AspNetCore.UnitTests;

public class HttpClientHintsRegistrationTests
{
    private static HttpClientHintsMiddlewareConfig GetConfig(Action<HttpClientHintsOptions>? configure = null)
    {
        ServiceCollection services = new();
        services.AddHttpClientHints(configure);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IOptions<HttpClientHintsMiddlewareConfig>>().Value;
    }

    [Fact]
    public void AddHttpClientHints_DefaultOptions_RequestOnlyLowEntropyHints()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig();

        Assert.Equal("Sec-CH-UA, Sec-CH-UA-Mobile", config.ResponseHeader);
        Assert.False(config.HasCriticalHeader);
        Assert.False(config.HasVaryHeader);
        Assert.Null(config.LifeTime);
    }

    [Fact]
    public void AddHttpClientHints_DefaultOptions_DoNotRequestUserAgent()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig(static options => options.UserAgent = true);

        Assert.DoesNotContain("User-Agent", config.ResponseHeader, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddHttpClientHints_AllOptions_RequestEveryHintInOrder()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig(static options =>
        {
            options.Platform = true;
            options.Architecture = true;
            options.Device = true;
            options.FullVersionList = true;
        });

        Assert.Equal(
            "Sec-CH-UA, Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version, Sec-CH-UA-Arch, Sec-CH-UA-Bitness, Sec-CH-UA-WoW64, "
            + "Sec-CH-UA-Model, Sec-CH-UA-Form-Factors, Sec-CH-UA-Mobile, Sec-CH-UA-Full-Version-List",
            config.ResponseHeader);
    }

    [Fact]
    public void AddHttpClientHints_Additional_AddsHintsWithoutEmptyEntriesAndDuplicates()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig(static options =>
            options.Additional = ["Sec-CH-Prefers-Color-Scheme", "", " ", "sec-ch-ua", "Sec-CH-Prefers-Color-Scheme"]);

        Assert.Equal("Sec-CH-UA, Sec-CH-UA-Mobile, Sec-CH-Prefers-Color-Scheme", config.ResponseHeader);
    }

    [Fact]
    public void AddHttpClientHints_AllHintsDisabled_RequestsNoHints()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig(static options =>
        {
            options.UserAgent = false;
            options.Mobile = false;
            options.Additional = null;
            options.Critical = null;
        });

        Assert.Empty(config.ResponseHeader!);
    }

    [Fact]
    public void AddHttpClientHints_Critical_SetsCriticalCHAndAddsHintsToAcceptCHAndVary()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig(static options =>
            options.Critical = ["Sec-CH-UA-Platform-Version", "sec-ch-ua-mobile", "Sec-CH-UA-Platform-Version", ""]);

        Assert.Equal("Sec-CH-UA, Sec-CH-UA-Mobile, Sec-CH-UA-Platform-Version", config.ResponseHeader);
        Assert.Equal("Sec-CH-UA-Platform-Version, sec-ch-ua-mobile", config.CriticalHeader);
        Assert.Equal("Sec-CH-UA-Platform-Version, sec-ch-ua-mobile", config.VaryHeader);
    }

    [Fact]
    public void AddHttpClientHints_Vary_AddsAllRequestedHintsToVary()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig(static options =>
        {
            options.Platform = true;
            options.Critical = ["Sec-CH-UA-Model"];
            options.Vary = true;
        });

        Assert.Equal("Sec-CH-UA, Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version, Sec-CH-UA-Mobile, Sec-CH-UA-Model", config.VaryHeader);
        Assert.Equal(config.ResponseHeader, config.VaryHeader);
        Assert.Equal("Sec-CH-UA-Model", config.CriticalHeader);
    }

    [Fact]
    public void AddHttpClientHints_TrimsNamesAndIgnoresNullEntries()
    {
        HttpClientHintsMiddlewareConfig config = GetConfig(static options =>
        {
            options.Platform = true;
            options.Additional = ["Sec-CH-UA-Platform ", " sec-ch-ua-mobile", null!, "\tSec-CH-Prefers-Color-Scheme\t"];
            options.Critical = ["Sec-CH-UA-Platform-Version "];
        });

        Assert.Equal("Sec-CH-UA, Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version, Sec-CH-UA-Mobile, Sec-CH-Prefers-Color-Scheme", config.ResponseHeader);
        Assert.Equal("Sec-CH-UA-Platform-Version", config.CriticalHeader);
    }

    [Theory]
    [InlineData("Sec CH")]
    [InlineData("Sec-CH-UA-Mobile, Sec-CH-UA-Model")]
    [InlineData("Sec-CH-UA;a=1")]
    [InlineData("1Sec-CH")]
    [InlineData("\"Sec-CH-UA\"")]
    [InlineData("Sec-CH-Ü")]
    public void AddHttpClientHints_InvalidAdditionalName_Throws(string name)
    {
        ServiceCollection services = new();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => services.AddHttpClientHints(options => options.Additional = [name]));

        Assert.Equal("config", exception.ParamName);
        Assert.Contains($"'{name}' in HttpClientHintsOptions.Additional", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddHttpClientHints_InvalidCriticalName_Throws()
    {
        ServiceCollection services = new();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => services.AddHttpClientHints(static options => options.Critical = ["Sec CH"]));

        Assert.Contains("'Sec CH' in HttpClientHintsOptions.Critical", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddHttpClientHints_ObsoleteLifetime_SendsWholeSecondsIndependentOfCulture()
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
#pragma warning disable CS0618 // Lifetime is obsolete
            HttpClientHintsMiddlewareConfig config = GetConfig(static options => options.Lifetime = TimeSpan.FromMilliseconds(1_500));
#pragma warning restore CS0618

            Assert.Equal("1", config.LifeTime);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void AddHttpClientHints_ObsoleteLifetime_SetsLifetimeInSeconds()
    {
#pragma warning disable CS0618 // Lifetime is obsolete
        HttpClientHintsMiddlewareConfig config = GetConfig(static options => options.Lifetime = TimeSpan.FromMinutes(30));
#pragma warning restore CS0618

        Assert.Equal("1800", config.LifeTime);
        Assert.True(config.HasLifetime);
    }

    [Fact]
    public void Lifetime_IsObsolete()
    {
#pragma warning disable CS0618 // Lifetime is obsolete
        const string PropertyName = nameof(HttpClientHintsOptions.Lifetime);
#pragma warning restore CS0618
        ObsoleteAttribute? attribute = (ObsoleteAttribute?)Attribute.GetCustomAttribute(
            typeof(HttpClientHintsOptions).GetProperty(PropertyName)!, typeof(ObsoleteAttribute));

        Assert.NotNull(attribute);
        Assert.Contains("Accept-CH-Lifetime", attribute.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HttpClientHintsOptions_HaveDataMinimizingDefaults()
    {
        HttpClientHintsOptions options = new();

        Assert.True(options.UserAgent);
        Assert.True(options.Mobile);
        Assert.False(options.Platform);
        Assert.False(options.Architecture);
        Assert.False(options.Device);
        Assert.False(options.FullVersionList);
        Assert.Null(options.Additional);
        Assert.Null(options.Critical);
        Assert.False(options.Vary);
    }

    [Fact]
    public async Task UseHttpClientHints_AddsMiddlewareAndReturnsApplicationBuilder()
    {
        ServiceCollection services = new();
        services.AddHttpClientHints();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ApplicationBuilder appBuilder = new(serviceProvider);

        IApplicationBuilder result = appBuilder.UseHttpClientHints();
        appBuilder.Run(static _ => Task.CompletedTask);
        DefaultHttpContext context = new();
        await appBuilder.Build()(context);

        Assert.Same(appBuilder, result);
        Assert.Equal("Sec-CH-UA, Sec-CH-UA-Mobile", context.Response.Headers["Accept-CH"].ToString());
    }

    [Fact]
    public async Task UseHttpClientHints_WithoutAddHttpClientHints_SetsNoHeader()
    {
        ServiceCollection services = new();
        services.AddOptions();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ApplicationBuilder appBuilder = new(serviceProvider);

        appBuilder.UseHttpClientHints();
        appBuilder.Run(static _ => Task.CompletedTask);
        DefaultHttpContext context = new();
        await appBuilder.Build()(context);

        Assert.False(context.Response.Headers.ContainsKey("Accept-CH"));
    }
}
