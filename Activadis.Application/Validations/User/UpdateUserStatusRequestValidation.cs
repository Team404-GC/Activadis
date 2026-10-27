using Entities = Activadis.Domain.Entities;
using Activadis.Shared.DTOs.User;
using Activadis.Domain.Enums;

namespace Activadis.Application.Validations.User
{
    public static class UpdateUserStatusRequestValidation
    {
        public static void Validate(this UpdateUserStatusRequest request, Entities.User user, Guid currentUserId, int activeAdminCount)
        {
            if (request.IsActive)
                return;

            if (user.Id == currentUserId)
                throw new ArgumentException("Je kan je eigen account niet deactiveren.");

            if (user.Role == UserRole.Admin && user.IsActive && activeAdminCount <= 1)
                throw new ArgumentException("De laatste actieve beheerder kan niet worden gedeactiveerd.");
        }
    }
}
