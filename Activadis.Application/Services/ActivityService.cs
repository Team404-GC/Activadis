using Activadis.Application.Validations.Activity;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.DTOs.Activity;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository ActivityRepository;

        public ActivityService(IActivityRepository activityRepository)
        {
            ActivityRepository = activityRepository;
        }

        public async Task CreateAsync(CreateActivityRequest request)
        {
            request.Validate();

            MemoryStream image = new MemoryStream();
            await request.Image.CopyToAsync(image);

            Activity activity = request.ToActivity(image.ToArray());
            activity = await ActivityRepository.AddAsync(activity);
        }
    }
}
