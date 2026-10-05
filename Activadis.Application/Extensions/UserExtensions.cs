using Activadis.Shared.DTOs.User;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;

namespace Activadis.Application.Extensions
{
    public static class UserExtensions
    {
        public static User ToUser(this CreateUserRequest request, string passwordSetupTokenHash, DateTime passwordSetupTokenExpiresAt)
        {
            return new User()
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                JobTitle = request.JobTitle.Trim(),
                HashedPassword = string.Empty,
                Role = Enum.Parse<UserRole>(request.Role),

                PasswordSetupTokenHash = passwordSetupTokenHash,
                PasswordSetupTokenExpiresAt = passwordSetupTokenExpiresAt
            };
        }

        public static bool HasPassword(this User user)
            => !string.IsNullOrEmpty(user.HashedPassword);

        public static bool PasswordSetupExpired(this User user)
            => user.PasswordSetupTokenExpiresAt is null || DateTime.UtcNow >= user.PasswordSetupTokenExpiresAt;
    }
}
