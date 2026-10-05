using Activadis.Application.Validations.User;
using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.User;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class UserService : IUserService
    {
        private const int PasswordSetupValidMinutes = 15;

        private readonly IUserRepository UserRepository;
        private readonly ITokenService TokenService;
        private readonly IEmailService EmailService;
        private readonly IConfiguration Configuration;

        public UserService(IUserRepository userRepository, ITokenService tokenService, IEmailService emailService, IConfiguration configuration)
        {
            UserRepository = userRepository;
            TokenService = tokenService;
            EmailService = emailService;
            Configuration = configuration;
        }

        public async Task<bool> CreateAsync(CreateUserRequest request)
        {
            User? existingUser = await UserRepository.GetByEmailAsync(request.Email.Trim());
            request.Validate(existingUser);

            string token = TokenService.GeneratePasswordSetupToken();
            string tokenHash = TokenService.HashPasswordSetupToken(token);
            DateTime tokenExpiresAt = DateTime.UtcNow.AddMinutes(PasswordSetupValidMinutes);

            User user = request.ToUser(tokenHash, tokenExpiresAt);

            bool sent = await EmailService.SendPasswordSetupAsync(user.Email, user.FullName, ToPasswordSetupLink(token));
            if (!sent)
                return false;

            await UserRepository.AddAsync(user);
            return true;
        }

        private string ToPasswordSetupLink(string token)
        {
            string? baseUrl = Configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("The frontend base url is empty!");

            return $"{baseUrl.TrimEnd('/')}/wachtwoord-instellen?token={token}";
        }
    }
}
