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
        private readonly IConfirmationService<EditSignUpRequest> EditSignOutConfirmationService;
        private readonly IConfirmationService<SignOutRequest> SignOutConfirmationService;
        private readonly IConfirmationService<SignUpRequest> SignUpConfirmationService;
        private readonly IActivityRepository ActivityRepository;
        private readonly ISignUpRepository SignUpRepository;
        private readonly IUserRepository UserRepository;
        private readonly IWaitlistService WaitlistService;

        public SignUpService(IActivityRepository activityRepository, ISignUpRepository signUpRepository, IUserRepository userRepository, IConfirmationService<SignOutRequest> signOutConfirmationService, IConfirmationService<SignUpRequest> signUpConfirmationService, IWaitlistService waitlistService, IConfirmationService<EditSignUpRequest> editSignOutConfirmationService)
        {
            ActivityRepository = activityRepository;
            SignUpRepository = signUpRepository;
            UserRepository = userRepository;
            SignOutConfirmationService = signOutConfirmationService;
            SignUpConfirmationService = signUpConfirmationService;
            WaitlistService = waitlistService;
            EditSignOutConfirmationService = editSignOutConfirmationService;
        }

        public async Task<SignUpDTO> GetSignUpAsync(GetSignUpRequest request, Guid userId)
        {
            SignUp? signUp = userId == Guid.Empty
                ? await SignUpRepository.GetByEmailAndActivityIdAsync(request.Email!, request.ActivityId)
                : await SignUpRepository.GetByUserIdAndActivityIdAsync(userId, request.ActivityId);

            if (signUp is null)
                throw new KeyNotFoundException("Er is geen inschrijving gevonden.");

            return signUp.ToSignUpDTO();
        }

        public async Task SignUpAsync(SignUpRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            int totalSignUps = await SignUpRepository.CountByActivityIdAsync(request.ActivityId);
            User? user = string.IsNullOrWhiteSpace(request.Email) ? null : await UserRepository.GetByEmailAsync(request.Email.Trim());
            request.Validate(activity, userId, totalSignUps, user);

            if (userId == Guid.Empty)
            {
                await SignUpConfirmationService.SendConfirmationAsync(request);
                return;
            }

            SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdIncludingDeletedAsync(userId, request.ActivityId);
            await SignUpRepository.CreateOrUpdateAsync(signUp, request, userId);
        }

        public async Task EditSignUpAsync(EditSignUpRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            request.Validate(activity, userId);

            if (userId == Guid.Empty)
            {
                await EditSignOutConfirmationService.SendConfirmationAsync(request);
                return;
            }

            SignUp? signUp = await SignUpRepository.GetByUserIdAndActivityIdAsync(userId, request.ActivityId);
            await SignUpRepository.UpdateAsync(signUp, request, userId);
        }

        public async Task SignOutAsync(SignOutRequest request, Guid userId)
        {
            Activity? activity = await ActivityRepository.GetByIdAsync(request.ActivityId);
            request.Validate(activity, userId);

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
