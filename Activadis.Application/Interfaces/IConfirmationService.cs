namespace Activadis.Application.Interfaces
{
    public interface IConfirmationService<TRequest>
    {
        Task SendConfirmationAsync(TRequest request);
        Task UseConfirmationAsync(string token);
        Task UseConfirmationAsync(string token, TRequest? request);
        TRequest? GetRequestByToken(string token);
    }
}
