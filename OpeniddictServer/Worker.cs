using OpenIddict.Abstractions;
using OpeniddictServer.Data;
using System.Globalization;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpeniddictServer
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            await RegisterApplicationsAsync(scope.ServiceProvider);


            static async Task RegisterApplicationsAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();


                if (await manager.FindByClientIdAsync("openiddictclient") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "openiddictclient",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "Mvc code PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("fr-FR")] = "Application cliente MVC"
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("http://localhost:5285/signout-callback-oidc")
                        },
                        RedirectUris =
                        {
                            new Uri("http://localhost:5285/signin-oidc")
                        },
                        ClientSecret = "codeflow_pkce_client_secret",
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,
                            Permissions.Endpoints.Revocation,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.ResponseTypes.Code,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "dataEventRecords"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }

                if (await manager.FindByClientIdAsync("mvc") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "mvc",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "Mvc code PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("fr-FR")] = "Application cliente MVC"
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("http://localhost:5235/signout-callback-oidc")
                        },
                        RedirectUris =
                        {
                            //"https://localhost:44338/callback/login/local"
                            new Uri("http://localhost:5235/callback/login/local")
                        },
                        ClientSecret = "mvc_pkce_client_secret",
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,
                            Permissions.Endpoints.Revocation,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.ResponseTypes.Code,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "dataEventRecords"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }

                const string client_id2 = "client-id-code";
                if (await manager.FindByClientIdAsync(client_id2) is null)
                {

                    var url = "http://localhost:5284";
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = client_id2,
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "web api code PKCE",
                        RedirectUris =
                        {
                             new Uri($"{url}/swagger/oauth2-redirect.html")
                        },

                        ClientSecret = "client-secret-code",
                        Permissions =
                        {
                     OpenIddictConstants.Permissions.Endpoints.Authorization,
                     OpenIddictConstants.Permissions.Endpoints.Token,

                    // Grant type permissions
                     OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                     OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                    // Scope permissions
                      Permissions.Prefixes.Scope + "dataEventRecords",

                    // Response types
                       OpenIddictConstants.Permissions.ResponseTypes.Code,
                       OpenIddictConstants.Permissions.ResponseTypes.IdToken
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }
            }

        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
