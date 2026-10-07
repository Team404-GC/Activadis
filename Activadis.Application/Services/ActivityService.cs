using Activadis.Application.DTOs.Activity;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Application.Validations.Activity;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;
using Activadis.Domain.Filters;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.Activity;

namespace Activadis.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository ActivityRepository;
        private readonly ISignUpRepository SignUpRepository;

        public ActivityService(IActivityRepository activityRepository, ISignUpRepository signUpRepository)
        {
            ActivityRepository = activityRepository;
            SignUpRepository = signUpRepository;
        }

        public async Task CreateAsync(CreateActivityRequest request)
        {
            request.Validate();

            MemoryStream image = new MemoryStream();
            await request.Image.CopyToAsync(image);

            Activity activity = request.ToActivity(image.ToArray());
            activity = await ActivityRepository.AddAsync(activity);
        }
		public async Task<IEnumerable<ActivityOverviewResponse>> GetActivitiesAsync(Guid userId, UserRole role, ActivityFilterRequest request)
		{
			bool isAdmin = role == UserRole.Admin;

			ActivityFilter filter = new ActivityFilter()
			{
				Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
				From = request.From,
				To = request.To,
				IncludePast = isAdmin && request.IncludePast
			};

			IEnumerable<Activity> activities = await ActivityRepository.GetListAsync(isAdmin, filter);

			Dictionary<Guid, bool> activitiesSignedIn = activities.ToDictionary(x => x.Id, x => false);
			foreach (Activity activity in activities)
			{
				bool hasSignedIn = await SignUpRepository.HasSignedUpAsync(userId, activity.Id);
				activitiesSignedIn[activity.Id] = hasSignedIn;
			}

			return activities.Select(activity => activity.ToOverviewResponse(activitiesSignedIn[activity.Id]));
		}
		public async Task<ActivityDetailResponse> GetDetailAsync(Guid id, Guid userId)
        {
            Activity activity = await ActivityRepository.GetDetailAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            bool hasSignedUp = await SignUpRepository.HasSignedUpAsync(userId, id);
            return activity.ToDetailResponse(hasSignedUp);
        }

        public async Task<IEnumerable<ActivityOverviewResponse>> GetSignedUpAsync(Guid userId)
        {
            IEnumerable<Activity> activities = await ActivityRepository.GetSignedUpByUserIdAsync(userId);

            Dictionary<Guid, bool> activitiesSignedIn = activities.ToDictionary(x => x.Id, x => false);
            foreach (Activity activity in activities)
            {
                bool hasSignedIn = await SignUpRepository.HasSignedUpAsync(userId, activity.Id);
                activitiesSignedIn[activity.Id] = hasSignedIn;
            }

            return activities.Select(activity => activity.ToOverviewResponse(activitiesSignedIn[activity.Id]));
        }
    }
}
