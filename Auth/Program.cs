
using Auth.Contexts;
using Auth.Endpoints;
using Auth.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using System.Linq.Expressions;
using System.Reflection;

namespace Auth
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            // Add services to the container.
            builder.Services.AddMvc();
            builder.Services.AddRazorPages();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "auth",
                    Version = "0.1"
                });

                options.ResolveConflictingActions(x => x.First());
            });

            #region db
            services.AddDbContext<ApplicationDbContext>(options =>
            {

                options.UseInMemoryDatabase("DEMO-PURPOSES-ONLY");
                options.UseOpenIddict<Guid>();
            });

            #endregion


            #region Identity
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
                // configure more options if you need
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
            })
          .AddSignInManager()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddUserManager<UserManager<ApplicationUser>>().AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
          .AddDefaultTokenProviders();
            #endregion

            #region
            services
                 .AddAuthentication(options =>
                 {
                     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                 })
                 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                 {
                     options.LoginPath = "/connect/login";
                 });

            services.AddAuthorization();
            #endregion

            #region OpenIddict
            services.AddOpenIddict()
            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores and models.
                // Note: call ReplaceDefaultEntities() to replace the default entities.
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Note: the sample uses the code and refresh token flows but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                // Supported flows are:
                //  => Authorization code flow
                //  => Client credentials flow
                //  => Device code flow
                //  => Implicit flow
                //  => Password flow
                //  => Refresh token flow
                options
                    .AllowAuthorizationCodeFlow()//.RequireProofKeyForCodeExchange()
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                // Using reference tokens means the actual access and refresh tokens
                // are stored in the database and different tokens, referencing the actual
                // tokens (in the db), are used in request headers. The actual tokens are not
                // made public.
                // => options.UseReferenceAccessTokens();
                // => options.UseReferenceRefreshTokens();

                // Set the lifetime of your tokens
                // => options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                // => options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                // Enable the token endpoint.
                options.SetAuthorizationEndpointUris("connect/authorize")
                    //.RequireProofKeyForCodeExchange() // enable PKCE
                    //.SetDeviceEndpointUris("connect/device")
                    //.SetIntrospectionEndpointUris("connect/introspect")
                    .SetLogoutEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token")
                    //.SetVerificationEndpointUris("connect/verify"),
                    .SetUserinfoEndpointUris("connect/userinfo");

                // Encryption and signing of tokens
                options
                    .AddEphemeralEncryptionKey() // only for Developing mode
                    .AddEphemeralSigningKey() // only for Developing mode
                    .DisableAccessTokenEncryption(); // only for Developing mode

                // Mark the "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    "api",
                    "custom");

                // Register the signing and encryption credentials.
                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core options.
                options
                    .UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough();


            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });
            services.AddTransient<ApplicationUserClaimsPrincipalFactory>();
            services.AddHostedService<Worker>();
            #endregion



            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();

                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.SetIsOriginAllowed(host => true);
                    builder.AllowCredentials();

                });
            });

            var types = typeof(Program).Assembly.ExportedTypes.Where(x => !x.IsAbstract && typeof(EndpointsBase).IsAssignableFrom(x));
            //foreach (var item in types)
            //{
            //    builder.Services.AddScoped(item, item);
            //}
            builder.Services.AddScoped<TestEndpoints>();
            var app = builder.Build();
            #region

            var methods = types.SelectMany(o => o.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)).ToList();

            foreach (MethodInfo methodInfo in methods)
            {

                using var scope = app.Services.CreateScope();
                var servicesType = scope.ServiceProvider.GetService(methodInfo.DeclaringType!);
                //var servicesType = app.Services.GetRequiredService<TestEndpoints>();
                var type = Expression.GetDelegateType(
                     methodInfo.GetParameters().

                         Select(parameterInfo => parameterInfo.ParameterType)
                .Concat(new List<Type>
                    { methodInfo.ReturnType }).ToArray());
                var instance = Delegate.
                    CreateDelegate(type, servicesType, methodInfo);
                //app.MapGet("/api/get/", [Microsoft.AspNetCore.Authorization.AuthorizeAttribute] () => { 


                //});
                app.MapMethods("/api/test/getasync", new[] { HttpMethods.Get }, instance);

            }


            #endregion
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            //app.UseAuthorization();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();
            app.MapControllers();

            await SeedUsers(app.Services);
            await app.RunAsync();

        }



        private static async Task SeedUsers(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();


            await using var context = scope.ServiceProvider.GetService<ApplicationDbContext>();



            if (context.Users.Any())
            {
                return;
            }


            var role = "admin";

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!context!.Roles.Any(r => r.Name == role))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = role, NormalizedName = role.ToUpper() });
            }

            #region developer

            var developer1 = new ApplicationUser
            {
                Email = "179722134@qq.com",
                NormalizedEmail = "179722134@QQ.COM",
                UserName = "179722134@qq.com",
                NormalizedUserName = "179722134QQ.COM",
                PhoneNumber = "13560360522",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
            };

            if (!context!.Users.Any(u => u.UserName == developer1.UserName))
            {
                var password = new PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(developer1, "123qwe!@#");
                developer1.PasswordHash = hashed;
                var userStore = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var result = await userStore.CreateAsync(developer1);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Cannot create account");
                }

                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var roleAdded = await userManager!.AddToRoleAsync(developer1, role);
                if (roleAdded.Succeeded)
                {
                    await context.SaveChangesAsync();
                }
            }

            #endregion

            await context.SaveChangesAsync();
        }
    }


}