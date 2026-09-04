using Activadis.Domain.Interfaces.Repositories;
using Activadis.Domain.Interfaces.Helpers;
using Activadis.Application.Interfaces;
using Activadis.Application.DTOs.Auth;
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

            if (user is null || !Password.Validate(user.HashedPassword, request.Password))
                throw new AuthenticationException("De email of het password is incorrect.");

            return TokenService.GenerateToken(user);
        }
    }
}
