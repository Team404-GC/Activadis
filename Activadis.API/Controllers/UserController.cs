using Microsoft.AspNetCore.Authorization;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Activadis.Shared.DTOs;

namespace Activadis.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService UserService;

        public UserController(IUserService userService)
        {
            UserService = userService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync(CreateUserRequest request)
        {
            try
            {
                bool created = await UserService.CreateAsync(request);
                if (!created)
                    return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("De uitnodigingsmail kon niet worden verstuurd. Het account is niet aangemaakt."));

                return Ok(ApiResponse<object>.Ok(message: "Het account is aangemaakt en de uitnodiging is verstuurd."));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }
    }
}
