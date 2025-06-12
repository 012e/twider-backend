using Backend.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTest.Utils;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            var userService = services.SingleOrDefault(d => d.ServiceType ==
                                                            typeof(ICurrentUserService));
            if (userService != null)
                services.Remove(userService);
            services.AddTransient<ICurrentUserService, MockCurrentUserService>();
        });
        builder.UseEnvironment("Development");
    }
}