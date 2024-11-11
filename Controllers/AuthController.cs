using Microsoft.AspNetCore.Mvc;
using ordreChange.Services.Interfaces;

namespace ordreChange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var token = await _authService.AuthenticateAsync(login.Username, login.Password);
            if (token == null)
                return Unauthorized();

            return Ok(new { token });
        }
    }

    public class LoginModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
