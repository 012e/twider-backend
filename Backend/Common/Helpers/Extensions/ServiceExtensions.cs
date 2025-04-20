using System.Reflection;
using Backend.Common.DbContext;
using Backend.Common.Helpers.Interfaces;
using Backend.Common.Middlewares;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.ClientFactory;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            options.RequireHttpsMetadata = false;
            options.IncludeErrorDetails = true;
            options.UseSecurityTokenValidators = true;
            options.Authority = configuration["OAuth:Authority"];
            options.Audience = configuration["OAuth:Audience"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidAudience = configuration["OAuth:Audience"],
                ValidIssuer = configuration["OAuth:Authority"]
            };
        });

        return services;
    }

    public static IServiceCollection AddKeycloakAdminApi(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(_ =>
        {
            var credentials = new PasswordGrantFlow()
            {
                KeycloakUrl = configuration["Keycloak:Url"],
                Realm = configuration["Keycloak:Realm"],
                UserName = configuration["Keycloak:UserName"],
                Password = configuration["Keycloak:Password"],
            };
            var httpClient = AuthenticationHttpClientFactory.Create(credentials);
            var usersApi = ApiClientFactory.Create<UsersApi>(httpClient);
            return usersApi;
        });

        return services;
    }


    public static IServiceCollection AddAppServices(this IServiceCollection services,
        IConfiguration configuration)
    {

        return services;
    }

    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndPoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndPoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }
}