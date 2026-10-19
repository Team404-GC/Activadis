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

            if (ExpiryMinutes <= 0)
                throw new InvalidOperationException("De ongeldigeheidsminuten zitten onder de nul.");

            DateTime expires = DateTime.MinValue;
            try
            {
                expires = DateTime.UtcNow.AddMinutes(ExpiryMinutes);
            }
            catch (ArgumentOutOfRangeException)
            {
                expires = DateTime.MaxValue;
            }

            Confirmations.AddOrUpdate(token,
                _ => new ConfirmationDTO<TRequest>(request, expires),
                (_, _) => new ConfirmationDTO<TRequest>(request, expires));

            try
            {
                using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();

                if (!await SendEmailAsync(scope, request, token))
                    throw new ArgumentException(MailError);
            }
            catch (Exception)
            {
                Confirmations.TryRemove(token, out _);
                return;
            }
        }

        public async Task UseConfirmationAsync(string token)
            => await UseConfirmationAsync(token, null);

        public async Task UseConfirmationAsync(string token, TRequest? request)
        {
            request ??= TakeRequestByToken(token)
                ?? throw new ArgumentException(TokenError);

            using AsyncServiceScope scope = ServiceScopeFactory.CreateAsyncScope();
            await ProcessConfirmationAsync(scope, request);
        }

        public TRequest? GetRequestByToken(string token)
        {
            ConfirmationDTO<TRequest>? confirmation = Confirmations.GetValueOrDefault(token);
            if (confirmation is null || confirmation.HasExpired())
                return null;

            return confirmation.Request;
        }

        private TRequest? TakeRequestByToken(string token)
        {
            return Confirmations.Remove(token, out ConfirmationDTO<TRequest>? confirmation) && !confirmation.HasExpired()
                ? confirmation.Request : null;
        }

        protected abstract Task<bool> SendEmailAsync(AsyncServiceScope scope, TRequest request, string token);
        protected abstract Task ProcessConfirmationAsync(AsyncServiceScope scope, TRequest request);
    }
}
