using Microsoft.AspNetCore.Authentication.JwtBearer;
using Activadis.Application.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.Activity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Activadis.Shared.DTOs;
using Activadis.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

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

		[AcceptVerbs("QUERY")]
		[SwaggerIgnore]
		public async Task<IActionResult> GetActivitiesAsync(ActivityFilterRequest filter)
		{
			if (!Guid.TryParse(Request.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
				userId = Guid.Empty;
			if (!Enum.TryParse(Request.HttpContext.User.FindFirstValue(ClaimTypes.Role), out UserRole role))
				role = UserRole.User;

			IEnumerable<ActivityOverviewResponse> activities = await ActivityService.GetActivitiesAsync(userId, role, filter);
			return Ok(ApiResponse<IEnumerable<ActivityOverviewResponse>>.Ok(activities));
		}

		[HttpGet("SignedUp")]
		[Authorize]
		public async Task<IActionResult> GetSignedUpAsync()
		{
			string? nameIdentifier = Request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(nameIdentifier, out Guid userId))
				return Challenge(JwtBearerDefaults.AuthenticationScheme);

			IEnumerable<ActivityOverviewResponse> activities = await ActivityService.GetSignedUpAsync(userId);
			return Ok(ApiResponse<IEnumerable<ActivityOverviewResponse>>.Ok(activities));
		}

		[HttpGet("{id:guid}")]
		[Authorize]
		public async Task<IActionResult> GetDetailAsync(Guid id)
		{
			string? nameIdentifier = Request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(nameIdentifier, out Guid userId))
				return Challenge(JwtBearerDefaults.AuthenticationScheme);

			try
			{
				ActivityDetailResponse activity = await ActivityService.GetDetailAsync(id, userId);
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
