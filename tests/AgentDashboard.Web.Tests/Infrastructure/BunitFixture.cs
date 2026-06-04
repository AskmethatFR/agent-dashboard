#pragma warning disable IDE0005 // Using directive is unnecessary
using AspNetCore.Localizer.Json.Extensions;
using AspNetCore.Localizer.Json.JsonOptions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Text;
using Xunit;
#pragma warning restore IDE0005

namespace AgentDashboard.Web.Tests.Infrastructure;

public class BunitFixture : IDisposable
{
    public BunitContext Context { get; }

    public BunitFixture()
    {
        Context = new BunitContext();
        Context.Services.AddJsonLocalization(options =>
        {
            options.ResourcesPath = "i18n";
            options.UseEmbeddedResources = false;
            options.SupportedCultureInfos = new HashSet<CultureInfo>
            {
                new CultureInfo("en-US"),
                new CultureInfo("fr-FR")
            };
            options.LocalizationMode = LocalizationMode.I18n;
            options.CacheDuration = TimeSpan.FromMinutes(30);
            options.FileEncoding = Encoding.UTF8;
            options.IgnoreJsonErrors = false;
        });
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
