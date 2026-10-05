using Activadis.Domain.Interfaces.Repositories;
using Activadis.Domain.Interfaces.Helpers;
using Activadis.Application.Validations.Auth;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.Auth;
using System.Security.Authentication;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository UserRepository;
        private readonly ITokenService TokenService;
        private readonly IPassword Password;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IPassword password)
        {
            UserRepository = userRepository;
            TokenService = tokenService;
            Password = password;
        }

        public async Task<Token> LoginAsync(LoginRequest request)
        {
            User? user = await UserRepository.GetByEmailAsync(request.Email);

            if (user is null || !user.HasPassword() || !Password.Validate(user.HashedPassword, request.Password))
                throw new AuthenticationException("De email of het password is incorrect.");

            return TokenService.GenerateToken(user);
        }

        public async Task CheckPasswordSetupAsync(string token)
            => await GetByPasswordSetupTokenAsync(token);

        public async Task SetPasswordAsync(SetPasswordRequest request)
        {
            User user = await GetByPasswordSetupTokenAsync(request.Token);
            request.Validate();

            user.HashedPassword = Password.Hash(request.Password);
            user.PasswordSetupTokenHash = null;
            user.PasswordSetupTokenExpiresAt = null;

            await UserRepository.UpdateAsync(user);
        }

        private async Task<User> GetByPasswordSetupTokenAsync(string token)
        {
            string tokenHash = TokenService.HashPasswordSetupToken(token);

            User user = await UserRepository.GetByPasswordSetupTokenHashAsync(tokenHash)
                ?? throw new ArgumentException("De link is ongeldig of al gebruikt.");

            if (user.PasswordSetupExpired())
                throw new ArgumentException("De link is verlopen. Neem contact op met een beheerder.");

            return user;
        }
    }
}
