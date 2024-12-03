using System.Diagnostics;
using System.Diagnostics.Metrics;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(
    ILogger<WeatherForecastController> logger, 
    Instrumentation instrumentation, 
    ApplicationMetrics applicationMetrics,
    IHttpClientFactory httpClientFactory,
    RedisConnection redisConnection)
    : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ActivitySource _activitySource = instrumentation.ActivitySource;
    private readonly Counter<int> _counter = applicationMetrics.CallCounter;

    [HttpGet(Name = "GetWeatherForecast")]
    public async ValueTask<IEnumerable<WeatherForecast>> Get()
    {
        logger.LogInformation("GetWeatherForecast called");
        _counter.Add(1, new KeyValuePair<string, object?>[]
        {
            new("controller", "WeatherForecast"),
            new("action", "Get"),
        });

        using var client = httpClientFactory.CreateClient();

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

            _ = await client.GetAsync("https://www.bing.com");
        }

        return results;
    }
    
    [HttpPost(Name = "Force4xxError")]
    public IActionResult Post()
    {
        logger.LogWarning("Force4xxError called");
        
        var statusCodes = new[]
        {
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status405MethodNotAllowed,
            StatusCodes.Status406NotAcceptable,
            StatusCodes.Status407ProxyAuthenticationRequired,
            StatusCodes.Status408RequestTimeout,
            StatusCodes.Status409Conflict,
            StatusCodes.Status410Gone,
            StatusCodes.Status411LengthRequired,
            StatusCodes.Status412PreconditionFailed,
            StatusCodes.Status413PayloadTooLarge,
            StatusCodes.Status414UriTooLong,
            StatusCodes.Status415UnsupportedMediaType,
            StatusCodes.Status416RangeNotSatisfiable,
            StatusCodes.Status417ExpectationFailed,
            StatusCodes.Status418ImATeapot,
            StatusCodes.Status421MisdirectedRequest,
            StatusCodes.Status422UnprocessableEntity,
            StatusCodes.Status423Locked,
            StatusCodes.Status424FailedDependency,
            StatusCodes.Status426UpgradeRequired,
            StatusCodes.Status428PreconditionRequired,
            StatusCodes.Status429TooManyRequests,
            StatusCodes.Status431RequestHeaderFieldsTooLarge,
            StatusCodes.Status451UnavailableForLegalReasons,
        };
        return new StatusCodeResult(statusCodes[Random.Shared.Next(statusCodes.Length)]);
    }

    [HttpDelete(Name = "Force5xxError")]
    public IActionResult Delete()
    {
        logger.LogError("Force5xxError called");
        
        var statusCodes = new[]
        {
            StatusCodes.Status500InternalServerError,
            StatusCodes.Status501NotImplemented,
            StatusCodes.Status502BadGateway,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status504GatewayTimeout,
            StatusCodes.Status505HttpVersionNotsupported,
            StatusCodes.Status506VariantAlsoNegotiates,
            StatusCodes.Status507InsufficientStorage,
            StatusCodes.Status508LoopDetected,
            StatusCodes.Status510NotExtended,
            StatusCodes.Status511NetworkAuthenticationRequired,
        };
        return new StatusCodeResult(statusCodes[Random.Shared.Next(statusCodes.Length)]);
    }

    [HttpGet("redis", Name = "RedisGet")]
    public async ValueTask<IActionResult> RedisGetAsync([FromRoute]string key = "example")
    {
        logger.LogInformation("RedisGet called");
        var redisString = new RedisString<string>(redisConnection, key, null);
        var value = await redisString.GetAsync();
        
        if (value.HasValue)
        {
            return Ok(value.Value);
        }

        return NotFound();
    }

    [HttpPost("redis", Name = "RedisPost")]
    public async ValueTask<IActionResult> RedisSetAsync([FromRoute]string key = "example", [FromBody] string value = "example")
    {
        logger.LogInformation("RedisSet called");
        var redisString = new RedisString<string>(redisConnection, key, null);
        await redisString.SetAsync(value);
        return NoContent();
    }
    
    [HttpDelete("redis", Name = "RedisDelete")]
    public async ValueTask<IActionResult> RedisDeleteAsync([FromRoute]string key = "example")
    {
        logger.LogInformation("RedisDelete called");
        var redisString = new RedisString<string>(redisConnection, key, null);
        await redisString.DeleteAsync();
        return NoContent();
    }
}