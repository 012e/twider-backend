using Backend.Common.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Backend.Common.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<ApplicationDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("ApplicationDbContext")));
        return services;

    }

    public static IServiceCollection AddAppSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(configuration.GetValue<string>("OAuth:AuthorizationUrl")!),
                        TokenUrl = new Uri(configuration.GetValue<string>("OAuth:TokenUrl")!),
                        Scopes = new Dictionary<string, string>()
                    }
                },
                Description = "OpenID Connect Authorization Code + PKCE Flow "
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new List<string> { "your-api-scope" }
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;
            options.Authority = "https://dev-z88j6uoisbogqn82.us.auth0.com/";
            options.Audience = configuration["OAuth:Audience"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = configuration["OAuth:Audience"],
                ValidIssuer = configuration["OAuth:Authority"]
            };
        });

        return services;
    }
}