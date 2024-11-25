using Microsoft.IdentityModel.Tokens;
using NLog;
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
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public AuthService(IAgentRepository agentRepository, IConfiguration configuration)
        {
            _agentRepository = agentRepository;
            _configuration = configuration;
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            Logger.Info("Authentication attempt for user: {Username}", username);
            try
            {
                var agent = await _agentRepository.GetByUsernameAsync(username);

                if (agent == null)
                {
                    Logger.Warn("Agent {Username} not found.", username);
                    throw new KeyNotFoundException($"User '{username}' not found.");
                }

                // Si mot de passe dans la base de données n'est pas hashé 
                if (password == agent.PasswordHash)
                {
                    Logger.Info("User {Username} authentication successful.", username);
                    return GenerateJwtToken(agent);
                }

                // Vérification si le mot de passe enregistré dans la base de données est déjà hashé
                if (!VerifyPasswordHash(password, agent.PasswordHash))
                {
                    Logger.Warn("Password verification failed for user {Username}.", username);
                    throw new UnauthorizedAccessException("Wrong password");
                }
                Logger.Info("User {Username} authentication successful.", username);
                return GenerateJwtToken(agent);
            }
            catch (KeyNotFoundException ex)
            {
                Logger.Warn(ex, "Authentication failure for user {Username} : {Message}", username, ex.Message);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Warn(ex, "Authentication failure for user {Username} : {Message}", username, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Authentication failure for user {Username}", username);
                throw; // Relancer l'exception pour la gestion en amont
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            try
            {

                using (var sha256 = SHA256.Create())
                {
                    var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                    return computedHash == storedHash;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error verifying password hash.");
                throw;
            }
        }
        public string HashPassword(string password)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error has occurred while hashing the password.");
                throw new InvalidOperationException("An error has occurred while hashing the password.", ex);
            }
        }
        private string GenerateJwtToken(Agent agent)
        {
            try
            {
                Logger.Info("JWT token generation for user {AgentId}.", agent.IdAgent);
                var jwtSettings = _configuration.GetSection("JwtSettings");
                string secureKey = jwtSettings["Secret"] ?? SecurityHelper.GenerateSecureKey(32);
                var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secureKey));
                var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                if (agent.Role == null)
                {
                    Logger.Warn("Agent role {AgentId} is not loaded.", agent.IdAgent);
                    throw new InvalidOperationException("The agent's role is not loaded.");
                }

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, agent.IdAgent.ToString()),
                new Claim(ClaimTypes.Role, agent.Role.Name),
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

                Logger.Info("JWT token successfully generated for user {AgentId}.", agent.IdAgent);
                return tokenHandler.WriteToken(token);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "Error generating JWT token: {Message}.", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error generating JWT token for user {AgentId}.", agent.IdAgent);
                throw new InvalidOperationException("Error generating JWT token.", ex);
            }
        }
    }
}
