using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services.Confirmations
{
    public class SignUpConfirmationService : ConfirmationService<SignUpRequest>, IConfirmationService<SignUpRequest>
    {
        protected override double ExpiryMinutes => 15;
        protected override string MailError => "De inschrijvingsmail is niet succesvol doorgestuurd. Probeer de inschrijving later opnieuw.";
        protected override string TokenError => "Er is geen inschrijving gevonden.";

        public SignUpConfirmationService(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory) { }

        protected override async Task<bool> SendEmailAsync(AsyncServiceScope scope, SignUpRequest request, string token)
        {
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            return await emailService.SendSignUpConfirmationAsync(request.Email!, request.FullName!, $"https://localhost:4200/confirm-signup/{token}"); ;
        }

        protected override async Task ProcessConfirmationAsync(AsyncServiceScope scope, SignUpRequest request)
        {
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();
            IActivityRepository activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
            IWaitlistService waitlistService = scope.ServiceProvider.GetRequiredService<IWaitlistService>();

            Activity activity = await activityRepository.GetByIdAsync(request.ActivityId)
                ?? throw new ArgumentException("De activiteit is niet gevonden.");

            if (DateTime.UtcNow >= activity.SignUpDeadline)
                throw new ArgumentException("De inschrijfdatum is al geweest.");

            int totalSignedUp = await signUpRepository.CountByActivityIdAsync(request.ActivityId);
            if (totalSignedUp >= activity.MaxParticipants)
            {
                await waitlistService.AddToWaitlistAsync(request);
                throw new ArgumentException("Deze activiteit heeft al het maximum aantal deelnemers. We hebben een nieuwe e-mail verstuurd ter informatie over wat er nu gaat gebeuren.");
            }

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdIncludingDeletedAsync(request.Email!, request.ActivityId);
            await signUpRepository.CreateOrUpdateAsync(signUp, request, Guid.Empty);
        }
    }
}
