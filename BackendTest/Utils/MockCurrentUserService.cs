using Backend.Common.DbContext;
using Backend.Common.Services;

namespace BackendTest.Utils;

public class MockCurrentUserService : ICurrentUserService
{
    private readonly ApplicationDbContext _dbContext;
    private User? user;

    public MockCurrentUserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public User? User
    {
        get
        {
            if (user == null)
            {
                user = _dbContext.Users.FirstOrDefault();
            }

            return user;
        }
    }
}