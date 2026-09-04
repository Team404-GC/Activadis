using Activadis.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Application.Interfaces
{
    public interface IUserService
    {
        Task<Token> LoginAsync(LoginRequest request);

    }
}
