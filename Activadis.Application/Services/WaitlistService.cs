using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;
using Activadis.Application.Extensions;

namespace Activadis.Application.Services
{
    public class WaitlistService : IWaitlistService
    {
        private readonly IConfirmationService<ResignUpRequest> ResignUpConfirmationService;
        private readonly ISignUpRepository SignUpRepository;
        private readonly IEmailService EmailService;

        public WaitlistService(IEmailService emailService, ISignUpRepository signUpRepository, IConfirmationService<ResignUpRequest> resignUpConfirmationService)
        {
            EmailService = emailService;
            SignUpRepository = signUpRepository;
            ResignUpConfirmationService = resignUpConfirmationService;
        }

        public async Task AddToWaitlistAsync(SignUpRequest request, SignUp? signUp)
        {
            int position = await SignUpRepository.GetLastPositionAsync(request.ActivityId) + 1;
            if (!await EmailService.SendAddWaitlistConfirmationAsync(request.Email!, request.FullName!, position))
                throw new ArgumentException("Er is een onverwachte fout opgetreden. Schrijf je alsjeblieft opnieuw in bij de activiteit.");

            await SignUpRepository.CreateOrUpdateAsync(signUp, request, Guid.Empty, position);
        }

        public async Task NotifyWaitlistAsync(Guid activityId)
        {
            SignUp? signUp = await SignUpRepository.GetFirstPositionAsync(activityId);
            if (signUp is null || signUp.WaitlistPosition is null)
                return;

            await ResignUpConfirmationService.SendConfirmationAsync(new ResignUpRequest()
            {
                FullName = signUp.FullName,
                Email = signUp.Email,
                HasPlusOne = signUp.HasPlusOne,
                ActivityId = activityId
            });
        }
    }
}
