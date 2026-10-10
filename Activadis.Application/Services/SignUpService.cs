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
        private readonly IConfirmationService<SignOutRequest> SignOutConfirmationService;
        private readonly IConfirmationService<SignUpRequest> SignUpConfirmationService;
        private readonly IActivityRepository ActivityRepository;
        private readonly ISignUpRepository SignUpRepository;

        public SignUpService(IActivityRepository activityRepository, ISignUpRepository signUpRepository, IConfirmationService<SignOutRequest> signOutConfirmationService, IConfirmationService<SignUpRequest> signUpConfirmationService)
        {
            ActivityRepository = activityRepository;
            SignUpRepository = signUpRepository;
            SignOutConfirmationService = signOutConfirmationService;
            SignUpConfirmationService = signUpConfirmationService;
        }

        public async Task SignUpAsync(SignUpRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            int totalSignUps = await SignUpRepository.CountByActivityIdAsync(request.ActivityId);
            request.Validate(activity, userId, totalSignUps);

            if (userId == Guid.Empty)
            {
                await SignUpConfirmationService.SendConfirmationAsync(request);
                return;
            }

            SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdIncludingDeletedAsync(userId, request.ActivityId);
            await SignUpRepository.CreateOrUpdateAsync(signUp, request, userId);
        }

        public async Task SignOutAsync(SignOutRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            SignOutValidation.Validate(activity);

            if (userId == Guid.Empty)
            {
                await SignOutConfirmationService.SendConfirmationAsync(request);
                return;
            }

            SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdAsync(userId, request.ActivityId)
                ?? throw new ArgumentException("Je bent niet ingeschreven bij deze activiteit.");

            await SignUpRepository.DeleteAsync(signUp);
        }
    }
}
