using Activadis.Application.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Activadis.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Activadis.Shared.DTOs;

namespace Activadis.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService ActivityService;

        public ActivityController(IActivityService activityService)
        {
            ActivityService = activityService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync(CreateActivityRequest request)
        {
            try
            {
                await ActivityService.CreateAsync(request);
                return Ok(ApiResponse<object>.Ok());
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(exception.Message));
            }
        }
    }
}
