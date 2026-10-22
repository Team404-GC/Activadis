using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services.Confirmations
{
    public class EditSignUpConfirmationService : ConfirmationService<EditSignUpRequest>, IConfirmationService<EditSignUpRequest>
    {
        protected override double ExpiryMinutes => 15;
        protected override string MailError => "De wijzigingsmail is niet succesvol doorgestuurd. Probeer je wijziging later opnieuw.";
        protected override string TokenError => "Er is geen wijziging gevonden.";

        public EditSignUpConfirmationService(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory) { }

        protected override async Task<bool> SendEmailAsync(AsyncServiceScope scope, EditSignUpRequest request, string token)
        {
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            string fullName = await signUpRepository.GetFullNameByEmailAndActivityIdAsync(request.Email!, request.ActivityId)
                ?? throw new ArgumentException("De naam onder de inschrijving is niet gevonden.");

            return await emailService.SendEditSignUpConfirmationAsync(request.Email!, fullName, token);
        }

        protected override async Task ProcessConfirmationAsync(AsyncServiceScope scope, EditSignUpRequest request)
        {
            IActivityRepository activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            Activity activity = await activityRepository.GetByIdAsync(request.ActivityId)
                ?? throw new ArgumentException("De activiteit is niet gevonden.");

            if (DateTime.UtcNow >= activity.SignOutDeadline)
                throw new ArgumentException("De uitschrijfdatum is al geweest, dit zorgt ervoor dat je niet meer kan wijzigen.");

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdAsync(request.Email!, request.ActivityId);
            await signUpRepository.UpdateAsync(signUp, request, Guid.Empty);
        }
    }
}
