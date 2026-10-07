using Activadis.Shared.DTOs;
using Activadis.Shared.DTOs.Activity;
using Activadis.UI.Application.DTOs.Activity;
using Activadis.UI.Application.Interfaces;

namespace Activadis.UI.Application.Services
{
	public class ActivityService : IActivityService
	{
		private readonly IHttpService HttpService;

		public ActivityService(IHttpService httpService)
		{
			HttpService = httpService;
		}

		public async Task<ApiResponse<object>> CreateAsync(CreateActivityRequest request)
			=> await HttpService.PostIncludeFileAsync<object, CreateActivityRequest>("/Activity", request, x => x.Image);
		public async Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetActivitiesAsync(ActivityFilterRequest filter)
			=> await HttpService.GetAsync<IEnumerable<ActivityOverviewResponse>>(ToUrl(filter));
		public async Task<ApiResponse<ActivityDetailResponse>> GetDetailAsync(Guid id)
			=> await HttpService.GetAsync<ActivityDetailResponse>($"/Activity/{id}");

		public async Task<ApiResponse<IEnumerable<ActivityOverviewResponse>>> GetSignedUpAsync()
			=> await HttpService.GetAsync<IEnumerable<ActivityOverviewResponse>>("/Activity/SignedUp");

		private static string ToUrl(ActivityFilterRequest filter)
		{
			List<string> parameters = [];

			Add("name", filter.Name);
			Add("from", filter.From?.ToString("O"));
			Add("to", filter.To?.ToString("O"));
			Add("includePast", filter.IncludePast ? "true" : null);

			return parameters.Count == 0
				? "/Activity"
				: $"/Activity?{string.Join('&', parameters)}";

			void Add(string key, string? value)
			{
				if (!string.IsNullOrWhiteSpace(value))
					parameters.Add($"{key}={Uri.EscapeDataString(value)}");
			}
		}
	}
}
