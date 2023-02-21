using Auth.Contexts;
using OpenIddict.Abstractions;

namespace Auth
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();



            const string client_id = "client-id-code";
            if (await manager.FindByClientIdAsync(client_id, cancellationToken) is null)
            {
                var url = _serviceProvider.GetRequiredService<IConfiguration>().GetValue<string>("AuthServer:Url");

                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = client_id,
                    ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                    ClientSecret = "client-secret-code",
                    DisplayName = "API testing clients with Authorization Code Flow demonstration",
                    RedirectUris = {

                    new Uri("https://localhost:20002/swagger/oauth2-redirect.html"),
                  },

                    Permissions =
                {
                    // Endpoint permissions
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,

                    // Grant type permissions
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                    // Scope permissions
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "custom",

                    // Response types
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.ResponseTypes.IdToken
                }
                }, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
