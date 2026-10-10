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

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdIncludingDeletedAsync(request.Email!, request.ActivityId);
            await signUpRepository.CreateOrUpdateAsync(signUp, request, Guid.Empty);
        }
    }
}
