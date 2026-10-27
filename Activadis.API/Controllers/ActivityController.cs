using Microsoft.AspNetCore.Authentication.JwtBearer;
using Activadis.Application.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.Elo.Stats;
using Activadis.Shared.DTOs.Activity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Activadis.Domain.Enums;
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

		[HttpGet("{id}")]
		public async Task<IActionResult> GetDetailAsync(string id)
		{
			string? nameIdentifier = Request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(nameIdentifier, out Guid userId))
				userId = Guid.Empty;

			bool isAdmin = Enum.TryParse(Request.HttpContext.User.FindFirstValue(ClaimTypes.Role), out UserRole role)
				&& role == UserRole.Admin;

			try
			{
				ActivityDetailResponse activity = await ActivityService.GetDetailAsync(id, userId, isAdmin);
				return Ok(ApiResponse<ActivityDetailResponse>.Ok(activity));
			}
			catch (ArgumentException exception)
			{
				return BadRequest(ApiResponse<ActivityDetailResponse>.Fail(exception.Message));
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<ActivityDetailResponse>.Fail(exception.Message));
			}
		}

		[HttpGet("{id:guid}/Photo")]
		[ResponseCache(Duration = 3600)]
		public async Task<IActionResult> GetPhotoAsync(Guid id)
		{
			try
			{
				ActivityPhotoResponse photo = await ActivityService.GetPhotoAsync(id);
				return File(photo.Image, photo.ImageContentType);
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<object>.Fail(exception.Message));
			}
		}

		[HttpGet("{id:guid}/Photos/{photoId:guid}")]
		[ResponseCache(Duration = 3600)]
		public async Task<IActionResult> GetPhotoByIdAsync(Guid id, Guid photoId)
		{
			try
			{
				ActivityPhotoResponse photo = await ActivityService.GetPhotoByIdAsync(id, photoId);
				return File(photo.Image, photo.ImageContentType);
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<object>.Fail(exception.Message));
			}
		}

		[HttpPost("{id:guid}/Photos")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddPhotosAsync(Guid id, [FromForm] AddActivityPhotosRequest request)
		{
			try
			{
				await ActivityService.AddPhotosAsync(id, request);
				return Ok(ApiResponse<object>.Ok());
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

		[HttpPut("{id:guid}/Photos/Order")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdatePhotoOrderAsync(Guid id, [FromBody] UpdateActivityPhotoOrderRequest request)
		{
			try
			{
				await ActivityService.UpdatePhotoOrderAsync(id, request);
				return Ok(ApiResponse<object>.Ok());
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

		[HttpDelete("{id:guid}/Photos/{photoId:guid}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeletePhotoAsync(Guid id, Guid photoId)
		{
			try
			{
				await ActivityService.DeletePhotoAsync(id, photoId);
				return Ok(ApiResponse<object>.Ok());
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<object>.Fail(exception.Message));
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

		[HttpPut]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> EditAsync([FromBody] EditActivityRequest request)
		{
			try
			{
				await ActivityService.EditAsync(request);
				return Ok(ApiResponse<object>.Ok());
			}
			catch (ArgumentException exception)
			{
				return BadRequest(ApiResponse<object>.Fail(exception.Message));
			}
		}

		[HttpPut("{id:guid}/categories")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateCategoriesAsync(Guid id, [FromBody] UpdateActivityCategoriesRequest request)
		{
			try
			{
				await ActivityService.UpdateCategoriesAsync(id, request);
				return Ok(ApiResponse<object>.Ok());
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

		[HttpGet("{id:guid}/participants")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetParticipantsAsync(Guid id)
		{
			try
			{
				IEnumerable<ActivityParticipantResponse> participants = await ActivityService.GetParticipantsAsync(id);
				return Ok(ApiResponse<IEnumerable<ActivityParticipantResponse>>.Ok(participants));
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<IEnumerable<ActivityParticipantResponse>>.Fail(exception.Message));
			}
		}

		[HttpGet("{id:guid}/signups")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetSignUpsAsync(Guid id)
		{
			try
			{
				ActivitySignUpListResponse signUps = await ActivityService.GetSignUpsAsync(id);
				return Ok(ApiResponse<ActivitySignUpListResponse>.Ok(signUps));
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<ActivitySignUpListResponse>.Fail(exception.Message));
			}
		}

		[HttpPost("{id:guid}/participants/inform")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> InformParticipantsAsync(Guid id)
		{
			try
			{
				InformParticipantsResponse response = await ActivityService.InformParticipantsAsync(id);
				return Ok(ApiResponse<InformParticipantsResponse>.Ok(response));
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<InformParticipantsResponse>.Fail(exception.Message));
			}
		}

		[HttpDelete("{id:guid}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CancelAsync(Guid id)
		{
			try
			{
				InformParticipantsResponse response = await ActivityService.CancelAsync(id);
				return Ok(ApiResponse<InformParticipantsResponse>.Ok(response));
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<InformParticipantsResponse>.Fail(exception.Message));
			}
		}

		[HttpGet("{id:guid}/matches/linkable")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetLinkableMatchesAsync(Guid id)
		{
			try
			{
				IEnumerable<RecentMatchDto> matches = await ActivityService.GetLinkableMatchesAsync(id);
				return Ok(ApiResponse<IEnumerable<RecentMatchDto>>.Ok(matches));
			}
			catch (KeyNotFoundException exception)
			{
				return NotFound(ApiResponse<IEnumerable<RecentMatchDto>>.Fail(exception.Message));
			}
		}

		[HttpPost("{id:guid}/matches")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> LinkMatchAsync(Guid id, [FromBody] LinkMatchRequest request)
		{
			try
			{
				await ActivityService.LinkMatchAsync(id, request);
				return Ok(ApiResponse<object>.Ok());
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

		[HttpDelete("{id:guid}/matches/{matchId:guid}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UnlinkMatchAsync(Guid id, Guid matchId)
		{
			try
			{
				await ActivityService.UnlinkMatchAsync(id, matchId);
				return Ok(ApiResponse<object>.Ok());
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
