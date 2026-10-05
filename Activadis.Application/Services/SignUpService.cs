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
        private readonly ISignUpStorageService SignUpStorageService;
        private readonly IActivityRepository ActivityRepository;
        private readonly ISignUpRepository SignUpRepository;

        public SignUpService(IActivityRepository activityRepository, ISignUpRepository signUpRepository, ISignUpStorageService signUpStorageService)
        {
            ActivityRepository = activityRepository;
            SignUpRepository = signUpRepository;
            SignUpStorageService = signUpStorageService;
        }

        public async Task SignUpAsync(SignUpRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            int totalSignUps = await SignUpRepository.CountByActivityIdAsync(request.ActivityId);
            request.Validate(activity, userId, totalSignUps);

            if (userId == Guid.Empty)
            {
                await SignUpStorageService.SendSignUpConfirmationAsync(request);
                return;
            }

            SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdIncludingDeletedAsync(userId, request.ActivityId);
            await SignUpRepository.CreateOrUpdateAsync(signUp, request, userId);
        }

        public async Task SignOutAsync(Guid activityId, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(activityId);
            SignOutValidation.Validate(activity);

            SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdAsync(userId, activityId)
                ?? throw new ArgumentException("Je bent niet ingeschreven bij deze activiteit.");

            await SignUpRepository.DeleteAsync(signUp);
        }
    }
}
