namespace BackendTest.Features.Comments;

using Xunit;
using Moq;
using Moq.EntityFrameworkCore;
using AutoFixture;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Backend.Common.DbContext;
using Backend.Features.Comment.Queries;
using Backend.Common.Helpers.Types;
using System;

public class GetCommentsByPostHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ApplicationDbContext> _mockDbContext;

    public GetCommentsByPostHandlerTests()
    {
        _fixture = new Fixture();
        _mockDbContext = new Mock<ApplicationDbContext>();

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior()); //recursionDepth
        _fixture.Customize<InfiniteCursorPaginationMeta>(c =>
            c.With(x => x.Cursor, (string?)null)); // User is not needed for testing
    }

    [Fact]
    public async Task Handle_ShouldReturnNestedComments_WhenCommentIdIsProvided()
    {
        // Arrange
        var post = _fixture.Create<Post>();
        var parentComment = _fixture.Create<Comment>();
        var childComment1 = _fixture.Create<Comment>();
        var childComment2 = _fixture.Create<Comment>();

        // Setup parent-child relationship
        parentComment.PostId = post.PostId;
        parentComment.ParentCommentId = null; // root comment
        childComment1.PostId = post.PostId;
        childComment1.ParentCommentId = parentComment.CommentId;
        childComment2.PostId = post.PostId;
        childComment2.ParentCommentId = parentComment.CommentId;

        // Adding users to comments
        parentComment.User = _fixture.Create<User>();
        childComment1.User = _fixture.Create<User>();
        childComment2.User = _fixture.Create<User>();

        // Create the query with a commentId that will be used to fetch nested comments
        var query = _fixture.Build<GetCommentsByPostQuery>()
            .With(q => q.PostId, post.PostId)
            .With(q => q.CommentId, parentComment.CommentId) // Querying replies for the parent comment
            .Create();

        var comments = new List<Comment> { parentComment, childComment1, childComment2 };

        // Setup DbContext mock
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post> { post });
        _mockDbContext.Setup(x => x.Comments).ReturnsDbSet(comments);

        var handler = new GetCommentsByPostHandler(_mockDbContext.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count); // Expecting 2 replies (childComment1 and childComment2)
        Assert.All(result.Value.Items, item => Assert.Equal(parentComment.CommentId, item.ParentCommentId));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoRepliesForCommentId()
    {
        // Arrange
        var post = _fixture.Create<Post>();
        var parentComment = _fixture.Create<Comment>();

        parentComment.PostId = post.PostId;
        parentComment.ParentCommentId = null; // root comment
        parentComment.User = _fixture.Create<User>();

        var query = _fixture.Build<GetCommentsByPostQuery>()
            .With(q => q.PostId, post.PostId)
            .With(q => q.CommentId, parentComment.CommentId) // Querying replies for the parent comment
            .Create();

        // Only the parent comment exists
        var comments = new List<Comment> { parentComment };

        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post> { post });
        _mockDbContext.Setup(x => x.Comments).ReturnsDbSet(comments);

        var handler = new GetCommentsByPostHandler(_mockDbContext.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items); // No replies to the parent comment
    }

    [Fact]
    public async Task Handle_ShouldReturnNestedCommentsWithPagination_WhenCommentIdIsProvidedAndPaginated()
    {
        // Arrange
        var post = _fixture.Create<Post>();
        var parentComment = _fixture.Create<Comment>();
        var childComment1 = _fixture.Create<Comment>();
        var childComment2 = _fixture.Create<Comment>();

        // Setup parent-child relationship
        parentComment.PostId = post.PostId;
        parentComment.ParentCommentId = null; // root comment
        childComment1.PostId = post.PostId;
        childComment1.ParentCommentId = parentComment.CommentId;
        childComment2.PostId = post.PostId;
        childComment2.ParentCommentId = parentComment.CommentId;

        // Adding users to comments
        parentComment.User = _fixture.Create<User>();
        childComment1.User = _fixture.Create<User>();
        childComment2.User = _fixture.Create<User>();

        // Create the query with a commentId and pagination metadata
        var query = _fixture.Build<GetCommentsByPostQuery>()
            .With(q => q.PostId, post.PostId)
            .With(q => q.CommentId, parentComment.CommentId) // Querying replies for the parent comment
            .With(q => q.Meta,
                new InfiniteCursorPaginationMeta
                {
                    PageSize = 2, // 2 children
                    Cursor = null // No cursor for the first page
                })
            .Create();

        var comments = new List<Comment> { parentComment, childComment1, childComment2 };


        // Setup DbContext mock
        _mockDbContext.Setup(x => x.Posts).ReturnsDbSet(new List<Post> { post });
        _mockDbContext.Setup(x => x.Comments).ReturnsDbSet(comments);

        var handler = new GetCommentsByPostHandler(_mockDbContext.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count); // Expecting only one reply due to pagination

        var expectedChildren = new List<Guid> { childComment1.CommentId, childComment2.CommentId }
            .OrderBy(id => id)
            .ToList().AsReadOnly();;
        var actualChildren = result.Value.Items
            .Select(c => c.CommentId)
            .OrderBy(id => id)
            .ToList().AsReadOnly();
        Assert.Equal(expectedChildren, actualChildren);
    }
}