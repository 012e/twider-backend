using System.Security.Claims;
using Backend.Common.DbContext;

namespace Backend.Common.Services;

public interface ICurrentUserService
{
    User? User { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public User? User
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Items["CurrentUser"] as User;
        }
    }
}
