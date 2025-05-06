using System.Reflection;
using Backend.Common.Configuration;
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
using Microsoft.Extensions.Options;
using Minio;

namespace Backend.Common.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register options with validation
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OAuthOptions>()
            .Bind(configuration.GetSection(OAuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<KeycloakOptions>()
            .Bind(configuration.GetSection(KeycloakOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MinioOptions>()
            .Bind(configuration.GetSection(MinioOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContextPool<ApplicationDbContext>(opt =>
        {
            var dbOptions = services.BuildServiceProvider().GetRequiredService<IOptions<DatabaseOptions>>().Value;
            opt.UseNpgsql(dbOptions.ApplicationDbContext);
            opt.EnableSensitiveDataLogging();
        });
        return services;
    }

    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.ToString());
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            var oauthOptions = services.BuildServiceProvider().GetRequiredService<IOptions<OAuthOptions>>().Value;

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(oauthOptions.AuthorizationUrl),
                        TokenUrl = new Uri(oauthOptions.TokenUrl),
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

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            var oauthOptions = services.BuildServiceProvider().GetRequiredService<IOptions<OAuthOptions>>().Value;

            options.RequireHttpsMetadata = false;
            options.IncludeErrorDetails = true;
            options.UseSecurityTokenValidators = true;
            options.Authority = oauthOptions.Authority;
            options.Audience = oauthOptions.Audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidAudience = oauthOptions.Audience,
                ValidIssuer = oauthOptions.Authority
            };
        });

        return services;
    }

    public static IServiceCollection AddKeycloakAdminApi(this IServiceCollection services)
    {
        services.AddSingleton(_ =>
        {
            var keycloakOptions = services.BuildServiceProvider().GetRequiredService<IOptions<KeycloakOptions>>().Value;

            var credentials = new PasswordGrantFlow()
            {
                KeycloakUrl = keycloakOptions.Url,
                Realm = keycloakOptions.Realm,
                UserName = keycloakOptions.Username,
                Password = keycloakOptions.Password,
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

    public static IServiceCollection AddMinio(this IServiceCollection services)
    {
        var minioOptions = services.BuildServiceProvider().GetRequiredService<IOptions<MinioOptions>>().Value;

        services.AddMinio(configureClient => configureClient
            .WithEndpoint(minioOptions.Endpoint)
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .Build());

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