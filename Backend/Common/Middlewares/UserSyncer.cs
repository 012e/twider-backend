using System.Security.Claims;
using Backend.Common.DbContext;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Model;

namespace Backend.Common.Middlewares;

public class UserSyncer
{
    private readonly RequestDelegate _next;

    public UserSyncer(RequestDelegate next)
    {
        _next = next;
    }


    // WTF, please fix
    public async Task InvokeAsync(HttpContext context, UsersApi usersApi, ApplicationDbContext db,
        ILogger<UserSyncer> logger)
    {
        string? oauthSub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst("sub")?.Value;
        if (oauthSub == null)
        {
            await _next(context);
            return;
        }

        var keycloakUser = await usersApi.GetUsersByUserIdAsync("master", oauthSub);
        if (keycloakUser == null)
        {
            logger.LogError("User with id {} not found in Keycloak.", oauthSub);
            await _next(context);
            return;
        }

        if (keycloakUser.Attributes == null)
        {
            logger.LogWarning("User with id {} has no attributes.", oauthSub);
        }

        if (await CreateUserIfNotExists(context, _next, oauthSub, keycloakUser, db)) return;
        await _next(context);
    }

    private async Task<bool> CreateUserIfNotExists(HttpContext context, RequestDelegate next, string oauthSub,
        UserRepresentation keycloakUser, ApplicationDbContext db)
    {
        var user = db.Users.FirstOrDefault(u => u.OauthSub == oauthSub);
        if (user != null)
        {
            await next(context);
            return true;
        }

        var verificationStatus = "verified";
        if (!keycloakUser.EmailVerified ?? false)
        {
            verificationStatus = "unverified";
        }

        user = new User
        {
            OauthSub = oauthSub,
            Email = keycloakUser.Email,
            VerificationStatus = verificationStatus,
            Username = keycloakUser.Username,
            // TODO: add more fields
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return false;
    }
}