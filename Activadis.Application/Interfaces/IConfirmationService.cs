namespace Activadis.Application.Interfaces
{
    public interface IConfirmationService<TRequest>
    {
        Task SendConfirmationAsync(TRequest request);
        Task UseConfirmationAsync(string token);
        TRequest? GetRequestByToken(string token);
    }
}
