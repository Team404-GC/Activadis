using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
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

        public async static Task CreateOrUpdateAsync(this ISignUpRepository signUpRepository, SignUp? signUp, SignUpRequest request, Guid userId)
        {
            if (signUp is null)
            {
                await signUpRepository.AddAsync(request.ToSignUp(userId));
                return;
            }

            if (signUp.DeletedAt is null)
                throw new ArgumentException("Je kan niet 2x inschrijven bij dezelfde activiteit.");

            signUp.HasPlusOne = request.HasPlusOne;
            signUp.DeletedAt = null;
            await signUpRepository.UpdateAsync(signUp);
        }
    }
}
