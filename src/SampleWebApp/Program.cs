using System.Net;
using CloudStructures;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleWebApp;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string serviceName = "SampleWebApp";
const string serviceVersion = "1.0.0";
var otlpEndpoint = new Uri("http://20.40.96.181:4317");

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddAttributes(
        [
            new KeyValuePair<string, object>("service.environment", "example")
        ]);
        resource.AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion);
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(serviceName)
            .AddAspNetCoreInstrumentation(options => { options.RecordException = true; })
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter(options => { options.Endpoint = otlpEndpoint; });
        tracing.AddSource(Instrumentation.ActivitySourceName);
        tracing.AddInstrumentation(new Instrumentation());
    })
    .WithMetrics(metrics => metrics
        .AddMeter(nameof(ApplicationMetrics))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
            {
                options.Endpoint = otlpEndpoint;
            })
        .AddConsoleExporter())
    .WithLogging(logging =>
    {
        logging.AddConsoleExporter();
        logging.AddOtlpExporter(options =>
        {
            options.Endpoint = otlpEndpoint;
        });
    }, 
        options =>
    {
        options.IncludeScopes = true;
        options.ParseStateValues = true;
        options.IncludeFormattedMessage = true;
    });

builder.Services.AddSingleton<Instrumentation>();
builder.Services.AddSingleton<ApplicationMetrics>();
builder.Services.AddScoped<RedisConnection>(_ =>
{
    var pass = Environment.GetEnvironmentVariable("CACHE_PASS_WORD");
    var connection = new RedisConnection(new RedisConfig("default", new ConfigurationOptions
    {
        Password = pass,
        Ssl = true,
        AbortOnConnectFail = false,
        EndPoints = new EndPointCollection
        {
            { "signoz-azure.redis.cache.windows.net", 6380 }
        }
    }));
    return connection;
});
builder.Services.AddHttpClient();

builder.Services.AddDbContext<ExampleDbContext>(static x => x.UseSqlServer());

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
