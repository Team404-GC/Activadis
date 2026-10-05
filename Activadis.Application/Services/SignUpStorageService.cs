using Microsoft.Extensions.DependencyInjection;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.Interfaces;
using System.Collections.Concurrent;
using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Services
{
    public class SignUpStorageService : ISignUpStorageService
    {
        private ConcurrentDictionary<string, (SignUpRequest request, DateTime expires)> SignUps { get; set; } = new ConcurrentDictionary<string, (SignUpRequest request, DateTime expires)>();

        private readonly IServiceScopeFactory ServiceScopeFactory;

        public SignUpStorageService(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        public async Task SendSignUpConfirmationAsync(SignUpRequest request)
        {
            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            await emailService.SendSignUpConfirmationAsync(request.Email!, request.FullName!, "");
        }
    }
}
