using System.Reflection;
using Backend.Common.Configuration;
using Backend.Common.Middlewares;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Backend.Common.Helpers.Interfaces;


namespace Backend.Common.Helpers;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder ConfigureSwagger(this WebApplication app, IConfiguration configuration)
    {
        if (!app.Environment.IsDevelopment()) return app;
        IdentityModelEventSource.ShowPII = true;
        IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            var oauthOptions = app.Services.GetRequiredService<IOptions<OAuthOptions>>().Value;

            setup.SwaggerEndpoint($"/swagger/v1/swagger.json", "Version 1.0");
            setup.OAuthClientId(oauthOptions.ClientId);
            setup.OAuthAppName("Weather API");
            setup.OAuthScopeSeparator(" ");
            setup.OAuthUsePkce();
            setup.OAuthAdditionalQueryStringParams(new Dictionary<string, string>()
                { { "audience", oauthOptions.Audience } });
        });

        return app;
    }

    public static IApplicationBuilder UseAppMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserSyncer>();
        app.UseMiddleware<CurrentUserMiddleware>();

        return app;
    }

    public static IApplicationBuilder SetupCors(this IApplicationBuilder app)
    {
        app.UseCors(options =>
        {
            options.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(_ => true);
        });

        return app;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services
            .GetRequiredService<IEnumerable<IEndPoint>>();

        IEndpointRouteBuilder builder =
            routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}