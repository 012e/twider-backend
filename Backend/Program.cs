using Backend.Common.DbContext;
using Backend.Common.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer()
    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()))
    .AddAppSwagger(configuration)
    .AddAppAuthentication(configuration)
    .AddPersistence(configuration);

var app = builder.Build();

app.ConfigureSwagger(configuration);

app.MapGet("/weatherforecast", async (ApplicationDbContext context) => { return await context.Comments.ToListAsync(); })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/fuck", () => { return "fuck you"; })
    .WithName("holy fuck")
    .WithOpenApi();


app.Run();