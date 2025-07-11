using Backend.Common.Helpers;
using Backend.Common.Helpers.Extensions;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Register services
builder.Services
    // Configuration
    .AddAppConfiguration(configuration)

    // Authentication and Authorization
    .AddAppAuthentication()
    .AddAuthorization()
    .AddKeycloakAdminApi()

    // API and Endpoint Configuration
    .AddEndpoints(typeof(Program).Assembly)
    .AddEndpointsApiExplorer()
    .AddAppSwagger()
    .AddCors()

    // Core Application Services
    .AddAppServices(configuration)
    .AddPersistence()
    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()))
    .AddMemoryCache()
    .AddMinio()

    // Error Handling
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()

    // Utilities and Services
    .AddHttpContextAccessor();

// Configure JSON serialization
ConfigureJsonOptions(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.ConfigureSwagger(configuration);
}

app
    .SetupCors()
    .UseHttpsRedirection()
    .UseExceptionHandler()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseAppMiddlewares();

app.MapEndpoints();

app.Run();
return;

// Helper method for JSON configuration
void ConfigureJsonOptions(IServiceCollection services)
{
    services.Configure<JsonOptions>(options =>
    {
        options.SerializerOptions.Converters.Add(new ResultJsonConverterFactory());
    });

    services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new ResultJsonConverterFactory());
    });
}

public partial class Program
{
}