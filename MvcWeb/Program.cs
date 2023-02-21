using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Server.AspNetCore;

namespace MvcWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
            })
 .AddCookie()
 .AddOpenIdConnect(options =>
 {
     //options.SignInScheme = "Cookies";
     options.Authority = "http://localhost:5068";

     //options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
     options.ClientId = "mvc";
     options.ClientSecret = "mvc_pkce_client_secret";
     options.RequireHttpsMetadata = false;
     options.ResponseType = OpenIdConnectResponseType.Code;
     /*     options.UsePkce = true*/
     ;
     ;
     options.Scope.Add("profile");
     options.SaveTokens = true;
     options.GetClaimsFromUserInfoEndpoint = true;
     options.TokenValidationParameters = new TokenValidationParameters
     {
         NameClaimType = "name",
         RoleClaimType = "role"
     };
     options.Events = new OpenIdConnectEvents
     {
         OnRemoteFailure = context =>
         {
             context.Response.Redirect("/");
             context.HandleResponse();

             return Task.FromResult(0);
         }
     };
 });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    //policy.RequireClaim("scope", "api");
                });
            });

            builder.Services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

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