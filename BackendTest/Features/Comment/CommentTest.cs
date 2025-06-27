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
        Assert.NotEqual(Guid.Empty, result.Value.CommentId);
        Assert.Equal("Test comment content", result.Value.Content);
        Assert.Null(result.Value.ParentCommentId);
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
        var parentCommentId = parentCommentResult.Value.CommentId;

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
        Assert.NotEqual(Guid.Empty, result.Value.CommentId);
        Assert.Equal("Reply to parent comment", result.Value.Content);
        Assert.Equal(parentCommentId, result.Value.ParentCommentId);
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
        var parentCommentId = parentCommentResult.Value.CommentId;

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
        var commentId = commentResult.Value.CommentId;

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
        var commentId = commentResult.Value.CommentId;

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
        var commentId = commentResult.Value.CommentId;

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
        var commentId = commentResult.Value.CommentId;

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

    [Fact]
    public async Task Should_Return_Correct_Pagination_Data_With_Unique_Comment_Ids()
    {
        // Arrange
        // First create a post to comment on
        var postCommand = new CreatePostCommand
        {
            Content = "Test post for pagination validation",
            MediaIds = new List<Guid>()
        };
        var postResult = await Mediator.Send(postCommand);
        Assert.True(postResult.IsSuccess);
        var postId = postResult.Value.Id;

        // Create multiple comments to test pagination
        var totalComments = 15;
        var createdCommentIds = new List<Guid>();
        
        for (int i = 0; i < totalComments; i++)
        {
            var commentCommand = new CreateCommentCommand
            {
                PostId = postId,
                Content = new CommentContent
                {
                    Content = $"Test comment {i:D2} for pagination" // Zero-padded for consistent ordering
                }
            };
            var commentResult = await Mediator.Send(commentCommand);
            Assert.True(commentResult.IsSuccess);
            createdCommentIds.Add(commentResult.Value.CommentId);
        }

        // Act - First page
        var firstPageSize = 5;
        var firstPageQuery = new GetCommentsByPostQuery(
            postId,
            null,
            new InfiniteCursorPaginationMeta
            {
                PageSize = firstPageSize
            });
        var firstPageResult = await Mediator.Send(firstPageQuery);

        // Assert first page
        Assert.True(firstPageResult.IsSuccess);
        Assert.NotNull(firstPageResult.Value);
        Assert.Equal(firstPageSize, firstPageResult.Value.Items.Count);
        Assert.True(firstPageResult.Value.HasMore); // Should have more items
        Assert.NotNull(firstPageResult.Value.NextCursor); // Should have next cursor

        // Validate all comment IDs in first page are unique GUIDs
        var firstPageCommentIds = firstPageResult.Value.Items.Select(x => x.CommentId).ToList();
        Assert.Equal(firstPageSize, firstPageCommentIds.Distinct().Count()); // All IDs should be unique
        Assert.All(firstPageCommentIds, id => Assert.NotEqual(Guid.Empty, id)); // No empty GUIDs

        // Act - Second page using cursor from first page
        var secondPageQuery = new GetCommentsByPostQuery(
            postId,
            null,
            new InfiniteCursorPaginationMeta
            {
                Cursor = firstPageResult.Value.NextCursor,
                PageSize = firstPageSize
            });
        var secondPageResult = await Mediator.Send(secondPageQuery);

        // Assert second page
        Assert.True(secondPageResult.IsSuccess);
        Assert.NotNull(secondPageResult.Value);
        Assert.Equal(firstPageSize, secondPageResult.Value.Items.Count);
        Assert.True(secondPageResult.Value.HasMore); // Should still have more items
        Assert.NotNull(secondPageResult.Value.NextCursor); // Should have next cursor

        // Validate all comment IDs in second page are unique GUIDs
        var secondPageCommentIds = secondPageResult.Value.Items.Select(x => x.CommentId).ToList();
        Assert.Equal(firstPageSize, secondPageCommentIds.Distinct().Count()); // All IDs should be unique
        Assert.All(secondPageCommentIds, id => Assert.NotEqual(Guid.Empty, id)); // No empty GUIDs

        // Validate no overlap between first and second page
        var overlappingIds = firstPageCommentIds.Intersect(secondPageCommentIds);
        Assert.Empty(overlappingIds); // No IDs should appear in both pages

        // Act - Third page (final page)
        var thirdPageQuery = new GetCommentsByPostQuery(
            postId,
            null,
            new InfiniteCursorPaginationMeta
            {
                Cursor = secondPageResult.Value.NextCursor,
                PageSize = firstPageSize
            });
        var thirdPageResult = await Mediator.Send(thirdPageQuery);

        // Assert third page
        Assert.True(thirdPageResult.IsSuccess);
        Assert.NotNull(thirdPageResult.Value);
        Assert.Equal(totalComments - (firstPageSize * 2), thirdPageResult.Value.Items.Count); // Remaining items
        Assert.False(thirdPageResult.Value.HasMore); // Should not have more items
        Assert.NotNull(thirdPageResult.Value.NextCursor); // Still should have cursor for consistency

        // Validate all comment IDs in third page are unique GUIDs
        var thirdPageCommentIds = thirdPageResult.Value.Items.Select(x => x.CommentId).ToList();
        Assert.Equal(thirdPageCommentIds.Count, thirdPageCommentIds.Distinct().Count()); // All IDs should be unique
        Assert.All(thirdPageCommentIds, id => Assert.NotEqual(Guid.Empty, id)); // No empty GUIDs

        // Validate no overlap between any pages
        var allPageIds = firstPageCommentIds.Concat(secondPageCommentIds).Concat(thirdPageCommentIds).ToList();
        Assert.Equal(totalComments, allPageIds.Count); // Should have all comments
        Assert.Equal(totalComments, allPageIds.Distinct().Count()); // All should be unique

        // Validate all created comments are present in the paginated results
        var allReturnedIds = allPageIds.ToHashSet();
        Assert.All(createdCommentIds, id => Assert.Contains(id, allReturnedIds));

        // Validate comment structure integrity
        var allComments = firstPageResult.Value.Items
            .Concat(secondPageResult.Value.Items)
            .Concat(thirdPageResult.Value.Items)
            .ToList();

        foreach (var comment in allComments)
        {
            // Validate comment structure
            Assert.NotEqual(Guid.Empty, comment.CommentId);
            Assert.NotNull(comment.Content);
            Assert.NotEmpty(comment.Content);
            Assert.True(comment.Content.StartsWith("Test comment")); // Should match our pattern
            Assert.Null(comment.ParentCommentId); // These are top-level comments
            Assert.Equal(0, comment.TotalReplies); // No replies added
            Assert.True(comment.CreatedAt > DateTime.MinValue);
            
            // Validate user data
            Assert.NotNull(comment.User);
            Assert.NotEqual(Guid.Empty, comment.User.UserId);
            Assert.NotNull(comment.User.Username);
            Assert.NotNull(comment.User.OauthSub);
        }
    }
}