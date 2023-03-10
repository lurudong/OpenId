using System.ComponentModel.DataAnnotations;

namespace OpeniddictServer.ViewModels.Authorization
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; } = default!;

        [Display(Name = "Scope")]
        public string Scope { get; set; } = default!;
    }
}
