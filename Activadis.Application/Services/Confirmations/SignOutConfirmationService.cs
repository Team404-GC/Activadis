using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services.Confirmations
{
    public class SignOutConfirmationService : ConfirmationService<SignOutRequest>, IConfirmationService<SignOutRequest>
    {
        protected override double ExpiryMinutes => 15;
        protected override string MailError => "De uitschrijvingsmail is niet succesvol doorgestuurd. Probeer de uitschrijving later opnieuw.";
        protected override string TokenError => "Er is geen uitschrijving gevonden.";

        public SignOutConfirmationService(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory) { }

        protected override async Task<bool> SendEmailAsync(AsyncServiceScope scope, SignOutRequest request, string token)
        {
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdAsync(request.Email!, request.ActivityId)
                ?? throw new ArgumentException(MailError);

            if (signUp.WaitlistPosition is not null)
                return await emailService.SendRemoveWaitlistConfirmationAsync(signUp.Email!, signUp.FullName!, $"https://localhost:4200/confirm-signout/{token}");

            return await emailService.SendSignOutConfirmationAsync(signUp.Email!, signUp.FullName!, $"https://localhost:4200/confirm-signout/{token}"); ;
        }

        protected override async Task ProcessConfirmationAsync(AsyncServiceScope scope, SignOutRequest request)
        {
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();
            IActivityRepository activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();

            Activity activity = await activityRepository.GetByIdAsync(request.ActivityId)
                ?? throw new ArgumentException("De activiteit is niet gevonden.");

            if (DateTime.UtcNow >= activity.SignOutDeadline)
                throw new ArgumentException("De uitschrijfdatum is al geweest.");

            SignUp signUp = await signUpRepository.GetByEmailAndActivityIdAsync(request.Email!, request.ActivityId)
                ?? throw new ArgumentException("Je bent niet ingeschreven bij deze activiteit.");

            await signUpRepository.DeleteAsync(signUp);
            if (signUp.WaitlistPosition is null)
                return;

            await signUpRepository.AdvanceWaitlistAsync(signUp.WaitlistPosition ?? 1, request.ActivityId);
        }
    }
}
