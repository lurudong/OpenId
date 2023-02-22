using Auth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Auth.Endpoints
{
    public sealed class UserInfoEndpoints
    {
        public static void ConfigureApplication(WebApplication app)
        {
            //app.MapPost("~/connect/userinfo", UserInfoAsync).ExcludeFromDescription();
            app.MapGet("~/connect/userinfo", UserInfoAsync).ExcludeFromDescription();

        }

        [Authorize]
        private static async Task<IResult> UserInfoAsync(
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager
            )
        {
            var user = await userManager.GetUserAsync(httpContext.User);
            if (user is null)
            {
                return Results.Problem("用户不存在");
            }

            return Results.Ok(user);
        }
    }
}


