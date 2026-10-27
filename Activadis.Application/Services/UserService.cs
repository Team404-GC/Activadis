using Activadis.Application.Validations.User;
using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.User;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;

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

        public async Task CreateAsync(CreateUserRequest request)
        {
            User? existingUser = await UserRepository.GetByEmailAsync(request.Email.Trim());
            request.Validate(existingUser);

            string token = TokenService.GeneratePasswordSetupToken();
            string tokenHash = TokenService.HashPasswordSetupToken(token);
            DateTime tokenExpiresAt = DateTime.UtcNow.AddMinutes(PasswordSetupValidMinutes);

            User user = request.ToUser(tokenHash, tokenExpiresAt);

            bool sent = await EmailService.SendPasswordSetupAsync(user.Email, user.FullName, token);
            if (!sent)
                throw new ArgumentException("De uitnodigingsmail kon niet worden verstuurd. Het account is niet aangemaakt.");

            await UserRepository.AddAsync(user);
        }

        public async Task<IEnumerable<UserResponse>> GetAllAsync()
        {
            IEnumerable<User> users = await UserRepository.GetAllAsync();
            return users
                .OrderBy(x => x.FullName)
                .Select(x => x.ToResponse());
        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            User user = await UserRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("De gebruiker is niet gevonden.");

            return user.ToResponse();
        }

        public async Task UpdateAsync(UpdateUserRequest request)
        {
            User user = await UserRepository.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException("De gebruiker is niet gevonden.");

            User? existingUser = await UserRepository.GetByEmailAsync(request.Email.Trim());
            int adminCount = await UserRepository.CountByRoleAsync(UserRole.Admin);
            request.Validate(user, existingUser, adminCount);

            user.FullName = request.FullName.Trim();
            user.Email = request.Email.Trim();
            user.JobTitle = request.JobTitle.Trim();
            user.Role = Enum.Parse<UserRole>(request.Role);

            await UserRepository.UpdateAsync(user);
        }
    }
}
