using Activadis.Domain.Entities;
using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Extensions
{
    public static class SignUpExtensions
    {
        public static SignUp ToSignUp(this SignUpRequest request, Guid userId)
        {
            return new SignUp()
            {
                FullName = request.FullName ?? string.Empty,
                Email = request.Email ?? string.Empty,

                HasPlusOne = request.HasPlusOne,
                IsExternal = userId == Guid.Empty,

                UserId = userId == Guid.Empty ? null : userId,
                ActivityId = request.ActivityId
            };
        }
    }
}
