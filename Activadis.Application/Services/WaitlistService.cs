using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class WaitlistService : IWaitlistService
    {
        private readonly IWaitlistRepository WaitlistRepository;
        private readonly IEmailService EmailService;

        public WaitlistService(IEmailService emailService, IWaitlistRepository waitlistRepository)
        {
            EmailService = emailService;
            WaitlistRepository = waitlistRepository;
        }

        public async Task AddToWaitlistAsync(SignUpRequest request)
        {
            int position = await WaitlistRepository.GetLastPositionAsync(request.ActivityId) + 1;
            if (!await EmailService.SendAddWaitlistAsync(request.Email!, request.FullName!, position))
                throw new ArgumentException("De inschrijvingsmail is niet succesvol doorgestuurd. Probeer de inschrijving later opnieuw.");

            await WaitlistRepository.AddAsync(new WaitlistEntry()
            {
                Position = position,
                Email = request.Email!,
                ActivityId = request.ActivityId
            });
        }

        public async Task NotifyWaitlistAsync(Guid activityId)
        {

        }

        public async Task RemoveFromWaitlistAsync(SignOutRequest request)
        {

        }
    }
}
