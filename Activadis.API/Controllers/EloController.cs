using Activadis.Application.DTOs;
using Activadis.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;
using Activadis.Shared.DTOs.Elo;

namespace Activadis.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class EloController : ControllerBase
    {
        private readonly IEloService _eloService;

        public EloController(IEloService eloService)
        {
            _eloService = eloService;
        }

        [HttpGet("leaderboard/overall")]
        public async Task<IActionResult> GetOverallLeaderboard()
        {
            try
            {
                var leaderboard = await _eloService.GetOverallLeaderboardAsync();
                return Ok(ApiResponse<List<LeaderboardEntryDto>>.Ok(leaderboard));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<LeaderboardEntryDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("leaderboard/category/{categoryId}")]
        public async Task<IActionResult> GetCategoryLeaderboard(Guid categoryId)
        {
            try
            {
                var leaderboard = await _eloService.GetCategoryLeaderboardAsync(categoryId);
                return Ok(ApiResponse<List<LeaderboardEntryDto>>.Ok(leaderboard));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<LeaderboardEntryDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("player/{userId}")]
        public async Task<IActionResult> GetPlayerProfile(Guid userId)
        {
            try
            {
                var profile = await _eloService.GetPlayerProfileAsync(userId);
                if (profile == null)
                    return NotFound(ApiResponse<PlayerProfileDto>.Fail("Player not found"));

                return Ok(ApiResponse<PlayerProfileDto>.Ok(profile));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PlayerProfileDto>.Fail(ex.Message));
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _eloService.GetCategoriesAsync();
                return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CategoryDto>>.Fail(ex.Message));
            }
        }

        [HttpPost("match/record")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecordMatch([FromBody] RecordMatchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<MatchResultDto>.Fail("Invalid match data"));

                var result = await _eloService.RecordMatchAsync(request);
                return Ok(ApiResponse<MatchResultDto>.Ok(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MatchResultDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MatchResultDto>.Fail(ex.Message));
            }
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<CategoryDto>.Fail("Invalid category data"));

                var category = await _eloService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategories), category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Fail(ex.Message));
            }
        }

        [HttpPut("categories/{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                if (request.Id != categoryId)
                    return BadRequest(ApiResponse<CategoryDto>.Fail("Category ID mismatch"));

                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<CategoryDto>.Fail("Invalid category data"));

                var category = await _eloService.UpdateCategoryAsync(request);
                if (category == null)
                    return NotFound(ApiResponse<CategoryDto>.Fail("Category not found"));

                return Ok(ApiResponse<CategoryDto>.Ok(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Fail(ex.Message));
            }
        }
    }
}
