using Activadis.Application.DTOs;
using Activadis.Application.Interfaces;
using Activadis.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Activadis.Application.Services
{
    public class AuthenticationService(IConfiguration configuration) : IAuthenticationService
    {
        public Token GenerateToken(User user)
        {
            ClaimsIdentity claims = new ClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.FullName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Role, user.UserRole.ToString())
                }
            );

            string? jwtKey = configuration["JWT:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new ArgumentException("The JWT secret key is empty!");

            string? issuer = configuration["JWT:Issuer"];
            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentException("The JWT issuer is empty!");

            string? expiryConfig = configuration["JWT:ExpiryMinutes"];
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

            string token = new JsonWebTokenHandler().CreateToken(descriptor);
            return new Token() { JWT = token, ExpiresOn = expiresOn };
        }
    }
}
