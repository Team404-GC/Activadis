using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Extensions;
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

            string? fullName = await signUpRepository.GetFullNameByEmailAndActivityIdAsync(request.Email!, request.ActivityId) ?? "?";
            return await emailService.SendSignOutConfirmationAsync(request.Email!, fullName, $"https://localhost:4200/confirm-signout/{token}"); ;
        }

        protected override async Task ProcessConfirmationAsync(AsyncServiceScope scope, SignOutRequest request)
        {
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdAsync(request.Email!, request.ActivityId)
                ?? throw new ArgumentException("Je bent niet ingeschreven bij deze activiteit.");

            await signUpRepository.DeleteAsync(signUp);
        }
    }
}
