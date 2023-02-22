using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace WebClient_1.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private readonly IHttpContextAccessor _contextAccessor;
        private Uri url = new Uri("https://localhost:20001/", UriKind.Absolute);
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpContextAccessor contextAccessor, IHttpClientFactory httpClientFactory)
        {
            _contextAccessor = contextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]

        public async Task<IActionResult> GetUser()
        {


            var re = "^Bearer (?<token>[a-zA-Z0-9-:._~+/]+=*)$";

            var authorization = _contextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
            var match = System.Text.RegularExpressions.Regex.Match(authorization!, re);
            var token = match.Groups["token"].Value;


            using var request = new HttpRequestMessage(HttpMethod.Get, $"{url.AbsoluteUri}connect/userinfo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var message = await _httpClientFactory.CreateClient().SendAsync(request);
            message.EnsureSuccessStatusCode();
            return new JsonResult(await message.Content.ReadAsStringAsync());
        }
    }
}
