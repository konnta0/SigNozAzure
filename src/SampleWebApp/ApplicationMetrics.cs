using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SampleWebApp;

public sealed class ApplicationMetrics
{ 
    private readonly Meter _meter;
    public Counter<int> CallCounter { get; init; }
    
    public ApplicationMetrics()
    {
        _meter = new Meter(nameof(ApplicationMetrics));
        CallCounter = _meter.CreateCounter<int>(nameof(CallCounter), description: "This is demo");
        
        _meter = new Meter(nameof(Instrumentation));
        _meter.CreateObservableCounter("thread_cpu_time", () => GetThreadCpuTime(Process.GetCurrentProcess()));
        CallCounter = _meter.CreateCounter<int>("get_weather_call_count", description: "This is demo");
    }
    
    private static IEnumerable<Measurement<double>> GetThreadCpuTime(Process process)
    {
        foreach (ProcessThread thread in process.Threads)
        {
            yield return new Measurement<double>(thread.TotalProcessorTime.TotalMilliseconds, new KeyValuePair<string, object?>("ProcessId", process.Id), new KeyValuePair<string, object?>("ThreadId", thread.Id));
        }
    }
}