using Activadis.Application.Validations.Activity;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Application.DTOs.Activity;
using Activadis.Application.Extensions;
using Activadis.Application.Interfaces;
using Activadis.Shared.DTOs.Elo.Stats;
using Activadis.Shared.DTOs.Activity;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;

namespace Activadis.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityPhotoRepository ActivityPhotoRepository;
        private readonly IActivityRepository ActivityRepository;
        private readonly ISignUpRepository SignUpRepository;
        private readonly ICategoryRepository CategoryRepository;
        private readonly IMatchRepository MatchRepository;

        public ActivityService(IActivityRepository activityRepository, IActivityPhotoRepository activityPhotoRepository, ISignUpRepository signUpRepository, ICategoryRepository categoryRepository, IMatchRepository matchRepository)
        {
            ActivityRepository = activityRepository;
            ActivityPhotoRepository = activityPhotoRepository;
            SignUpRepository = signUpRepository;
            CategoryRepository = categoryRepository;
            MatchRepository = matchRepository;
        }

        public async Task CreateAsync(CreateActivityRequest request)
        {
            request.Validate();
            request.Photos.ValidatePhotos(0);

            IEnumerable<Category> categories = await CategoryRepository.GetByIdsAsync(request.CategoryIds);
            request.CategoryIds.ValidateCategories(categories, []);

            List<ActivityPhoto> photos = await request.Photos.ToActivityPhotosAsync(0);

            Activity activity = request.ToActivity(photos, categories);
            activity = await ActivityRepository.AddAsync(activity);
        }

        public async Task EditAsync(EditActivityRequest request)
        {
            request.Validate();

            Activity activity = await ActivityRepository.GetDetailAsync(request.ActivityId)
                ?? throw new ArgumentException("De activiteit is niet gevonden.");

            activity.Name = request.Name;
            activity.Description = request.Description;
            activity.Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim();
            activity.CostPerPerson = request.CostPerPerson;
            activity.Location = request.Location;

            activity.MinParticipants = request.MinParticipants;
            if (request.MaxParticipants < activity.SignUps.Count)
                throw new ArgumentException("Het maximum aantal deelnemers kan niet onder het huidige aantal deelnemers zitten.");

            activity.MaxParticipants = request.MaxParticipants;

            activity.FoodIncluded = request.FoodIncluded;
            if (activity.PublishedOn is not null && request.IsDraft && activity.SignUps.Count > 0)
                throw new ArgumentException("De activiteit kan niet meer een draft worden nadat er al iemand aan deelneemt.");

            activity.PublishedOn = request.IsDraft ? null : DateTime.UtcNow;

            activity.StartDate = request.StartDate;
            activity.EndDate = request.EndDate;

            activity.SignUpDeadline = request.SignUpDeadline;
            activity.SignOutDeadline = request.SignOutDeadline;

            await ActivityRepository.UpdateAsync(activity);
        }

        public async Task<IEnumerable<ActivityOverviewResponse>> GetActivitiesAsync(Guid userId, UserRole role, ActivityFilterRequest request)
        {
            bool isAdmin = role == UserRole.Admin;

            IEnumerable<Activity> activities = await ActivityRepository.GetListAsync(isAdmin, request.ToFilter(isAdmin));

            Dictionary<Guid, bool> activitiesSignedIn = activities.ToDictionary(x => x.Id, x => false);
            foreach (Activity activity in activities)
            {
                bool hasSignedIn = await SignUpRepository.HasSignedUpAsync(userId, activity.Id);
                activitiesSignedIn[activity.Id] = hasSignedIn;
            }

            IEnumerable<Guid> activityIdsWithPhotos = await ActivityPhotoRepository.GetActivityIdsWithPhotosAsync(activities.Select(x => x.Id));
            return activities.Select(activity => activity.ToOverviewResponse(activitiesSignedIn[activity.Id], activityIdsWithPhotos.Contains(activity.Id)));
        }

        public async Task<ActivityDetailResponse> GetDetailAsync(Guid id, Guid userId, bool isAdmin)
        {
            Activity activity = await ActivityRepository.GetDetailAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            if (activity.PublishedOn is null && !isAdmin)
                throw new KeyNotFoundException("De activiteit is niet gevonden.");

            if (userId == Guid.Empty && !activity.ExternalAllowed)
                throw new ArgumentException("Deze activiteit accepteert geen externe deelnemers.");

            bool hasSignedUp = userId != Guid.Empty && await SignUpRepository.HasSignedUpAsync(userId, id);
            IEnumerable<Guid> photoIds = await ActivityPhotoRepository.GetIdsByActivityIdAsync(id);
            return activity.ToDetailResponse(hasSignedUp, userId != Guid.Empty, photoIds);
        }

        public async Task<ActivityPhotoResponse> GetPhotoAsync(Guid id)
        {
            ActivityPhoto photo = await ActivityPhotoRepository.GetFirstByActivityIdAsync(id)
                ?? throw new KeyNotFoundException("De activiteit heeft geen foto.");

            return photo.ToPhotoResponse();
        }

        public async Task<ActivityPhotoResponse> GetPhotoByIdAsync(Guid id, Guid photoId)
        {
            ActivityPhoto photo = await ActivityPhotoRepository.GetByIdAndActivityIdAsync(photoId, id)
                ?? throw new KeyNotFoundException("De foto is niet gevonden.");

            return photo.ToPhotoResponse();
        }

        public async Task AddPhotosAsync(Guid id, AddActivityPhotosRequest request)
        {
            Activity activity = await ActivityRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            int photoCount = await ActivityPhotoRepository.CountByActivityIdAsync(id);
            request.Photos.ValidatePhotos(photoCount);

            List<ActivityPhoto> photos = await request.Photos.ToActivityPhotosAsync(photoCount);
            foreach (ActivityPhoto photo in photos)
            {
                photo.ActivityId = id;
                await ActivityPhotoRepository.AddAsync(photo);
            }

            await ActivityRepository.UpdateAsync(activity);
        }

        public async Task UpdatePhotoOrderAsync(Guid id, UpdateActivityPhotoOrderRequest request)
        {
            Activity activity = await ActivityRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            IEnumerable<Guid> photoIds = await ActivityPhotoRepository.GetIdsByActivityIdAsync(id);
            request.PhotoIds.ValidatePhotoOrder(photoIds);

            await SavePhotoOrderAsync(request.PhotoIds);
            await ActivityRepository.UpdateAsync(activity);
        }

        public async Task DeletePhotoAsync(Guid id, Guid photoId)
        {
            Activity activity = await ActivityRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            ActivityPhoto photo = await ActivityPhotoRepository.GetByIdAndActivityIdAsync(photoId, id)
                ?? throw new KeyNotFoundException("De foto is niet gevonden.");

            await ActivityPhotoRepository.DeleteAsync(photo);
            await SavePhotoOrderAsync(await ActivityPhotoRepository.GetIdsByActivityIdAsync(id));
            await ActivityRepository.UpdateAsync(activity);
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

            IEnumerable<Guid> activityIdsWithPhotos = await ActivityPhotoRepository.GetActivityIdsWithPhotosAsync(activities.Select(x => x.Id));
            return activities.Select(activity => activity.ToOverviewResponse(activitiesSignedIn[activity.Id], activityIdsWithPhotos.Contains(activity.Id)));
        }

        public async Task UpdateCategoriesAsync(Guid id, UpdateActivityCategoriesRequest request)
        {
            Activity activity = await ActivityRepository.GetWithCategoriesAndMatchesAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            IEnumerable<Category> categories = await CategoryRepository.GetByIdsAsync(request.CategoryIds);
            request.CategoryIds.ValidateCategories(categories, activity.Categories.Select(x => x.Id));
            activity.ValidateCategoryRemoval(request.CategoryIds);

            activity.Categories.Clear();
            foreach (Category category in categories)
                activity.Categories.Add(category);

            await ActivityRepository.UpdateAsync(activity);
        }

        public async Task<IEnumerable<ActivityParticipantResponse>> GetParticipantsAsync(Guid id)
        {
            Activity activity = await ActivityRepository.GetDetailAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            return activity.SignUps
                .Where(x => x.UserId is not null)
                .Select(x => x.ToParticipantResponse())
                .OrderBy(x => x.FullName);
        }

        public async Task<IEnumerable<RecentMatchDto>> GetLinkableMatchesAsync(Guid id)
        {
            Activity activity = await ActivityRepository.GetWithCategoriesAndMatchesAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            IEnumerable<Match> matches = await MatchRepository.GetUnlinkedMatchesAsync(activity.Categories.Select(x => x.Id).ToList());
            return matches.Select(x => x.ToRecentMatchDto());
        }

        public async Task LinkMatchAsync(Guid id, LinkMatchRequest request)
        {
            Activity activity = await ActivityRepository.GetWithCategoriesAndMatchesAsync(id)
                ?? throw new KeyNotFoundException("De activiteit is niet gevonden.");

            Match match = await MatchRepository.GetByIdAsync(request.MatchId)
                ?? throw new KeyNotFoundException("De wedstrijd is niet gevonden.");

            match.ValidateLink(activity);

            match.ActivityId = activity.Id;
            await MatchRepository.UpdateAsync(match);
        }

        public async Task UnlinkMatchAsync(Guid id, Guid matchId)
        {
            Match match = await MatchRepository.GetByIdAsync(matchId)
                ?? throw new KeyNotFoundException("De wedstrijd is niet gevonden.");

            match.ValidateUnlink(id);

            match.ActivityId = null;
            await MatchRepository.UpdateAsync(match);
        }

        private async Task SavePhotoOrderAsync(IEnumerable<Guid> photoIds)
        {
            int sortOrder = 0;
            foreach (Guid photoId in photoIds)
                await ActivityPhotoRepository.UpdateSortOrderAsync(photoId, sortOrder++);
        }
    }
}