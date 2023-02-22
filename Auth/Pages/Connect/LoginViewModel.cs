using System.ComponentModel.DataAnnotations;

namespace Auth.Pages.Connect
{

    public class LoginViewModel
    {

        [Required(ErrorMessage = "请输入登录名")]
        [EmailAddress]
        [Display(Name = "登录名")]

        public string UserName { get; set; } = "179722134@qq.com";

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "密码")]
        public string Password { get; set; } = "123qwe!@#";

        [Required]
        public string ReturnUrl { get; set; } = null!;
    }
}
