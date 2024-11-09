using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

//[Authorize] //! Pour exiger que l’utilisateur soit authentifié
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel login)
    {
        // Exemple simplifié : Validez l'utilisateur dans la base de données ici
        if (login.Username == "agent" && login.Password == "password") // Remplacez par la validation réelle
        {
            var token = GenerateJwtToken("agentId", "Acheteur");
            return Ok(new { token });
        }
        return Unauthorized();
    }

    private string GenerateJwtToken(string agentId, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
        var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, agentId),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
