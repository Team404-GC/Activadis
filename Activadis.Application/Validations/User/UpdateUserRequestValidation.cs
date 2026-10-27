using Entities = Activadis.Domain.Entities;
using Activadis.Shared.DTOs.User;
using Activadis.Domain.Enums;

namespace Activadis.Application.Validations.User
{
    public static class UpdateUserRequestValidation
    {
        public static void Validate(this UpdateUserRequest request, Entities.User user, Entities.User? existingUser, int activeAdminCount)
        {
            // The user keeping its own email address is not a duplicate.
            request.Validate(existingUser?.Id == user.Id ? null : existingUser);

            if (user.Role == UserRole.Admin && user.IsActive && Enum.Parse<UserRole>(request.Role) != UserRole.Admin && activeAdminCount <= 1)
                throw new ArgumentException("De rol beheerder kan niet worden weggehaald bij de laatste actieve beheerder.");
        }
    }
}
