namespace BackendTest.Features.Comment.Commands;

using Xunit;
using Moq;
using Moq.EntityFrameworkCore;
using AutoFixture;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Backend.Common.DbContext;
using Backend.Features.Comment.Commands; // Assuming command is here
using Backend.Common.Services; // For ICurrentUserService
using Microsoft.AspNetCore.Http; // For StatusCodes
using System;

public class CreateCommentHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;

    public CreateCommentHandlerTests()
    {
        _fixture = new Fixture();
        _mockDbContext = new Mock<ApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize<Comment>(c =>
            c.With(inner => inner.CommentId, Guid.NewGuid)); // Use a different ID for each comment;
    }

    [Fact]
    public async Task Handle_ShouldCreateRootComment_WhenUserLoggedInAndPostExists()
    {
        // Arrange
        var currentUser = _fixture.Create<User>();
        var post = _fixture.Create<Post>();
        var command = _fixture.Build<CreateCommentCommand>()
            .With(c => c.PostId, post.PostId)
            .Without(c => c.ParentCommentId) // This is a root comment
            .Create();

        _mockCurrentUserService.Setup(x => x.User).Returns(currentUser);
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post> { post });
        _mockDbContext.Setup(x => x.Comments).ReturnsDbSet(new List<Comment>()); // Empty DbSet initially

        Comment addedComment = null; // To capture the comment added to the context
        _mockDbContext.Setup(x => x.Comments.Add(It.IsAny<Comment>()))
            .Callback<Comment>(cmt =>
            {
                addedComment = cmt;
            });

        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Indicate one entity was saved

        var handler = new CreateCommentHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Verify the properties of the added comment
        Assert.NotNull(addedComment);
        Assert.Equal(command.PostId, addedComment.PostId);
        Assert.Equal(command.Content.Content, addedComment.Content);
        Assert.Equal(currentUser.UserId, addedComment.UserId);
        Assert.Null(addedComment.ParentCommentId); // Ensure it's a root comment

        // Verify that SaveChangesAsync was called
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(x => x.Comments.Add(It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateReplyComment_WhenUserLoggedInPostAndParentExist()
    {
        // Arrange
        var currentUser = _fixture.Create<User>();
        var post = _fixture.Create<Post>();
        var parentComment = _fixture.Build<Comment>()
            .With(c => c.PostId, post.PostId) // Parent comment belongs to the post
            .Without(c =>
                c.ParentCommentId) // Parent itself is a root comment (or another reply, doesn't matter for this test)
            .Create();

        var command = _fixture.Build<CreateCommentCommand>()
            .With(c => c.PostId, post.PostId)
            .With(c => c.ParentCommentId, parentComment.CommentId) // This is a reply
            .Create();

        _mockCurrentUserService.Setup(x => x.User).Returns(currentUser);
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post> { post });
        // Need to include the parent comment in the DbSet for the handler to find it
        _mockDbContext.Setup(x => x.Comments).ReturnsDbSet(new List<Comment> { parentComment });

        Comment addedComment = null; // To capture the comment added to the context
        _mockDbContext.Setup(x => x.Comments.Add(It.IsAny<Comment>()))
            .Callback<Comment>(cmt => addedComment = cmt);

        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Indicate one entity was saved

        var handler = new CreateCommentHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Verify the properties of the added comment
        Assert.NotNull(addedComment);
        Assert.Equal(command.PostId, addedComment.PostId);
        Assert.Equal(command.Content.Content, addedComment.Content);
        Assert.Equal(currentUser.UserId, addedComment.UserId);
        Assert.Equal(command.ParentCommentId, addedComment.ParentCommentId); // Ensure ParentCommentId is set

        // Verify that SaveChangesAsync was called
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(x => x.Comments.Add(It.IsAny<Comment>()), Times.Once);
    }


    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserIsNotLoggedIn()
    {
        // Arrange
        var command = _fixture.Build<CreateCommentCommand>()
            .Create();

        _mockCurrentUserService.Setup(x => x.User).Returns((User)null); // User is null

        var handler = new CreateCommentHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(StatusCodes.Status403Forbidden, result.Error.Status);
        Assert.Equal("User must be logged in to comment", result.Error.Title);

        // Verify that database operations were NOT called
        _mockDbContext.Verify(x => x.Posts, Times.Never); // Should not even check for the post
        _mockDbContext.Verify(x => x.Comments.Add(It.IsAny<Comment>()), Times.Never);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldReturnBadRequest_WhenContentIsNullOrWhitespace(string content)
    {
        // Arrange
        var currentUser = _fixture.Create<User>();
        var command = _fixture.Build<CreateCommentCommand>()
            .With(c => c.Content, new CommentContent
            {
                Content = content
            })
            .Create();

        _mockCurrentUserService.Setup(x => x.User).Returns(currentUser);

        var handler = new CreateCommentHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Error.Status);
        Assert.Equal("Content cannot be empty", result.Error.Title);
        Assert.Equal("Comment content cannot be empty.", result.Error.Detail);

        // Verify that database operations were NOT called
        _mockDbContext.Verify(x => x.Posts, Times.Never); // Should not check for the post yet
        _mockDbContext.Verify(x => x.Comments, Times.Never); // Should not check for parent comment
        _mockDbContext.Verify(x => x.Comments.Add(It.IsAny<Comment>()), Times.Never);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var currentUser = _fixture.Create<User>();
        var nonExistentPostId = Guid.NewGuid();
        var command = _fixture.Build<CreateCommentCommand>()
            .With(c => c.PostId, nonExistentPostId)
            .Without(c => c.ParentCommentId) // Doesn't matter if it's a reply or root for this test
            .Create();

        _mockCurrentUserService.Setup(x => x.User).Returns(currentUser);
        // Setup DbContext mock to return an empty DbSet for Posts
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post>());
        // Mock comments DbSet as well in case ParentCommentId was provided (though not needed for this specific scenario path)
        _mockDbContext.Setup(x => x.Comments).ReturnsDbSet(new List<Comment>());


        var handler = new CreateCommentHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(StatusCodes.Status404NotFound, result.Error.Status);
        Assert.Equal("Post not found", result.Error.Title);
        Assert.Contains($"Post with ID {nonExistentPostId} not found.", result.Error.Detail);

        // Verify that SaveChangesAsync and Add were NOT called
        _mockDbContext.Verify(x => x.Comments.Add(It.IsAny<Comment>()), Times.Never);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenParentCommentDoesNotExist()
    {
        // Arrange
        var currentUser = _fixture.Create<User>();
        var post = _fixture.Create<Post>();
        var nonExistentParentCommentId = Guid.NewGuid();

        var command = _fixture.Build<CreateCommentCommand>()
            .With(c => c.PostId, post.PostId)
            .With(c => c.ParentCommentId, nonExistentParentCommentId) // Provide a parent ID that doesn't exist
            .Create();

        _mockCurrentUserService.Setup(x => x.User).Returns(currentUser);
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post> { post }); // Post exists
        // Setup DbContext mock to return an empty DbSet for Comments (or one that doesn't contain the parent)
        _mockDbContext.Setup(x => x.Comments).ReturnsDbSet(new List<Comment>());


        var handler = new CreateCommentHandler(_mockDbContext.Object, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.NotNull(result.Error);
        Assert.Equal(StatusCodes.Status404NotFound, result.Error.Status);
        Assert.Equal("Parent comment not found", result.Error.Title);
        Assert.Contains($"Parent comment with ID {nonExistentParentCommentId} not found.", result.Error.Detail);

        // Verify that SaveChangesAsync and Add were NOT called
        _mockDbContext.Verify(x => x.Comments.Add(It.IsAny<Comment>()), Times.Never);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}