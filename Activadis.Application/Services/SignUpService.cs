using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.Validations.SignUp;
using Activadis.Application.Interfaces;
using Activadis.Application.Extensions;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class SignUpService : ISignUpService
    {
        private readonly IActivityRepository ActivityRepository;
        private readonly ISignUpRepository SignUpRepository;

        public SignUpService(IActivityRepository activityRepository, ISignUpRepository signUpRepository)
        {
            ActivityRepository = activityRepository;
            SignUpRepository = signUpRepository;
        }

        public async Task SignUpAsync(SignUpRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            request.Validate(activity, userId);

            SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdIncludingDeletedAsync(userId, request.ActivityId);
            if (signUp is null)
            {
                await SignUpRepository.AddAsync(
                    request.ToSignUp(userId)
                );
                return;
            }

            if (signUp.DeletedAt is null)
                throw new ArgumentException("Je kan niet 2x inschrijven bij dezelfde activiteit.");

            signUp.HasPlusOne = request.HasPlusOne;
            signUp.DeletedAt = null;
            await SignUpRepository.UpdateAsync(signUp);
        }
    }
}
