using System.Diagnostics;

namespace SampleWebApp;

public sealed class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "dice-server";
    internal const string ActivitySourceVersion = "1.0.0";

    public ActivitySource ActivitySource { get; } = new(ActivitySourceName, ActivitySourceVersion);

    public void Dispose()
    {
        this.ActivitySource.Dispose();
    }
}