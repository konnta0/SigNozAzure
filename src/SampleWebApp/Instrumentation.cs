using System.Diagnostics;

namespace SampleWebApp;

public sealed class Instrumentation : DefaultTraceListener, IDisposable
{
    public const string ActivitySourceName = "SampleWebApp-server";
    private const string ActivitySourceVersion = "1.0.0";

    public ActivitySource ActivitySource { get; } = new(ActivitySourceName, ActivitySourceVersion);

    public void Dispose()
    {
        this.ActivitySource.Dispose();
    }
}