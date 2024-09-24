using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly ActivitySource _activitySource;
    private readonly Counter<int> _counter;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, Instrumentation instrumentation)
    {
        _logger = logger;
        _activitySource = instrumentation.ActivitySource;
        _counter = new Meter("WeatherForecastController").CreateCounter<int>("call_count", description: "The number of times the controller is called");
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation("GetWeatherForecast called");
        _counter.Add(1);
        const string activityName = "GetWeatherForecast";
        return Enumerable.Range(1, 5).Select(index =>
            {
                using var activity = _activitySource.StartActivity(activityName);
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