using System.Net;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Commands.CreatePost;
using Backend.Features.Search.Queries.SearchPosts;
using BackendTest.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace BackendTest.Features.Search;

public class SearchTest(IntegrationTestFactory factory) : BaseCqrsIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Search_Posts_Successfully()
    {
        // Arrange - First create some posts to search
        var postContents = new[]
        {
            "Test post with unique keyword xylophone",
            "Another test post about guitars and music",
            "Third test post talking about programming"
        };

        var postIds = new List<Guid>();
        foreach (var content in postContents)
        {
            var createCommand = new CreatePostCommand
            {
                Content = content,
                MediaIds = new List<Guid>()
            };
            var createResult = await Mediator.Send(createCommand);
            Assert.True(createResult.IsSuccess);
            postIds.Add(createResult.Value.Id);
        }

        // Act - Search for posts
        var query = new SearchPostsQuery
        {
            Query = "xylophone",
            PaginationMeta = new InfiniteCursorPaginationMeta
            {
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
    public Task Should_Validate_Search_Query_Parameters()
    {
        // Act & Assert - Empty query
        var emptyQuery = new SearchPostsQuery
        {
            Query = "",
            PaginationMeta = new InfiniteCursorPaginationMeta
            {
                PageSize = 10
            }
        };

        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(emptyQuery);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            emptyQuery, validationContext, validationResults, true);
        
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
        
        // Act & Assert - Whitespace query
        var whitespaceQuery = new SearchPostsQuery
        {
            Query = "   ",
            PaginationMeta = new InfiniteCursorPaginationMeta
            {
                PageSize = 10
            }
        };
        
        var results = whitespaceQuery.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(whitespaceQuery));
        Assert.NotEmpty(results);
        return Task.CompletedTask;
    }
}