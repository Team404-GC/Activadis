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

                Comment = request.Comment,
                HasPlusOne = request.HasPlusOne,
                IsExternal = userId == Guid.Empty,
                WaitlistPosition = position,

                UserId = userId == Guid.Empty ? null : userId,
                ActivityId = request.ActivityId
            };
        }

        public static SignUpDTO ToSignUpDTO(this SignUp request)
        {
            return new SignUpDTO()
            {
                Comment = request.Comment,
                HasPlusOne = request.HasPlusOne
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

            signUp.Comment = request.Comment;
            signUp.HasPlusOne = request.HasPlusOne;
            signUp.DeletedAt = null;
            signUp.WaitlistPosition = position;

            await signUpRepository.UpdateAsync(signUp);
        }

        public async static Task UpdateAsync(this ISignUpRepository signUpRepository, SignUp? signUp, EditSignUpRequest request, Guid userId)
        {
            if (signUp is null || signUp.WaitlistPosition is not null)
                throw new ArgumentException("Je bent niet ingeschreven bij deze activiteit.");

            signUp.Comment = request.Comment;
            signUp.HasPlusOne = request.HasPlusOne;

            await signUpRepository.UpdateAsync(signUp);
        }
    }
}
