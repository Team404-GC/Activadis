using Activadis.Shared.DTOs;
using Activadis.UI.Application.DTOs.Activity;

namespace Activadis.UI.Application.Interfaces
{
    public interface IActivityService
    {
        Task<ApiResponse<object>> CreateAsync(CreateActivityRequest request);
    }
}
