using Backend.Features.Post.Commands.CreatePost;
using BackendTest.Utils;

namespace BackendTest.Features;

public class Smoke(IntegrationTestFactory factory) : BaseCqrsIntegrationTest(factory)
{
    [Fact]
    public async Task should_be_able_to_create_post()
    {
        var command = new CreatePostCommand
        {
            Content = "Test Post",
        };

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}