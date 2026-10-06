using Microsoft.AspNetCore.Authentication.JwtBearer;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Activadis.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Activadis.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignUpController : ControllerBase
    {
        private readonly ISignUpStorageService SignUpStorageService;
        private readonly ISignUpService SignUpService;

        public SignUpController(ISignUpService signUpService, ISignUpStorageService signUpStorageService)
        {
            SignUpService = signUpService;
            SignUpStorageService = signUpStorageService;
        }

        [HttpPost]
        public async Task<IActionResult> SignUpAsync(SignUpRequest request)
        {
            string? id = Request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(id, out Guid userId))
                userId = Guid.Empty;

            string? fullName = Request.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if (fullName is not null)
                request.FullName = fullName;

            string? email = Request.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (email is not null)
                request.Email = email;

            try
            {
                await SignUpService.SignUpAsync(request, userId);
                return Ok(ApiResponse<object>.Ok());
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }

        [HttpPost("Confirmation")]
        public async Task<IActionResult> ConfirmSignUpAsync([FromBody] string token)
        {
            try
            {
                await SignUpStorageService.UseSignUpConfirmationAsync(token);
                return Ok(ApiResponse<object>.Ok());
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }

        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOutAsync(SignOutRequest request)
        {
            string? id = Request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(id, out Guid userId))
                userId = Guid.Empty;

            try
            {
                await SignUpService.SignOutAsync(request, userId);
                return Ok(ApiResponse<object>.Ok());
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }


        [HttpDelete("SignOut/Confirmation/{token}")]
        public async Task<IActionResult> ConfirmSignOutAsync(string token)
        {
            try
            {
                await SignUpStorageService.UseSignOutConfirmationAsync(token);
                return Ok(ApiResponse<object>.Ok());
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }
    }
}
