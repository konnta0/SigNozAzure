using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleWebApp;

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
            new[]
            {
                new KeyValuePair<string, object>("service.environment", "example"),
            });
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
        .AddMeter(serviceName)
        .AddMeter("WeatherForecastController")
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
