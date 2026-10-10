using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Activadis.Application.DTOs;

namespace Activadis.Application.Services.Confirmations
{
    public abstract class ConfirmationService<TRequest> : IConfirmationService<TRequest> where TRequest : class
    {
        private ConcurrentDictionary<string, ConfirmationDTO<TRequest>> Confirmations { get; } = new ConcurrentDictionary<string, ConfirmationDTO<TRequest>>();

        protected readonly IServiceScopeFactory ServiceScopeFactory;

        protected abstract double ExpiryMinutes { get; }
        protected abstract string MailError { get; }
        protected abstract string TokenError { get; }

        protected ConfirmationService(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        public async Task SendConfirmationAsync(TRequest request)
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(16);
            string token = Convert.ToHexString(bytes).ToLower();

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            if (!await SendEmailAsync(scope, request, token))
                throw new ArgumentException(MailError);

            Confirmations.AddOrUpdate(string.Join(',', bytes),
                _ => new ConfirmationDTO<TRequest>(request, DateTime.UtcNow.AddMinutes(ExpiryMinutes)),
                (_, _) => new ConfirmationDTO<TRequest>(request, DateTime.UtcNow.AddMinutes(ExpiryMinutes)));
        }

        public async Task UseConfirmationAsync(string token)
        {
            if (!ConvertExtensions.TryFromHexString(token, out byte[] bytes))
                throw new ArgumentException(TokenError);

            TRequest? request = Confirmations.Remove(string.Join(',', bytes), out var confirmation) && !confirmation.HasExpired()
                ? confirmation.Request : null;

            if (request is null)
                throw new ArgumentException(TokenError);

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            await ProcessConfirmationAsync(scope, request);
        }

        protected abstract Task<bool> SendEmailAsync(AsyncServiceScope scope, TRequest request, string token);
        protected abstract Task ProcessConfirmationAsync(AsyncServiceScope scope, TRequest request);
    }
}
