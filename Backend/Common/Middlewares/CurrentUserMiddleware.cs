using System.Security.Claims;
using Backend.Common.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Backend.Common.Middlewares;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db, ILogger<CurrentUserMiddleware> logger)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            logger.LogInformation("User is authenticated.");
            string? sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? context.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(sub))
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.OauthSub == sub);
                if (user != null)
                {
                    context.Items["CurrentUser"] = user;
                }
            }
        }

        await _next(context);
    }
}
