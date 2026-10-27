using Activadis.Shared.DTOs.User;

namespace Activadis.Application.Interfaces
{
    public interface IUserService
    {
        Task CreateAsync(CreateUserRequest request);
        Task<IEnumerable<UserResponse>> GetAllAsync();
        Task<UserResponse> GetByIdAsync(Guid id);
        Task UpdateAsync(UpdateUserRequest request);
    }
}
