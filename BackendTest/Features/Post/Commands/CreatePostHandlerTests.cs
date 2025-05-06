using Backend.Features.Post.Commands.CreatePost;

namespace BackendTest.Features.Post.Commands;

using Xunit;
using Moq;
using AutoFixture;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common.DbContext;
using Backend.Features.Post.Commands.AddReaction; // Assuming command is here
using Backend.Common.Helpers.Types;
using Backend.Common.Services; // For ICurrentUserService
// For IRequestHandler
using Microsoft.AspNetCore.Http; // For StatusCodes
// For ProblemDetails
using Microsoft.EntityFrameworkCore.ChangeTracking; // For EntityEntry
using Microsoft.EntityFrameworkCore; // For DbSet
using System;

// For LINQ operations

// Assuming CreatePostCommand, ItemId, User, Post are defined in appropriate namespaces
// Example definitions (adjust based on your actual code):
// namespace Backend.Features.Post.Commands { public record CreatePostCommand(string Content) : IRequest<ApiResult<ItemId>>; }
// namespace Backend.Common.Helpers.Types { public record ItemId(Guid Value); }
// namespace Backend.Common.Services { public interface ICurrentUserService { User? User { get; } } }
// namespace Backend.Domain.Entities { public class User { public Guid UserId { get; set; } } }
// namespace Backend.Domain.Entities { public class Post { public Guid PostId { get; set; } public string Content { get; set; } public Guid UserId { get; set; } } }


public class CreatePostHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;

    public CreatePostHandlerTests()
    {
        _fixture = new Fixture();
        _mockDbContext = new Mock<ApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        // Configure AutoFixture to prevent recursion issues
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Customize ItemId creation to always give a value
        _fixture.Customize<ItemId>(c => c.FromFactory(() => new ItemId(Guid.NewGuid())));

        // Ensure User can be created with a UserId
        _fixture.Customize<User>(c => c.With(u => u.UserId, Guid.NewGuid()));

         // Ensure Post can be created with a PostId and UserId
        _fixture.Customize<Post>(c => c
            .With(p => p.PostId, Guid.NewGuid())
            .With(p => p.UserId, Guid.NewGuid()));

        // No longer need to set up db.Posts here as we'll do it per test
    }

    [Fact]
    public async Task Handle_ShouldCreatePost_WhenUserIsLoggedIn()
    {
        // Arrange
        var currentUser = _fixture.Create<User>();
        var command = _fixture.Create<CreatePostCommand>(); // AutoFixture will create with content

        _mockCurrentUserService.Setup(x => x.User).Returns(currentUser);

        // --- Local DbSet Mocking ---
        var mockPostsDbSet = new Mock<DbSet<Post>>();

        mockPostsDbSet.Setup(x => x.Add(It.IsAny<Post>()))
             .Returns<Post>((post) => {
                // Create a mock EntityEntry for the post that was just "added"
                var entityEntryMock = new Mock<EntityEntry<Post>>();
                // Ensure the Entity property points back to the passed-in post
                entityEntryMock.SetupGet(e => e.Entity).Returns(post);
                // Assign a PostId to the passed-in post object
                 if (post.PostId == Guid.Empty) // Avoid overwriting AutoFixture's generated ID if it sets one
                {
                     post.PostId = Guid.NewGuid();
                }
                return entityEntryMock.Object;
             });


        // Set up the DbContext to return our local mocked DbSet<Post>
        _mockDbContext.Setup(db => db.Posts).Returns(mockPostsDbSet.Object);
        // --- End Local DbSet Mocking ---


        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Indicate one entity was saved

        var handler = new CreatePostHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id); // Check if the returned ID is not empty

        // Verify that Add was called on the mocked DbSet (accessed via DbContext.Posts)
        // Use It.Is<Post> to verify the properties of the post that was added
        mockPostsDbSet.Verify(x => x.Add(It.Is<Post>(p =>
            p.Content == command.Content && // Verify content matches command
            p.UserId == currentUser.UserId && // Verify UserId matches current user
            p.PostId != Guid.Empty // Verify a PostId was assigned by the mock Add setup
        )), Times.Once);

        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserIsNotLoggedIn()
    {
        // Arrange
        var command = _fixture.Create<CreatePostCommand>();

        _mockCurrentUserService.Setup(x => x.User).Returns((User)null); // User is null

        // No need to set up DbContext.Posts here as the handler should return early

        var handler = new CreatePostHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        // Asserting against the specific status code and title from the handler
        Assert.Equal(StatusCodes.Status404NotFound, result.Error.Status);
        Assert.Equal("User not found", result.Error.Title);
        Assert.Contains("User with ID ", result.Error.Detail);


        // Verify that database operations were NOT called
        // We don't need to verify the DbSet mock if the handler exits before accessing it
        _mockDbContext.Verify(x => x.Posts, Times.Never);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}