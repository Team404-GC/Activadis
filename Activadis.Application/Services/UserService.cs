using Activadis.Application.DTOs;
using Activadis.Application.Interfaces;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces;
using Activadis.Domain.Interfaces.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;

namespace Activadis.Application.Services
{
    public class UserService(IUserRepository userRepository, IAuthenticationService authenticationService, IPassword password) : IUserService
    {
        public async Task<Token> LoginAsync(LoginRequest request)
        {
            User? user = await userRepository.GetUserByEmail(request.Email);

            if (user is null || !password.Validate(user.HashedPassword, request.Password))
                throw new AuthenticationException("The email or password is incorrect.");

            return authenticationService.GenerateToken(user);
        }
    }
}
