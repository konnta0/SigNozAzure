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
    private readonly Counter<int> Counter = instrumentation.CallCounter;

    [HttpGet(Name = "GetWeatherForecast")]
    public async ValueTask<IEnumerable<WeatherForecast>> Get()
    {
        logger.LogInformation("GetWeatherForecast called");
        Counter.Add(1);

        var results = new List<WeatherForecast>();
        const string activityName = "Iteration";
        for (var i = 0; i < 5; i++)
        {
            using var activity = _activitySource.StartActivity(activityName,
                ActivityKind.Server,
                Activity.Current?.Context ?? default(ActivityContext)
            );
            await Task.Delay(TimeSpan.FromMilliseconds(i * 100));

            activity?.SetTag("iteration", i);
            
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
            activity?.SetTag("date", date);

            var temperatureC = Random.Shared.Next(-20, 55);
            activity?.SetTag("temperatureC", temperatureC);

            results.Add(new WeatherForecast
            {
                Date = date,
                TemperatureC = temperatureC,
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });
        }

        return results;
    }
}