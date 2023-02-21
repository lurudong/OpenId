using System.ComponentModel.DataAnnotations;

namespace Auth.Pages.Connect
{

    public class LoginViewModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "用户名")]
        public string UserName { get; set; } = null!;

        [Required]
        [Display(Name = "密码")]
        public string Password { get; set; } = null!;

        [Required]
        public string ReturnUrl { get; set; } = null!;
    }
}
