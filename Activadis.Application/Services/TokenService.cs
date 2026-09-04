using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Extensions.Configuration;
using Activadis.Application.Interfaces;
using Activadis.Application.DTOs.Auth;
using Microsoft.IdentityModel.Tokens;
using Activadis.Domain.Entities;
using System.Security.Claims;
using System.Text;

namespace Activadis.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration Configuration;

        public TokenService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Token GenerateToken(User user)
        {
            ClaimsIdentity claims = new ClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }
            );

            string? jwtKey = Configuration["JWT:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new ArgumentException("The JWT secret key is empty!");

            string? issuer = Configuration["JWT:Issuer"];
            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentException("The JWT issuer is empty!");

            string? expiryConfig = Configuration["JWT:ExpiryMinutes"];
            if (string.IsNullOrWhiteSpace(expiryConfig))
                throw new ArgumentException("The JWT expiry minutes is empty!");

            if (!double.TryParse(expiryConfig, out double expiryMinutes) || expiryMinutes <= 0)
                throw new ArgumentException("The JWT expiry minutes is invalid!");

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime expiresOn = DateTime.UtcNow.AddMinutes(expiryMinutes);
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Issuer = issuer,
                Subject = claims,
                Expires = expiresOn,
                SigningCredentials = credentials
            };

            return new Token()
            {
                JWT = new JsonWebTokenHandler().CreateToken(descriptor),
                ExpiresOn = expiresOn
            };
        }
    }
}
