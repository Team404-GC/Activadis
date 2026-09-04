using Activadis.Application.DTOs.Auth;
using Activadis.Domain.Entities;

namespace Activadis.Application.Interfaces
{
    public interface ITokenService
    {
        Token GenerateToken(User user);
    }
}
