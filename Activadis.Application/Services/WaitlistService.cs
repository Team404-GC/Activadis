using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

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

        public async Task AddToWaitlistAsync(SignUpRequest request)
        {
            int position = await SignUpRepository.GetLastPositionAsync(request.ActivityId) + 1;
            if (!await EmailService.SendAddWaitlistAsync(request.Email!, request.FullName!, position))
                throw new ArgumentException("Er is een onverwachte fout opgetreden. Schrijf je alsjeblieft opnieuw in bij de activiteit.");

            await SignUpRepository.AddAsync(new SignUp()
            {
                WaitlistPosition = position,
                FullName = request.FullName!,
                Email = request.Email!,
                ActivityId = request.ActivityId
            });
        }

        public async Task NotifyWaitlistAsync(Guid activityId)
        {
            SignUp? signUp = await SignUpRepository.GetFirstPositionAsync(activityId);
            if (signUp is null)
                return;

            await ResignUpConfirmationService.SendConfirmationAsync(new ResignUpRequest()
            {
                FullName = signUp.FullName,
                Email = signUp.Email,
                HasPlusOne = signUp.HasPlusOne,
                ActivityId = activityId
            });
        }

        public async Task RemoveFromWaitlistAsync(SignOutRequest request)
        {
            SignUp? signUp = await SignUpRepository.GetByEmailAndActivityIdFromWaitlistAsync(request.Email!, request.ActivityId)
                ?? throw new ArgumentException("Je staat niet onze wachtlijst voor deze activiteit.");

            //if (!await EmailService.SendAddWaitlistAsync(signUp.Email, signUp.FullName))
            //    throw new ArgumentException("De uitschrijvingsmail is niet succesvol doorgestuurd. Probeer de uitschrijving later opnieuw.");

            await SignUpRepository.DeleteAsync(signUp);
        }
    }
}
