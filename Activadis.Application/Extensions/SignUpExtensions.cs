using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Extensions
{
    public static class SignUpExtensions
    {
        public static SignUp ToSignUp(this SignUpRequest request, Guid userId, int? position = null)
        {
            return new SignUp()
            {
                FullName = request.FullName ?? string.Empty,
                Email = request.Email ?? string.Empty,

                HasPlusOne = request.HasPlusOne,
                IsExternal = userId == Guid.Empty,
                WaitlistPosition = position,

                UserId = userId == Guid.Empty ? null : userId,
                ActivityId = request.ActivityId
            };
        }

        public async static Task CreateOrUpdateAsync(this ISignUpRepository signUpRepository, SignUp? signUp, SignUpRequest request, Guid userId, int? position = null)
        {
            if (signUp is null)
            {
                await signUpRepository.AddAsync(request.ToSignUp(userId, position));
                return;
            }

            if (signUp.DeletedAt is null)
                throw new ArgumentException("Je kan niet 2x inschrijven bij dezelfde activiteit.");

            signUp.HasPlusOne = request.HasPlusOne;
            signUp.DeletedAt = null;
            signUp.WaitlistPosition = position;
            await signUpRepository.UpdateAsync(signUp);
        }
    }
}
