using Activadis.Application.Validations.Activity;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.DTOs.Activity;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.Activity;
using Activadis.Domain.Entities;

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

        public async Task<IEnumerable<ActivityOverviewResponse>> GetUpcomingAsync()
        {
            IEnumerable<Activity> activities = await ActivityRepository.GetUpcomingAsync();
            return activities.Select(activity => activity.ToOverviewResponse());
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
            return activities.Select(activity => activity.ToOverviewResponse());
        }
    }
}
