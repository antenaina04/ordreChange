using Microsoft.IdentityModel.Tokens;
using ordreChange.Models;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services.Interfaces;
using ordreChange.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ordreChange.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAgentRepository agentRepository, IConfiguration configuration)
        {
            _agentRepository = agentRepository;
            _configuration = configuration;
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var agent = await _agentRepository.GetByUsernameAsync(username);
            /*
            // Utilisation PWD déjà crypté dans la base de données
            if (agent == null || !VerifyPasswordHash(password, agent.PasswordHash))
                return null;
            */
            if (agent == null)
                return null;

            // DEVELOPMENT/TEST mode
            if (password == agent.PasswordHash)
            {
                return GenerateJwtToken(agent);
            }

            // PRODUCTION mode
            if (!VerifyPasswordHash(password, agent.PasswordHash))
                return null;
            return GenerateJwtToken(agent);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == storedHash;
            }
        }
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }
        private string GenerateJwtToken(Agent agent)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            string secureKey = jwtSettings["Secret"] ?? SecurityHelper.GenerateSecureKey(32);
            var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secureKey));
            var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, agent.IdAgent.ToString()),
                new Claim(ClaimTypes.Role, agent.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()) // Date d'émission
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
