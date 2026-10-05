using Activadis.Application.DTOs.SignUp;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.SignUp;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Activadis.Application.Services
{
    public class SignUpStorageService : ISignUpStorageService
    {
        private ConcurrentDictionary<string, SignUpConfirmationDTO> SignUps { get; set; } = new ConcurrentDictionary<string, SignUpConfirmationDTO>();

        private readonly IServiceScopeFactory ServiceScopeFactory;

        public SignUpStorageService(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        public async Task SendSignUpConfirmationAsync(SignUpRequest request)
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(16);
            SignUps.AddOrUpdate(string.Join(',', tokenBytes), _ => new SignUpConfirmationDTO(request, DateTime.UtcNow.AddMinutes(15)),
                (_, _) => new SignUpConfirmationDTO(request, DateTime.UtcNow.AddMinutes(15)));

            string token = Convert.ToHexString(tokenBytes).ToLower();

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            await emailService.SendSignUpConfirmationAsync(request.Email!, request.FullName!, $"https://localhost:4200/confirm-signup/{token}");
        }

        public async Task UseSignUpConfirmationAsync(string token)
        {
            byte[] tokenBytes = Convert.FromHexString(token.ToUpper());
            SignUpRequest? request = SignUps.Remove(string.Join(',', tokenBytes), out SignUpConfirmationDTO? confirmation) && !confirmation.HasExpired()
                ? confirmation.Request : null;

            if (request is null)
                return;

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            ISignUpRepository signUpRepository = scope.ServiceProvider.GetRequiredService<ISignUpRepository>();

            SignUp? signUp = await signUpRepository.GetByEmailAndActivityIdIncludingDeletedAsync(request.Email!, request.ActivityId);
            await signUpRepository.CreateOrUpdateAsync(signUp, request, Guid.Empty);
        }
    }
}
