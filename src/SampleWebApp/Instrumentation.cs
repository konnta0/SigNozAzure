using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SampleWebApp;

public sealed class Instrumentation : DefaultTraceListener, IDisposable
{
    public const string ActivitySourceName = "SampleWebApp-server";
    private const string ActivitySourceVersion = "1.0.0";

    private readonly Meter _meter;

    public Counter<int> CallCounter { get; init; } 
        
    public ActivitySource ActivitySource { get; } = new(ActivitySourceName, ActivitySourceVersion);

    public Instrumentation()
    {
        _meter = new Meter(nameof(Instrumentation));
        _meter.CreateObservableCounter("thread.cpu_time", () => GetThreadCpuTime(Process.GetCurrentProcess()));
        CallCounter = _meter.CreateCounter<int>("get_weather_call_count", description: "This is demo");
    }
    
    public void Dispose()
    {
        this.ActivitySource.Dispose();
    }
    
    private static IEnumerable<Measurement<double>> GetThreadCpuTime(Process process)
    {
        foreach (ProcessThread thread in process.Threads)
        {
            yield return new Measurement<double>(thread.TotalProcessorTime.TotalMilliseconds, new KeyValuePair<string, object?>("ProcessId", process.Id), new KeyValuePair<string, object?>("ThreadId", thread.Id));
        }
    }
}