using Activadis.Application.Validations.SignUp;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
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
        private readonly IWaitlistService WaitlistService;

        public SignUpService(IActivityRepository activityRepository, ISignUpRepository signUpRepository, IConfirmationService<SignOutRequest> signOutConfirmationService, IConfirmationService<SignUpRequest> signUpConfirmationService, IWaitlistService waitlistService)
        {
            ActivityRepository = activityRepository;
            SignUpRepository = signUpRepository;
            SignOutConfirmationService = signOutConfirmationService;
            SignUpConfirmationService = signUpConfirmationService;
            WaitlistService = waitlistService;
        }

        public async Task SignUpAsync(SignUpRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            int totalSignUps = await SignUpRepository.CountByActivityIdAsync(request.ActivityId);
            request.Validate(activity, userId, totalSignUps);

            if (userId != Guid.Empty)
            {
                SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdIncludingDeletedAsync(userId, request.ActivityId);
                await SignUpRepository.CreateOrUpdateAsync(signUp, request, userId);
            }
            else await SignUpConfirmationService.SendConfirmationAsync(request);
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

            SignUp signUp = await SignUpRepository.GetByUserIdAndActivityIdAsync(userId, request.ActivityId)
                    ?? throw new ArgumentException("Je bent niet ingeschreven bij deze activiteit.");

            await SignUpRepository.DeleteAsync(signUp);
            await WaitlistService.NotifyWaitlistAsync(request.ActivityId);
        }
    }
}
