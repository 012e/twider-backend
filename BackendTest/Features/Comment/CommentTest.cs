using Backend.Common.Helpers.Types;
using Backend.Features.Comment.Commands.AddReaction;
using Backend.Features.Comment.Commands.CreateComment;
using Backend.Features.Comment.Commands.DeleteComment;
using Backend.Features.Comment.Commands.DeleteReaction;
using Backend.Features.Comment.Commands.UpdateComment;
using Backend.Features.Comment.Queries;
using Backend.Features.Post.Commands.CreatePost;
using BackendTest.Utils;

namespace BackendTest.Features.Comment;

public class CommentTest(IntegrationTestFactory factory) : BaseCqrsIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Create_Comment_Successfully()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Test post for comments",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create comment command
        var command = new CreateCommentCommand
        {
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Test comment content"
            }
        };

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task Should_Create_Reply_To_Comment_Successfully()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Test post for reply",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create parent comment
        var parentCommentCommand = new CreateCommentCommand
        {
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Parent comment"
            }
        };
        var parentCommentResult = await Mediator.Send(parentCommentCommand);
        Assert.True(parentCommentResult.IsSuccess);
        var parentCommentId = parentCommentResult.Value.Id;

        // Create reply command
        var replyCommand = new CreateCommentCommand
        {
            PostId = postId,
            ParentCommentId = parentCommentId,
            Content = new CommentContent
            {
                Content = "Reply to parent comment"
            }
        };

        // Act
        var result = await Mediator.Send(replyCommand);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task Should_Get_Comments_By_Post_Successfully()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Post for getting comments",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Add multiple comments to the post
        for (int i = 0; i < 3; i++)
        {
            var commentCommand = new CreateCommentCommand
            {
                PostId = postId,
                Content = new CommentContent
                {
                    Content = $"Test comment {i}"
                }
            };
            var commentResult = await Mediator.Send(commentCommand);
            Assert.True(commentResult.IsSuccess);
        }

        // Act
        var query = new GetCommentsByPostQuery(
            postId,
            null,
            new InfiniteCursorPaginationMeta
            {
                PageSize = 10
            });
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.Items);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task Should_Get_Replies_To_Comment_Successfully()
    {
        // Arrange
        // First create a post 
        var postCommand = new CreatePostCommand
        {
            Content = "Post for getting replies",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create parent comment
        var parentCommentCommand = new CreateCommentCommand
        {
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Parent comment for replies"
            }
        };
        var parentCommentResult = await Mediator.Send(parentCommentCommand);
        Assert.True(parentCommentResult.IsSuccess);
        var parentCommentId = parentCommentResult.Value.Id;

        // Add multiple replies to the parent comment
        for (int i = 0; i < 3; i++)
        {
            var replyCommand = new CreateCommentCommand
            {
                PostId = postId,
                ParentCommentId = parentCommentId,
                Content = new CommentContent
                {
                    Content = $"Reply {i} to parent comment"
                }
            };
            var replyResult = await Mediator.Send(replyCommand);
            Assert.True(replyResult.IsSuccess);
        }

        // Act
        var query = new GetCommentsByPostQuery(
            postId,
            parentCommentId,
            new InfiniteCursorPaginationMeta
            {
                PageSize = 10
            });
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.Items);
        Assert.Equal(3, result.Value.Items.Count);
        foreach (var reply in result.Value.Items)
        {
            Assert.Equal(parentCommentId, reply.ParentCommentId);
        }
    }

    [Fact]
    public async Task Should_Return_Empty_For_NonExistent_Post()
    {
        // Arrange
        var nonExistentPostId = Guid.NewGuid();

        // Act
        var query = new GetCommentsByPostQuery(
            nonExistentPostId,
            null,
            new InfiniteCursorPaginationMeta
            {
                PageSize = 10
            });
        var result = await Mediator.Send(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Status);
    }

    [Fact]
    public async Task Should_Update_Comment_Successfully()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Test post for updating comment",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create a comment
        var createCommentCommand = new CreateCommentCommand
        {
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Original comment content"
            }
        };
        var commentResult = await Mediator.Send(createCommentCommand);
        Assert.True(commentResult.IsSuccess);
        var commentId = commentResult.Value.Id;

        // Act - Update the comment
        var updateCommand = new UpdateCommentCommand
        {
            CommentId = commentId,
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Updated comment content"
            }
        };
        var updateResult = await Mediator.Send(updateCommand);

        // Assert
        Assert.True(updateResult.IsSuccess);

        // Verify the content was updated by fetching comments
        var query = new GetCommentsByPostQuery(
            postId,
            null,
            new InfiniteCursorPaginationMeta
            {
                PageSize = 10
            });
        var commentsResult = await Mediator.Send(query);

        Assert.True(commentsResult.IsSuccess);
        var updatedComment = commentsResult.Value.Items.FirstOrDefault(c => c.CommentId == commentId);
        Assert.NotNull(updatedComment);
        Assert.Equal("Updated comment content", updatedComment.Content);
    }

    [Fact]
    public async Task Should_Delete_Comment_Successfully()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Test post for deleting comment",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create a comment
        var createCommentCommand = new CreateCommentCommand
        {
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Comment to be deleted"
            }
        };
        var commentResult = await Mediator.Send(createCommentCommand);
        Assert.True(commentResult.IsSuccess);
        var commentId = commentResult.Value.Id;

        // Act - Delete the comment
        var deleteCommand = new DeleteCommentCommand
        {
            CommentId = commentId,
            PostId = postId
        };
        var deleteResult = await Mediator.Send(deleteCommand);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        // Verify the comment was deleted by fetching comments
        var query = new GetCommentsByPostQuery(
            postId,
            null,
            new InfiniteCursorPaginationMeta
            {
                PageSize = 10
            });
        var commentsResult = await Mediator.Send(query);

        Assert.True(commentsResult.IsSuccess);
        Assert.Empty(commentsResult.Value.Items);
    }

    [Fact]
    public async Task Should_Add_Reaction_To_Comment()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Test post for comment reaction",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create a comment
        var createCommentCommand = new CreateCommentCommand
        {
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Comment for reaction test"
            }
        };
        var commentResult = await Mediator.Send(createCommentCommand);
        Assert.True(commentResult.IsSuccess);
        var commentId = commentResult.Value.Id;

        // Act - Add a reaction
        var reactionCommand = new AddReactionCommand
        {
            CommentId = commentId,
            PostId = postId,
            ReactionType = new AddReactionCommand.ReactionDto
            {
                ReactionType = ReactionType.Like
            }
        };
        var reactionResult = await Mediator.Send(reactionCommand);

        // Assert
        Assert.True(reactionResult.IsSuccess);
    }

    [Fact]
    public async Task Should_Remove_Reaction_From_Comment()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Test post for removing comment reaction",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create a comment
        var createCommentCommand = new CreateCommentCommand
        {
            PostId = postId,
            Content = new CommentContent
            {
                Content = "Comment for reaction removal test"
            }
        };
        var commentResult = await Mediator.Send(createCommentCommand);
        Assert.True(commentResult.IsSuccess);
        var commentId = commentResult.Value.Id;

        // Add a reaction first
        var addReactionCommand = new AddReactionCommand
        {
            CommentId = commentId,
            PostId = postId,
            ReactionType = new AddReactionCommand.ReactionDto
            {
                ReactionType = ReactionType.Like
            }
        };
        var addReactionResult = await Mediator.Send(addReactionCommand);
        Assert.True(addReactionResult.IsSuccess);

        // Act - Remove the reaction
        var deleteReactionCommand = new DeleteReactionCommand
        {
            CommentId = commentId,
            PostId = postId
        };
        var deleteReactionResult = await Mediator.Send(deleteReactionCommand);

        // Assert
        Assert.True(deleteReactionResult.IsSuccess);
    }
}