#pragma warning disable IDE0005 // Using directive is unnecessary
using Bunit;
using System;
using Xunit;
#pragma warning restore IDE0005

namespace AgentDashboard.Web.Tests.Infrastructure;

public class BunitFixture : IDisposable
{
    public BunitContext Context { get; } = new BunitContext();

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
