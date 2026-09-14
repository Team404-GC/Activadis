using Activadis.Application.DTOs.Activity;

namespace Activadis.Application.Interfaces
{
    public interface IActivityService
    {
        Task CreateAsync(CreateActivityRequest request);
    }
}
