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
        private readonly ISignUpRepository SignUpRepository;
        private readonly IWaitlistService WaitlistService;
        private readonly ITokenService TokenService;
        private readonly IEmailService EmailService;
        private readonly IConfiguration Configuration;

        public UserService(IUserRepository userRepository, ISignUpRepository signUpRepository, IWaitlistService waitlistService, ITokenService tokenService, IEmailService emailService, IConfiguration configuration)
        {
            UserRepository = userRepository;
            SignUpRepository = signUpRepository;
            WaitlistService = waitlistService;
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

        public async Task<UserResponse> GetByIdentifierAsync(string identifier)
        {
            User user = await UserRepository.GetByIdentifierAsync(identifier)
                ?? throw new KeyNotFoundException("De gebruiker is niet gevonden.");

            return user.ToResponse();
        }

        public async Task UpdateAsync(UpdateUserRequest request)
        {
            User user = await UserRepository.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException("De gebruiker is niet gevonden.");

            User? existingUser = await UserRepository.GetByEmailAsync(request.Email.Trim());
            int activeAdminCount = await UserRepository.CountActiveByRoleAsync(UserRole.Admin);
            request.Validate(user, existingUser, activeAdminCount);

            user.FullName = request.FullName.Trim();
            user.Email = request.Email.Trim();
            user.JobTitle = request.JobTitle.Trim();
            user.Role = Enum.Parse<UserRole>(request.Role);

            await UserRepository.UpdateAsync(user);
        }

        public async Task UpdateStatusAsync(Guid id, Guid currentUserId, UpdateUserStatusRequest request)
        {
            User user = await UserRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("De gebruiker is niet gevonden.");

            int activeAdminCount = await UserRepository.CountActiveByRoleAsync(UserRole.Admin);
            request.Validate(user, currentUserId, activeAdminCount);

            user.IsActive = request.IsActive;
            await UserRepository.UpdateAsync(user);

            if (user.IsActive)
                return;

            // Upcoming registrations are removed, past registrations stay as history.
            IEnumerable<SignUp> signUps = await SignUpRepository.GetUpcomingByUserIdAsync(user.Id);
            foreach (SignUp signUp in signUps)
            {
                await SignUpRepository.DeleteAsync(signUp);
                await WaitlistService.NotifyWaitlistAsync(signUp.ActivityId);
            }
        }
    }
}
