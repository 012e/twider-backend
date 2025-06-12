using Backend.Common.DbContext;
using Backend.Common.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTest.Utils;

public class BaseCqrsIntegrationTest : IClassFixture<IntegrationTestFactory>
{
    private readonly IServiceScope _scope;
    protected readonly User CurrentUser;
    protected readonly IMediator Mediator;

    public BaseCqrsIntegrationTest(IntegrationTestFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Mediator = _scope.ServiceProvider.GetService<IMediator>() ??
                   throw new InvalidOperationException("IMediator service not found");
        CurrentUser = _scope.ServiceProvider.GetService<ICurrentUserService>()!.User!;
    }
}