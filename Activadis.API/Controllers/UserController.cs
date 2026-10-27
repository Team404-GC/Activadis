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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync()
        {
            IEnumerable<UserResponse> users = await UserService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<UserResponse>>.Ok(users));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdentifierAsync(string id)
        {
            try
            {
                UserResponse user = await UserService.GetByIdentifierAsync(id);
                return Ok(ApiResponse<UserResponse>.Ok(user));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<UserResponse>.Fail(exception.Message));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync(CreateUserRequest request)
        {
            try
            {
                await UserService.CreateAsync(request);
                return Ok(ApiResponse<object>.Ok(message: "Het account is aangemaakt en de uitnodiging is verstuurd."));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateUserRequest request)
        {
            try
            {
                await UserService.UpdateAsync(request);
                return Ok(ApiResponse<object>.Ok(message: "De gebruiker is met succes bijgewerkt."));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(exception.Message));
            }
        }
    }
}
