using Activadis.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using System.Collections.Concurrent;
using Activadis.Shared.DTOs.SignUp;
using System.Security.Cryptography;
using Activadis.Application.DTOs;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class SignUpStorageService : ISignUpStorageService
    {
        private ConcurrentDictionary<string, ConfirmationDTO<SignUpRequest>> SignUps { get; set; } = new ConcurrentDictionary<string, ConfirmationDTO<SignUpRequest>>();
        private ConcurrentDictionary<string, ConfirmationDTO<SignOutRequest>> SignOuts { get; set; } = new ConcurrentDictionary<string, ConfirmationDTO<SignOutRequest>>();

        private readonly IServiceScopeFactory ServiceScopeFactory;

        public SignUpStorageService(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        public async Task SendSignUpConfirmationAsync(SignUpRequest request)
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(16);
            string token = Convert.ToHexString(tokenBytes).ToLower();

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            bool succeeded = await emailService.SendSignUpConfirmationAsync(request.Email!, request.FullName!, $"https://localhost:4200/confirm-signup/{token}");
            if (!succeeded)
                throw new ArgumentException("De inschrijvingsmail is niet succesvol doorgestuurd. Probeer de inschrijving later opnieuw.");

            SignUps.AddOrUpdate(string.Join(',', tokenBytes), _ => new ConfirmationDTO<SignUpRequest>(request, DateTime.UtcNow.AddMinutes(15)),
                (_, _) => new ConfirmationDTO<SignUpRequest>(request, DateTime.UtcNow.AddMinutes(15)));
        }

        public async Task UseSignUpConfirmationAsync(string token)
        {
            byte[] tokenBytes = [];
            try
            {
                tokenBytes = Convert.FromHexString(token.ToUpper());
            }
            catch (FormatException)
            {
                throw new ArgumentException("Er is geen inschrijving gevonden.");
            }

            SignUpRequest? request = SignUps.Remove(string.Join(',', tokenBytes), out ConfirmationDTO<SignUpRequest>? confirmation) && !confirmation.HasExpired()
                ? confirmation.Request : null;

            if (request is null)
                throw new ArgumentException("Er is geen inschrijving gevonden.");

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdIncludingDeletedAsync(request.Email!, request.ActivityId);
            await signUpRepository.CreateOrUpdateAsync(signUp, request, Guid.Empty);
        }

        public async Task SendSignOutConfirmationAsync(SignOutRequest request)
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(16);
            string token = Convert.ToHexString(tokenBytes).ToLower();

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            string? fullName = await signUpRepository.GetFullNameByEmailAndActivityIdAsync(request.Email!, request.ActivityId) ?? "?";
            bool succeeded = await emailService.SendSignOutConfirmationAsync(request.Email!, fullName, $"https://localhost:4200/confirm-signout/{token}");
            if (!succeeded)
                throw new ArgumentException("De uitschrijvingsmail is niet succesvol doorgestuurd. Probeer de uitschrijving later opnieuw.");

            SignOuts.AddOrUpdate(string.Join(',', tokenBytes), _ => new ConfirmationDTO<SignOutRequest>(request, DateTime.UtcNow.AddMinutes(15)),
                (_, _) => new ConfirmationDTO<SignOutRequest>(request, DateTime.UtcNow.AddMinutes(15)));
        }

        public async Task UseSignOutConfirmationAsync(string token)
        {
            byte[] tokenBytes = [];
            try
            {
                tokenBytes = Convert.FromHexString(token.ToUpper());
            }
            catch (FormatException)
            {
                throw new ArgumentException("Er is geen uitschrijving gevonden.");
            }

            SignOutRequest? request = SignOuts.Remove(string.Join(',', tokenBytes), out ConfirmationDTO<SignOutRequest>? confirmation) && !confirmation.HasExpired()
                ? confirmation.Request : null;

            if (request is null)
                throw new ArgumentException("Er is geen uitschrijving gevonden.");

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdAsync(request.Email!, request.ActivityId)
                ?? throw new ArgumentException("Je bent niet ingeschreven bij deze activiteit.");

            await signUpRepository.DeleteAsync(signUp);
        }
    }
}
