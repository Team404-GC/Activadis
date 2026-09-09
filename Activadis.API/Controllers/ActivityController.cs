using Activadis.Application.DTOs.Activity;
using Activadis.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<IActionResult> CreateAsync(CreateActivityRequest request)
        {
            try
            {
                await ActivityService.CreateAsync(request);
                return NoContent();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
