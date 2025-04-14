using Microsoft.IdentityModel.Logging;

namespace Backend.Common.Helpers;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigureSwagger(this WebApplication app, IConfiguration configuration)
    {
        if (!app.Environment.IsDevelopment()) return app;
        IdentityModelEventSource.ShowPII = true;
        IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            setup.SwaggerEndpoint($"/swagger/v1/swagger.json", "Version 1.0");
            setup.OAuthClientId(configuration["OAuth:ClientId"]);
            setup.OAuthAppName("Weather API");
            setup.OAuthScopeSeparator(" ");
            setup.OAuthUsePkce();
            setup.OAuthAdditionalQueryStringParams(new Dictionary<string, string>()
                { { "audience", configuration["OAuth:Audience"]! } });
        });

        return app;
    }
}