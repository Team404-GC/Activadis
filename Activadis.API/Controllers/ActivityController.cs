using Activadis.Application.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.Activity;
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUpcomingAsync()
        {
            IEnumerable<ActivityOverviewResponse> activities = await ActivityService.GetUpcomingAsync();
            return Ok(ApiResponse<IEnumerable<ActivityOverviewResponse>>.Ok(activities));
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetDetailAsync(Guid id)
        {
            try
            {
                ActivityDetailResponse activity = await ActivityService.GetDetailAsync(id);
                return Ok(ApiResponse<ActivityDetailResponse>.Ok(activity));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<ActivityDetailResponse>.Fail(exception.Message));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateActivityRequest request)
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
