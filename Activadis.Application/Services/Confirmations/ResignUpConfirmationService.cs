using Microsoft.Extensions.DependencyInjection;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;

namespace Activadis.Application.Services.Confirmations
{
    public class ResignUpConfirmationService : ConfirmationService<ResignUpRequest>, IConfirmationService<ResignUpRequest>
    {
        protected override double ExpiryMinutes => 15;
        protected override string MailError => "De herinschrijvingsmail is niet succesvol doorgestuurd.";
        protected override string TokenError => "Er is geen herinschrijving gevonden.";

        public ResignUpConfirmationService(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory) { }

        protected override Task<bool> SendEmailAsync(AsyncServiceScope scope, ResignUpRequest request, string token)
        {
            throw new NotImplementedException();
        }

        protected override Task ProcessConfirmationAsync(AsyncServiceScope scope, ResignUpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
