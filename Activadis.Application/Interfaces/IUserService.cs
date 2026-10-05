using Activadis.Shared.DTOs.User;

namespace Activadis.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> CreateAsync(CreateUserRequest request);
    }
}
