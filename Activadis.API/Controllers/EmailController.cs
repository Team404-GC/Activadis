using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Activadis.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Activadis.Shared.DTOs;

namespace Activadis.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService EmailService;

        public EmailController(IEmailService emailService)
        {
            EmailService = emailService;
        }

        [HttpPost("Test")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTestAsync()
        {
            string? email = Request.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Challenge(JwtBearerDefaults.AuthenticationScheme);

            string fullName = Request.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            bool sent = await EmailService.SendTestAsync(email, fullName);
            if (!sent)
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("De testmail kon niet worden verstuurd."));

            return Ok(ApiResponse<object>.Ok(message: "De testmail is verstuurd."));
        }
    }
}
