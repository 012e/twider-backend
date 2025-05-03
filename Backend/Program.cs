using Backend.Common.Helpers;
using Backend.Common.Middlewares;
using Backend.Common.Services;
using Backend.Features.User;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddAppAuthentication(configuration)
    .AddAppServices(configuration)
    .AddAppSwagger(configuration)
    .AddAuthorization()
    .AddEndpoints(typeof(Program).Assembly)
    .AddEndpointsApiExplorer()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddHttpContextAccessor()
    .AddKeycloakAdminApi(configuration)
    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()))
    .AddMemoryCache()
    .AddPersistence(configuration)
    .AddProblemDetails()
    .AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new ResultJsonConverterFactory());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new ResultJsonConverterFactory());
});

var app = builder.Build();

app.ConfigureSwagger(configuration);

app
    .UseHttpsRedirection()
    .UseExceptionHandler()
    .UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAppMiddlewares();

app.MapEndpoints();


app.Run();