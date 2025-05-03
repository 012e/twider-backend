using Backend.Common.Helpers;
using Backend.Common.Middlewares;
using Backend.Common.Services;
using Backend.Features.User;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Register services
builder.Services
    // Configuration
    .AddAppConfiguration(configuration)

    // Authentication and Authorization
    .AddAppAuthentication(configuration)
    .AddAuthorization()
    .AddKeycloakAdminApi(configuration)

    // API and Endpoint Configuration
    .AddEndpoints(typeof(Program).Assembly)
    .AddEndpointsApiExplorer()
    .AddAppSwagger(configuration)

    // Core Application Services
    .AddAppServices(configuration)
    .AddPersistence(configuration)
    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()))
    .AddMemoryCache()

    // Error Handling
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails()

    // Utilities and Services
    .AddHttpContextAccessor()
    .AddScoped<ICurrentUserService, CurrentUserService>();

// Configure JSON serialization
ConfigureJsonOptions(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.ConfigureSwagger(configuration);
}

app.UseHttpsRedirection()
    .UseExceptionHandler()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();
app.UseAppMiddlewares();

app.MapEndpoints();

app.Run();

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