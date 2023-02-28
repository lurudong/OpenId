using Auth.Model;
using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Auth.Endpoints
{
    internal sealed class UserInfoEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("~/connect/userinfo", UserInfoAsync).ExcludeFromDescription();
        }



        [Authorize]
        private async Task<IResult> UserInfoAsync(
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


