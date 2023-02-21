using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages
{
    public class LoginModel : PageModel
    {
        public string? ReturnUrl { get; set; }
        public IActionResult OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl;
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/"
            });
        }
    }
}
