using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //不是跳转会报pii错误
            IdentityModelEventSource.ShowPII = true;
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddTransient<AuthorizedHandler>();
            // Add services to the container.
            //builder.Services.AddRazorPages();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.SignInScheme = "Cookies";
        options.Authority = "http://localhost:5068";
        options.ClientId = "openiddictclient";
        options.ClientSecret = "codeflow_pkce_client_secret";
        options.RequireHttpsMetadata = false;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.Scope.Add("profile");
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
            //builder.Services.AddAuthorization(options => options.AddPolicy("CookieAuthenticationPolicy", builder =>
            //{
            //    builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
            //    builder.RequireAuthenticatedUser();
            //}));

            builder.Services.AddHttpClient();
            builder.Services.AddRazorPages();

            //builder.Services.AddControllersWithViews();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }


            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(options =>
            {
                options.MapRazorPages();
                options.MapControllers();
                options.MapDefaultControllerRoute();
            });
            app.Urls.Add("http://localhost:5285");
            //app.MapRazorPages();
            //app.MapControllers();
            //app.MapDefaultControllerRoute();
            app.Run();
        }
    }
}