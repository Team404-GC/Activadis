using Activadis.UI.Application.DTOs.Activity;

namespace Activadis.UI.Application.Interfaces
{
    public interface IActivityService
    {
        Task CreateAsync(CreateActivityRequest request);
    }
}
