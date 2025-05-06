using Backend.Features.Post.Commands.UpdatePost;

namespace BackendTest.Features.Post.Commands;

using Xunit;
using Moq;
using Moq.EntityFrameworkCore;
using AutoFixture;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Backend.Common.DbContext;
using Backend.Features.Post.Commands.AddReaction; // Assuming the command is in this namespace
using MediatR; // For Unit type
using Microsoft.AspNetCore.Http; // For StatusCodes
using System;

public class UpdatePostHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ApplicationDbContext> _mockDbContext;

    public UpdatePostHandlerTests()
    {
        _fixture = new Fixture();
        _mockDbContext = new Mock<ApplicationDbContext>();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task Handle_ShouldUpdatePostContent_WhenPostExists()
    {
        // Arrange
        var existingPost = _fixture.Create<Post>();
        var newContent = _fixture.Create<string>();

        var command = _fixture.Build<UpdatePostCommand>()
            .With(c => c.PostId, existingPost.PostId)
            .With(c => c.Content, new UpdatePostCommand.UpdateContent
            {
                Content = newContent,
            }) // Assuming Content is a class with a Content property
            .Create();

        // Setup DbContext mock to return the existing post
        // Use a List and AsQueryable() to simulate DbSet behavior for Find/FirstOrDefault
        var postsList = new List<Post> { existingPost };
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(postsList);

        // Setup SaveChangesAsync mock
        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Indicate one entity was saved

        var handler = new UpdatePostHandler(_mockDbContext.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Unit.Value, result.Value);

        // Verify that the post's content was updated
        Assert.Equal(newContent, existingPost.Content);

        // Verify that SaveChangesAsync was called
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var nonExistentPostId = Guid.NewGuid();
        var command = _fixture.Build<UpdatePostCommand>()
            .With(c => c.PostId, nonExistentPostId)
            .Create();

        // Setup DbContext mock to return an empty DbSet for Posts
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post>());

        var handler = new UpdatePostHandler(_mockDbContext.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed); // Explicitly check for failure
        Assert.NotNull(result.Error);
        Assert.Equal(StatusCodes.Status404NotFound, result.Error.Status);
        Assert.Equal("Post not found", result.Error.Title);
        Assert.Contains($"Post with ID {nonExistentPostId} not found.", result.Error.Detail);

        // Verify that SaveChangesAsync was NOT called
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}