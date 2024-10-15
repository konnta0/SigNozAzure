using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger, Instrumentation instrumentation)
    : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ActivitySource _activitySource = instrumentation.ActivitySource;
    private readonly Counter<int> _counter = new Meter("WeatherForecastController").CreateCounter<int>("call_count", description: "The number of times the controller is called");

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        logger.LogInformation("GetWeatherForecast called");
        _counter.Add(1);
        const string activityName = "GetWeatherForecast";
        return Enumerable.Range(1, 5).Select(index =>
            {
                using var activity = _activitySource.StartActivity(activityName,
                    ActivityKind.Server,
                    Activity.Current?.Context ?? default(ActivityContext)
                );
                activity?.SetTag("iteration", index);
                var date = DateOnly.FromDateTime(DateTime.Now.AddDays(index));
                activity?.SetTag("date", date);
                var temperatureC = Random.Shared.Next(-20, 55);
                activity?.SetTag("temperatureC", temperatureC);
                return new WeatherForecast
                {
                    Date = date,
                    TemperatureC = temperatureC,
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };
            })
            .ToArray();
    }
}