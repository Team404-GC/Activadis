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
        private readonly IConfirmationService<SignOutRequest> SignOutConfirmationService;
        private readonly IConfirmationService<SignUpRequest> SignUpConfirmationService;
        private readonly ISignUpService SignUpService;

        public SignUpController(ISignUpService signUpService, IConfirmationService<SignOutRequest> signOutConfirmationService, IConfirmationService<SignUpRequest> signUpConfirmationService)
        {
            SignUpService = signUpService;
            SignOutConfirmationService = signOutConfirmationService;
            SignUpConfirmationService = signUpConfirmationService;
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
                await SignUpConfirmationService.UseConfirmationAsync(token);
                return Ok(ApiResponse<object>.Ok(message: "Je bent succesvol ingeschreven."));
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
                await SignOutConfirmationService.UseConfirmationAsync(token);
                return Ok(ApiResponse<object>.Ok(message: "Je bent succesvol uitgeschreven."));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }
    }
}
