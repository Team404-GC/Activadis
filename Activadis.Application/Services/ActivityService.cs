using Activadis.Application.DTOs.Activity;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;

namespace Activadis.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly List<string> ValidImageTypes = new List<string>() { "png", "jpg", "jpeg", "webp" };

        private readonly IActivityRepository ActivityRepository;

        public ActivityService(IActivityRepository activityRepository)
        {
            ActivityRepository = activityRepository;
        }

        public async Task CreateAsync(CreateActivityRequest request)
        {
            if (request.Image.Length <= 0 || ValidImageTypes.Contains(request.Image.FileName.Split('.').Last()))
                throw new ArgumentException("De foto is ongeldig.");

            MemoryStream image = new MemoryStream();
            await request.Image.CopyToAsync(image);

            Activity activity = request.ToActivity(image.ToArray());
            activity = await ActivityRepository.AddAsync(activity);
        }
    }
}
