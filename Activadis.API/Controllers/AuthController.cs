using Activadis.Application.Interfaces;
using System.Security.Authentication;
using Activadis.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Activadis.Shared.DTOs;

namespace Activadis.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService UserService;

        public AuthController(IAuthService userService)
        {
            UserService = userService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            try
            {
                Token token = await UserService.LoginAsync(request);
                return Ok(ApiResponse<Token>.Ok(token));
            }
            catch (AuthenticationException exception)
            {
                return Unauthorized(ApiResponse<Token>.Fail(exception.Message));
            }
        }
    }
}
