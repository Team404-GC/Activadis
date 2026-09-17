using Microsoft.AspNetCore.Authentication.JwtBearer;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.SignUp;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Activadis.Shared.DTOs;

namespace Activadis.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignUpController : ControllerBase
    {
        private readonly ISignUpService SignUpService;

        public SignUpController(ISignUpService signUpService)
        {
            SignUpService = signUpService;
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

        [HttpDelete("{activityId}")]
        public async Task<IActionResult> SignOutAsync(Guid activityId)
        {
            string? id = Request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(id, out Guid userId))
                return Challenge(JwtBearerDefaults.AuthenticationScheme);

            try
            {
                await SignUpService.SignOutAsync(activityId, userId);
                return Ok(ApiResponse<object>.Ok());
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }
    }
}
