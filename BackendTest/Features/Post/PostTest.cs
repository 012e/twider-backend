using Backend.Common.Helpers.Types;
using Backend.Features.Media.Commands;
using Backend.Features.Post.Commands.AddReaction;
using Backend.Features.Post.Commands.CreatePost;
using Backend.Features.Post.Commands.DeletePost;
using Backend.Features.Post.Commands.DeleteReaction;
using Backend.Features.Post.Commands.UpdatePost;
using Backend.Features.Post.Queries.GetPostById;
using Backend.Features.Post.Queries.GetPosts;
using BackendTest.Utils;

namespace BackendTest.Features.Post;

public class PostTest(IntegrationTestFactory factory) : BaseCqrsIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Create_Post_Successfully()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            Content = "Test Post Content",
            MediaIds = []
        };

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task Should_Get_Post_By_Id_Successfully()
    {
        // Arrange - Create a post first
        var createCommand = new CreatePostCommand
        {
            Content = "Post to retrieve by ID",
            MediaIds = new List<Guid>()
        };
        var createResult = await Mediator.Send(createCommand);
        Assert.True(createResult.IsSuccess);
        var postId = createResult.Value.Id;

        // Act
        var query = new GetPostByIdQuery(postId);
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(postId, result.Value.PostId);
        Assert.Equal("Post to retrieve by ID", result.Value.Content);
    }

    [Fact]
    public async Task Should_Return_Not_Found_For_Nonexistent_Post_Id()
    {
        // Arrange
        var nonExistentPostId = Guid.NewGuid();

        // Act
        var query = new GetPostByIdQuery(nonExistentPostId);
        var result = await Mediator.Send(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Status);
    }

    [Fact]
    public async Task Should_Get_Multiple_Posts()
    {
        // Arrange - Create multiple posts
        for (int i = 0; i < 3; i++)
        {
            var createCommand = new CreatePostCommand
            {
                Content = $"Test post {i}",
                MediaIds = new List<Guid>()
            };
            var createResult = await Mediator.Send(createCommand);
            Assert.True(createResult.IsSuccess);
        }

        // Act
        var query = new GetPostsQuery
        {
            PaginationMeta = new InfiniteCursorPaginationMeta
            {
                Cursor = null,
                PageSize = 10
            }
        };
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.Items);
    }

    [Fact]
    public async Task Should_Update_Post_Successfully()
    {
        // Arrange - Create a post first
        var createCommand = new CreatePostCommand
        {
            Content = "Original content",
            MediaIds = []
        };
        var createResult = await Mediator.Send(createCommand);
        Assert.True(createResult.IsSuccess);
        var postId = createResult.Value.Id;

        // Act - Update the post
        var updateCommand = new UpdatePostCommand
        {
            PostId = postId,
            Content = new UpdatePostCommand.UpdateContent
            {
                Content = "Updated content"
            }
        };
        var updateResult = await Mediator.Send(updateCommand);

        // Assert update was successful
        Assert.True(updateResult.IsSuccess);

        // Verify the content was updated
        var query = new GetPostByIdQuery(postId);
        var queryResult = await Mediator.Send(query);

        Assert.True(queryResult.IsSuccess);
        Assert.Equal("Updated content", queryResult.Value.Content);
    }

    [Fact]
    public async Task Should_Delete_Post_Successfully()
    {
        // Arrange - Create a post first
        var createCommand = new CreatePostCommand
        {
            Content = "Post to be deleted",
            MediaIds = new List<Guid>()
        };
        var createResult = await Mediator.Send(createCommand);
        Assert.True(createResult.IsSuccess);
        var postId = createResult.Value.Id;

        // Act - Delete the post
        var deleteCommand = new DeletePostCommand(postId);
        var deleteResult = await Mediator.Send(deleteCommand);

        // Assert deletion was successful
        Assert.True(deleteResult.IsSuccess);

        // Verify the post no longer exists
        var query = new GetPostByIdQuery(postId);
        var queryResult = await Mediator.Send(query);

        Assert.False(queryResult.IsSuccess);
        Assert.Equal(404, queryResult.Error?.Status);
    }

    [Fact]
    public async Task Should_Add_Reaction_To_Post()
    {
        // Arrange - Create a post first
        var createCommand = new CreatePostCommand
        {
            Content = "Post for reaction test",
            MediaIds = new List<Guid>()
        };
        var createResult = await Mediator.Send(createCommand);
        Assert.True(createResult.IsSuccess);
        var postId = createResult.Value.Id;

        // Act - Add a reaction
        var reactionCommand = new PostReactionCommand
        {
            PostId = postId,
            ReactionType = new PostReactionCommand.ReactionDto
            {
                ReactionType = ReactionType.Like
            }
        };
        var reactionResult = await Mediator.Send(reactionCommand);

        // Assert
        Assert.True(reactionResult.IsSuccess);

        // Verify the reaction was added
        var query = new GetPostByIdQuery(postId);
        var queryResult = await Mediator.Send(query);

        Assert.True(queryResult.IsSuccess);
        Assert.NotNull(queryResult.Value.UserReaction);
        Assert.Equal("like", queryResult.Value.UserReaction);
        Assert.True(queryResult.Value.ReactionCount > 0);
    }

    [Fact]
    public async Task Should_Remove_Reaction_From_Post()
    {
        // Arrange - Create a post first
        var createCommand = new CreatePostCommand
        {
            Content = "Post for reaction removal test",
            MediaIds = new List<Guid>()
        };
        var createResult = await Mediator.Send(createCommand);
        Assert.True(createResult.IsSuccess);
        var postId = createResult.Value.Id;

        // Add a reaction first
        var reactionCommand = new PostReactionCommand
        {
            PostId = postId,
            ReactionType = new PostReactionCommand.ReactionDto
            {
                ReactionType = ReactionType.Like
            }
        };
        var reactionResult = await Mediator.Send(reactionCommand);
        Assert.True(reactionResult.IsSuccess);

        // Verify reaction was added
        var query = new GetPostByIdQuery(postId);
        var queryResult = await Mediator.Send(query);
        Assert.True(queryResult.IsSuccess);
        Assert.NotNull(queryResult.Value.UserReaction);

        // Act - Remove the reaction
        var removeReactionCommand = new RemoveReactionCommand
        {
            PostId = postId
        };
        var removeResult = await Mediator.Send(removeReactionCommand);

        // Assert
        Assert.True(removeResult.IsSuccess);

        // Verify the reaction was removed
        var verifyQuery = new GetPostByIdQuery(postId);
        var verifyResult = await Mediator.Send(verifyQuery);

        // The UserReaction should be null now
        Assert.True(verifyResult.IsSuccess);
        Assert.Equal(0, verifyResult.Value.ReactionCount);
    }

    [Fact]
    public async Task Should_Create_Post_With_Media_Successfully()
    {
        // Arrange
        // First generate media URLs and IDs
        var mediaCommand = new GenerateUploadUrlCommand();
        var mediaResult = await Mediator.Send(mediaCommand);
        Assert.True(mediaResult.IsSuccess);

        var mediaId = mediaResult.Value.MediumId;

        // Create post with media
        var command = new CreatePostCommand
        {
            Content = "Test post with media attachment",
            MediaIds = new List<Guid> { mediaId }
        };

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);

        // Verify media is attached to post
        var postQuery = new GetPostByIdQuery(result.Value.Id);
        var postResult = await Mediator.Send(postQuery);

        Assert.True(postResult.IsSuccess);
        Assert.NotEmpty(postResult.Value.MediaUrls);
    }
}