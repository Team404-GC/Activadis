using Activadis.Shared.DTOs.User;
using Activadis.Shared.DTOs;

namespace Activadis.UI.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<object>> CreateAsync(CreateUserRequest request);
        Task<ApiResponse<IEnumerable<UserResponse>>> GetAllAsync();
        Task<ApiResponse<UserResponse>> GetByIdAsync(Guid id);
        Task<ApiResponse<object>> UpdateAsync(UpdateUserRequest request);
    }
}
