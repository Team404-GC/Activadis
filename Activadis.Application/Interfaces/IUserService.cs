using Activadis.Shared.DTOs.User;

namespace Activadis.Application.Interfaces
{
    public interface IUserService
    {
        Task CreateAsync(CreateUserRequest request);
        Task<IEnumerable<UserResponse>> GetAllAsync();
        Task<UserResponse> GetByIdentifierAsync(string identifier);
        Task UpdateAsync(UpdateUserRequest request);
        Task UpdateStatusAsync(Guid id, Guid currentUserId, UpdateUserStatusRequest request);
    }
}
