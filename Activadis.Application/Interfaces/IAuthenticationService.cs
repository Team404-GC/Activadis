using Activadis.Application.DTOs;
using Activadis.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Activadis.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Token GenerateToken(User user);
    }
}
