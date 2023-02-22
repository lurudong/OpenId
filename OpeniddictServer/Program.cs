using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpeniddictServer.Data;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;
namespace OpeniddictServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMvc();
            builder.Services.AddRazorPages();
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(connectionString);
                    options.UseOpenIddict();

                });
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddDefaultTokenProviders()
      .AddDefaultUI();


            //builder.Services.Configure<IdentityOptions>(options =>
            //{

            //    options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            //    options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            //    options.ClaimsIdentity.RoleClaimType = Claims.Role;
            //    options.ClaimsIdentity.EmailClaimType = Claims.Email;

            //    options.SignIn.RequireConfirmedAccount = false;
            //});

            builder.Services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });


            builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            options => options.LoginPath = "/connect/login");
            builder.Services.AddAuthorization();
            builder.Services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                         .UseDbContext<ApplicationDbContext>();

                    options.UseQuartz();
                }).AddServer(options =>
                {
                    // Enable the authorization, logout, token and userinfo endpoints.
                    //options.SetAuthorizationEndpointUris("connect/authorize")
                    //   //.SetDeviceEndpointUris("connect/device")
                    //   .SetIntrospectionEndpointUris("connect/introspect")
                    //   .SetLogoutEndpointUris("connect/logout")
                    //   .SetTokenEndpointUris("connect/token")
                    //   .SetUserinfoEndpointUris("connect/userinfo")
                    //   .SetVerificationEndpointUris("connect/verify");

                    //options.AllowAuthorizationCodeFlow()
                    //       .AllowHybridFlow()
                    //       .AllowPasswordFlow()
                    //       .AllowClientCredentialsFlow()
                    //       .AllowRefreshTokenFlow()
                    //       ;

                    //options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "dataEventRecords");

                    //options.AddDevelopmentEncryptionCertificate()
                    //       .AddDevelopmentSigningCertificate();

                    //// Encryption and signing of tokens
                    //options
                    //    .AddEphemeralEncryptionKey() // only for Developing mode
                    //    .AddEphemeralSigningKey() // only for Developing mode
                    //    .DisableAccessTokenEncryption(); // only for Developing mode

                    //options.UseAspNetCore()
                    //       .EnableAuthorizationEndpointPassthrough()
                    //       .EnableLogoutEndpointPassthrough()
                    //       .EnableTokenEndpointPassthrough()
                    //       .EnableUserinfoEndpointPassthrough()
                    //       .EnableStatusCodePagesIntegration()
                    //       //½ûÓÃhttps
                    //       .DisableTransportSecurityRequirement();


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
                        .AllowAuthorizationCodeFlow()
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
                        // enable PKCE
                        //.SetDeviceEndpointUris("connect/device")
                        //.SetIntrospectionEndpointUris("connect/introspect")
                        .SetLogoutEndpointUris("connect/logout")
                        .SetTokenEndpointUris("connect/token")
                        //.SetVerificationEndpointUris("connect/verify"),
                        .SetUserinfoEndpointUris("connect/userinfo");

                    options.RequireProofKeyForCodeExchange();

                    // Encryption and signing of tokens
                    options
                        .AddEphemeralEncryptionKey() // only for Developing mode
                        .AddEphemeralSigningKey() // only for Developing mode
                        .DisableAccessTokenEncryption(); // only for Developing mode

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "dataEventRecords");

                    // Register the signing and encryption credentials.
                    options
                        .AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                    options.Configure(options => options.CodeChallengeMethods.Add(CodeChallengeMethods.Plain));
                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options
                        .UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough()
                        .DisableTransportSecurityRequirement();
                }).AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            builder.Services.AddHostedService<Worker>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.MapDefaultControllerRoute();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //    endpoints.MapDefaultControllerRoute();
            //    endpoints.MapRazorPages();
            //});
            app.Run();
        }
        private static void SetSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                if (httpContext.Request.Scheme != "https")
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

    }
}