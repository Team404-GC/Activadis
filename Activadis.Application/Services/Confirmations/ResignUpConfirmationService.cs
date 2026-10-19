using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services.Confirmations
{
    public class ResignUpConfirmationService : ConfirmationService<ResignUpRequest>, IConfirmationService<ResignUpRequest>
    {
        protected override double ExpiryMinutes => double.MaxValue;
        protected override string MailError => "De herinschrijvingsmail is niet succesvol doorgestuurd.";
        protected override string TokenError => "Er is geen herinschrijving gevonden.";

        public ResignUpConfirmationService(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory) { }

        protected override async Task<bool> SendEmailAsync(AsyncServiceScope scope, ResignUpRequest request, string token)
        {
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            return await emailService.SendResignUpConfirmationAsync(request.Email, request.FullName, $"https://localhost:4200/confirm-resignup/{token}");
        }

        protected override async Task ProcessConfirmationAsync(AsyncServiceScope scope, ResignUpRequest request)
        {
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            SignUp signUp = await signUpRepository.GetByEmailAndActivityIdAsync(request.Email, request.ActivityId)
                ?? throw new ArgumentException("De inschrijving is niet gevonden.");
            int position = signUp.WaitlistPosition ?? 1;

            signUp.HasPlusOne = request.HasPlusOne;
            signUp.WaitlistPosition = null;
            await signUpRepository.UpdateAsync(signUp);

            await signUpRepository.AdvanceWaitlistAsync(position, request.ActivityId);
        }
    }
}
