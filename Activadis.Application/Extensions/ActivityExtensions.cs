using Activadis.Application.Validations.Activity;
using Activadis.Application.DTOs.Activity;
using Activadis.Shared.DTOs.Activity;
using Microsoft.AspNetCore.Http;
using Activadis.Domain.Entities;

namespace Activadis.Application.Extensions
{
    public static class ActivityExtensions
    {
        public static Activity ToActivity(this CreateActivityRequest request, List<ActivityPhoto> photos, IEnumerable<Category> categories)
        {
            return new Activity()
            {
                Name = request.Name,
                Description = request.Description,
                Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim(),
                CostPerPerson = request.CostPerPerson,

                Location = request.Location,
                Photos = photos,
                Categories = categories.ToList(),

                MinParticipants = request.MinParticipants,
                MaxParticipants = request.MaxParticipants,

                FoodIncluded = request.FoodIncluded,
                ExternalAllowed = request.ExternalAllowed,
                PlusOneAllowed = request.PlusOneAllowed,

                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PublishedOn = request.IsDraft ? null : DateTime.UtcNow,

                SignUpDeadline = request.SignUpDeadline,
                SignOutDeadline = request.SignOutDeadline
            };
        }

        public async static Task<List<ActivityPhoto>> ToActivityPhotosAsync(this IEnumerable<IFormFile> files, int sortOrder)
        {
            List<ActivityPhoto> photos = [];
            foreach (IFormFile file in files)
            {
                using MemoryStream stream = new MemoryStream();
                await file.CopyToAsync(stream);

                byte[] image = stream.ToArray();
                image.ValidateImage(file.FileName);

                photos.Add(new ActivityPhoto()
                {
                    Image = image,
                    ImageContentType = file.ContentType,
                    SortOrder = sortOrder++,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return photos;
        }
        public static ActivityParticipantResponse ToParticipantResponse(this SignUp signUp)
        {
            return new ActivityParticipantResponse()
            {
                UserId = signUp.UserId ?? Guid.Empty,
                UserSlug = signUp.User?.Slug,
                FullName = signUp.User?.FullName ?? signUp.FullName
            };
        }

        public static ActivitySignUpResponse ToSignUpResponse(this SignUp signUp)
        {
            return new ActivitySignUpResponse()
            {
                Id = signUp.Id,
                UserId = signUp.UserId,
                UserSlug = signUp.User?.Slug,
                FullName = signUp.User?.FullName ?? signUp.FullName,
                Email = signUp.IsExternal ? signUp.Email : null,
                Comment = signUp.Comment,
                HasPlusOne = signUp.HasPlusOne,
                IsExternal = signUp.IsExternal,
                WaitlistPosition = signUp.WaitlistPosition
            };
        }

        public static ActivitySignUpListResponse ToSignUpListResponse(this IEnumerable<SignUp> signUps)
        {
            List<ActivitySignUpResponse> participants = signUps
                .Where(x => x.WaitlistPosition is null)
                .Select(x => x.ToSignUpResponse())
                .OrderBy(x => x.FullName)
                .ToList();

            return new ActivitySignUpListResponse()
            {
                Participants = participants,
                Waitlist = signUps
                    .Where(x => x.WaitlistPosition is not null)
                    .OrderBy(x => x.WaitlistPosition)
                    .Select(x => x.ToSignUpResponse())
                    .ToList(),
                TotalParticipants = participants.Count,
                TotalPlusOnes = participants.Count(x => x.HasPlusOne)
            };
        }

        public static ActivityOverviewResponse ToOverviewResponse(this Activity activity, bool hasSignedIn, bool hasPhoto)
        {
            return new ActivityOverviewResponse()
            {
                Id = activity.Id,
                Slug = activity.Slug,
                Name = activity.Name,

                Location = activity.Location,
                HasPhoto = hasPhoto,
                PhotoVersion = (activity.UpdatedAt ?? activity.CreatedAt).Ticks,
                IsDraft = activity.PublishedOn is null,
                IsDeleted = activity.DeletedAt is not null,
                HasTakenPlace = DateTime.UtcNow >= activity.StartDate,
                HasSignedIn = hasSignedIn,
                PlusOneAllowed = activity.PlusOneAllowed,

                StartDate = activity.StartDate,
                EndDate = activity.EndDate
            };
        }

        public static ActivityPhotoResponse ToPhotoResponse(this ActivityPhoto photo)
        {
            return new ActivityPhotoResponse()
            {
                Image = photo.Image,
                ImageContentType = photo.ImageContentType
            };
        }

        public static ActivityDetailResponse ToDetailResponse(this Activity activity, bool hasSignedUp, bool includeElo, IEnumerable<Guid> photoIds)
        {
            return new ActivityDetailResponse()
            {
                Id = activity.Id,
                Slug = activity.Slug,
                Name = activity.Name,
                Description = activity.Description,
                Requirements = activity.Requirements,
                CostPerPerson = activity.CostPerPerson,

                Location = activity.Location,
                PhotoIds = photoIds.ToList(),

                MinParticipants = activity.MinParticipants,
                MaxParticipants = activity.MaxParticipants,
                HasParticipantLimit = activity.HasParticipantLimit(),
                TotalSignUps = activity.SignUps.Count,
                AvailableSpots = activity.AvailableSpots(),

                FoodIncluded = activity.FoodIncluded,
                ExternalAllowed = activity.ExternalAllowed,
                PlusOneAllowed = activity.PlusOneAllowed,
                IsDraft = activity.PublishedOn is null,

                StartDate = activity.StartDate,
                EndDate = activity.EndDate,

                SignUpDeadline = activity.SignUpDeadline,
                SignOutDeadline = activity.SignOutDeadline,

                HasTakenPlace = activity.HasTakenPlace(),
                IsFull = activity.IsFull(),
                SignUpDeadlinePassed = activity.SignUpDeadlinePassed(),
                IsOpenForSignUp = activity.IsOpenForSignUp(),
                HasSignedUp = hasSignedUp,

                Categories = includeElo
                    ? activity.Categories.OrderBy(x => x.DisplayOrder).Select(x => x.ToDto()).ToList()
                    : [],
                Matches = includeElo
                    ? activity.Matches.OrderByDescending(x => x.MatchDate).Select(x => x.ToRecentMatchDto()).ToList()
                    : []
            };
        }
    }
}
