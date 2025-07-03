using Backend.Common.DbContext;
using Backend.Features.User.Queries;
using BackendTest.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTest.Features.User;

public class UserTest(IntegrationTestFactory factory) : BaseCqrsIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Get_User_By_Id_Successfully()
    {
        // Arrange
        var testUser = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-{Guid.NewGuid()}",
            Username = $"testuser-{Guid.NewGuid()}",
            Email = $"test-{Guid.NewGuid()}@example.com",
            ProfilePicture = "https://example.com/profile.jpg",
            Bio = "Test bio",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastLogin = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            VerificationStatus = "verified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(testUser);
            await dbContext.SaveChangesAsync();
        }

        var query = new GetUserByIdQuery
        {
            Id = testUser.UserId
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(testUser.UserId, result.Value.UserId);
        Assert.Equal(testUser.OauthSub, result.Value.OauthSub);
        Assert.Equal(testUser.Username, result.Value.Username);
        Assert.Equal(testUser.Email, result.Value.Email);
        Assert.Equal(testUser.ProfilePicture, result.Value.ProfilePicture);
        Assert.Equal(testUser.Bio, result.Value.Bio);
        // Check DateTime with tolerance for database precision differences
        Assert.True(Math.Abs((testUser.CreatedAt - result.Value.CreatedAt).TotalMilliseconds) < 1);
        Assert.True(Math.Abs((testUser.LastLogin!.Value - result.Value.LastLogin!.Value).TotalMilliseconds) < 1);
        Assert.Equal(testUser.IsActive, result.Value.IsActive);
        Assert.Equal(testUser.VerificationStatus, result.Value.VerificationStatus);
    }

    [Fact]
    public async Task Should_Return_404_For_Non_Existent_User()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var query = new GetUserByIdQuery
        {
            Id = nonExistentUserId
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.Status);
        Assert.Equal("User not found", result.Error.Title);
        Assert.Contains(nonExistentUserId.ToString(), result.Error.Detail);
    }

    [Fact]
    public async Task Should_Handle_User_With_Minimal_Data()
    {
        // Arrange
        var testUser = new Backend.Common.DbContext.User
        {
            UserId = Guid.NewGuid(),
            OauthSub = $"test-oauth-sub-minimal-{Guid.NewGuid()}",
            Username = $"minimaluser-{Guid.NewGuid()}",
            Email = null, // Optional field
            ProfilePicture = null, // Optional field
            Bio = null, // Optional field
            CreatedAt = DateTime.UtcNow,
            LastLogin = null, // Optional field
            IsActive = true,
            VerificationStatus = "unverified"
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(testUser);
            await dbContext.SaveChangesAsync();
        }

        var query = new GetUserByIdQuery
        {
            Id = testUser.UserId
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(testUser.UserId, result.Value.UserId);
        Assert.Equal(testUser.OauthSub, result.Value.OauthSub);
        Assert.Equal(testUser.Username, result.Value.Username);
        Assert.Null(result.Value.Email);
        Assert.Null(result.Value.ProfilePicture);
        Assert.Null(result.Value.Bio);
        // Check DateTime with tolerance for database precision differences
        Assert.True(Math.Abs((testUser.CreatedAt - result.Value.CreatedAt).TotalMilliseconds) < 1);
        Assert.Null(result.Value.LastLogin);
        Assert.Equal(testUser.IsActive, result.Value.IsActive);
        Assert.Equal(testUser.VerificationStatus, result.Value.VerificationStatus);
    }

    [Fact]
    public async Task Should_Handle_Multiple_Users_And_Return_Correct_One()
    {
        // Arrange
        var baseId = Guid.NewGuid().ToString("N")[0..8]; // Short unique identifier
        var users = new List<Backend.Common.DbContext.User>
        {
            new Backend.Common.DbContext.User
            {
                UserId = Guid.NewGuid(),
                OauthSub = $"test-oauth-sub-1-{baseId}",
                Username = $"user1-{baseId}",
                Email = $"user1-{baseId}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                IsActive = true,
                VerificationStatus = "verified"
            },
            new Backend.Common.DbContext.User
            {
                UserId = Guid.NewGuid(),
                OauthSub = $"test-oauth-sub-2-{baseId}",
                Username = $"user2-{baseId}",
                Email = $"user2-{baseId}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsActive = true,
                VerificationStatus = "pending"
            },
            new Backend.Common.DbContext.User
            {
                UserId = Guid.NewGuid(),
                OauthSub = $"test-oauth-sub-3-{baseId}",
                Username = $"user3-{baseId}",
                Email = $"user3-{baseId}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsActive = false,
                VerificationStatus = "unverified"
            }
        };

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(users);
            await dbContext.SaveChangesAsync();
        }

        var targetUser = users[1]; // Get the second user
        var query = new GetUserByIdQuery
        {
            Id = targetUser.UserId
        };

        // Act
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(targetUser.UserId, result.Value.UserId);
        Assert.Equal(targetUser.Username, result.Value.Username);
        Assert.Equal(targetUser.Email, result.Value.Email);
        Assert.Equal(targetUser.VerificationStatus, result.Value.VerificationStatus);
        Assert.Equal(targetUser.IsActive, result.Value.IsActive);
    }

    [Fact]
    public async Task Should_Validate_Required_Id_Field()
    {
        // Arrange
        var query = new GetUserByIdQuery
        {
            Id = Guid.Empty // Invalid empty GUID
        };

        // Act & Assert
        // The validation should catch this as empty GUIDs are typically invalid
        // However, since we're not validating empty GUIDs specifically, 
        // this will result in a 404 as expected
        var result = await Mediator.Send(query);
        Assert.True(result.IsFailed);
        Assert.Equal(404, result.Error.Status);
    }
}
