using Activadis.Application.DTOs.Activity;
using Activadis.Application.Interfaces;
using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;
using Activadis.Domain.Filters;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.Activity;
using Moq;
using Xunit;
using Match = Activadis.Domain.Entities.Match;

namespace Activadis.UnitTests
{
    public class ActivityServiceTests
    {
        private readonly Mock<IActivityRepository> _activityRepositoryMock = new();
        private readonly Mock<IActivityPhotoRepository> _photoRepositoryMock = new();
        private readonly Mock<ISignUpRepository> _signUpRepositoryMock = new();
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
        private readonly Mock<IMatchRepository> _matchRepositoryMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly ActivityService _sut;

        public ActivityServiceTests()
        {
            _sut = new ActivityService(
                _activityRepositoryMock.Object,
                _photoRepositoryMock.Object,
                _signUpRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _matchRepositoryMock.Object,
                _emailServiceMock.Object);

            _activityRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Activity>())).ReturnsAsync((Activity activity) => activity);
            _categoryRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([]);
            _photoRepositoryMock.Setup(r => r.GetActivityIdsWithPhotosAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([]);
            _photoRepositoryMock.Setup(r => r.GetIdsByActivityIdAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        }

        private static CreateActivityRequest CreateRequest()
        {
            DateTime start = DateTime.UtcNow.AddDays(10);

            return new CreateActivityRequest()
            {
                Name = "Bowlingavond",
                Description = "Een avond bowlen.",
                Location = "Doetinchem",
                MaxParticipants = 10,
                StartDate = start,
                EndDate = start.AddHours(3),
                SignUpDeadline = start.AddDays(-3),
                SignOutDeadline = start.AddDays(-2),
                Photos = [TestData.CreateImage()]
            };
        }

        private static EditActivityRequest EditRequest(Activity activity)
        {
            return new EditActivityRequest()
            {
                ActivityId = activity.Id,
                Name = "Nieuwe naam",
                Description = "Nieuwe beschrijving",
                Location = "Ulft",
                MaxParticipants = 20,
                IsDraft = false,
                StartDate = activity.StartDate,
                EndDate = activity.EndDate,
                SignUpDeadline = activity.SignUpDeadline,
                SignOutDeadline = activity.SignOutDeadline
            };
        }

        [Fact]
        public async Task CreateAsync_EmptyName_ThrowsArgumentException()
        {
            CreateActivityRequest request = CreateRequest();
            request.Name = " ";

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(request));

            _activityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Activity>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_TooManyPhotos_ThrowsArgumentException()
        {
            CreateActivityRequest request = CreateRequest();
            request.Photos = Enumerable.Range(0, 11).Select(_ => TestData.CreateImage()).ToList();

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_InactiveCategory_ThrowsArgumentException()
        {
            Category category = new Category() { Id = Guid.NewGuid(), Name = "Darts", IsActive = false };
            CreateActivityRequest request = CreateRequest();
            request.CategoryIds = [category.Id];
            _categoryRepositoryMock.Setup(r => r.GetByIdsAsync(request.CategoryIds)).ReturnsAsync([category]);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_AddsActivityWithPhotoAndCategories()
        {
            Category category = new Category() { Id = Guid.NewGuid(), Name = "Darts", IsActive = true };
            CreateActivityRequest request = CreateRequest();
            request.Requirements = "  Eigen pijlen  ";
            request.CategoryIds = [category.Id];
            _categoryRepositoryMock.Setup(r => r.GetByIdsAsync(request.CategoryIds)).ReturnsAsync([category]);
            Activity? added = null;
            _activityRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Activity>()))
                .Callback<Activity>(activity => added = activity)
                .ReturnsAsync((Activity activity) => activity);

            await _sut.CreateAsync(request);

            Assert.NotNull(added);
            Assert.Equal("Bowlingavond", added.Name);
            Assert.Equal("Eigen pijlen", added.Requirements);
            Assert.Single(added.Photos);
            Assert.Equal("image/jpeg", added.Photos.First().ImageContentType);
            Assert.Equal(category, Assert.Single(added.Categories));
            Assert.Null(added.PublishedOn);
        }

        [Fact]
        public async Task CreateAsync_PhotoIsNotAnImage_ThrowsArgumentException()
        {
            CreateActivityRequest request = CreateRequest();
            request.Photos = [TestData.CreateImage(content: [1, 2, 3, 4, 5])];

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(request));
        }

        [Fact]
        public async Task EditAsync_ActivityNotFound_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetDetailAsync(activity.Id.ToString())).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.EditAsync(EditRequest(activity)));
        }

        [Fact]
        public async Task EditAsync_MaxParticipantsBelowSignUps_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity(x => x.SignUps = [TestData.CreateSignUp(x.Id), TestData.CreateSignUp(x.Id)]);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync(activity.Id.ToString())).ReturnsAsync(activity);
            EditActivityRequest request = EditRequest(activity);
            request.MaxParticipants = 1;

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.EditAsync(request));

            _activityRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Activity>()), Times.Never);
        }

        [Fact]
        public async Task EditAsync_BackToDraftWithSignUps_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity(x => x.SignUps = [TestData.CreateSignUp(x.Id)]);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync(activity.Id.ToString())).ReturnsAsync(activity);
            EditActivityRequest request = EditRequest(activity);
            request.IsDraft = true;

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.EditAsync(request));
        }

        [Fact]
        public async Task EditAsync_ValidRequest_UpdatesActivity()
        {
            Activity activity = TestData.CreateActivity(x => x.Requirements = "Oud");
            _activityRepositoryMock.Setup(r => r.GetDetailAsync(activity.Id.ToString())).ReturnsAsync(activity);
            EditActivityRequest request = EditRequest(activity);
            request.Requirements = " ";

            await _sut.EditAsync(request);

            Assert.Equal("Nieuwe naam", activity.Name);
            Assert.Equal("Ulft", activity.Location);
            Assert.Equal(20, activity.MaxParticipants);
            Assert.Null(activity.Requirements);
            Assert.NotNull(activity.PublishedOn);
            _activityRepositoryMock.Verify(r => r.UpdateAsync(activity), Times.Once);
        }

        [Fact]
        public async Task GetActivitiesAsync_ReturnsSignedUpAndPhotoFlagsPerActivity()
        {
            Guid userId = Guid.NewGuid();
            Activity signedUp = TestData.CreateActivity();
            Activity other = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetListAsync(false, It.IsAny<ActivityFilter>())).ReturnsAsync([signedUp, other]);
            _signUpRepositoryMock.Setup(r => r.HasSignedUpAsync(userId, signedUp.Id)).ReturnsAsync(true);
            _photoRepositoryMock.Setup(r => r.GetActivityIdsWithPhotosAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync([other.Id]);

            List<ActivityOverviewResponse> result = (await _sut.GetActivitiesAsync(userId, UserRole.User, new ActivityFilterRequest())).ToList();

            Assert.Equal(2, result.Count);
            Assert.True(result.Single(x => x.Id == signedUp.Id).HasSignedIn);
            Assert.False(result.Single(x => x.Id == signedUp.Id).HasPhoto);
            Assert.False(result.Single(x => x.Id == other.Id).HasSignedIn);
            Assert.True(result.Single(x => x.Id == other.Id).HasPhoto);
        }

        [Fact]
        public async Task GetActivitiesAsync_Admin_PassesIncludePastToFilter()
        {
            ActivityFilter? filter = null;
            _activityRepositoryMock.Setup(r => r.GetListAsync(true, It.IsAny<ActivityFilter>()))
                .Callback<bool, ActivityFilter>((_, f) => filter = f)
                .ReturnsAsync([]);

            await _sut.GetActivitiesAsync(Guid.NewGuid(), UserRole.Admin, new ActivityFilterRequest() { IncludePast = true });

            Assert.NotNull(filter);
            Assert.True(filter.IncludePast);
        }

        [Fact]
        public async Task GetDetailAsync_ActivityNotFound_ThrowsKeyNotFoundException()
        {
            _activityRepositoryMock.Setup(r => r.GetDetailAsync("onbekend")).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetDetailAsync("onbekend", Guid.NewGuid(), false));
        }

        [Fact]
        public async Task GetDetailAsync_DraftForEmployee_ThrowsKeyNotFoundException()
        {
            Activity activity = TestData.CreateActivity(x => x.PublishedOn = null);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync("bowlingavond")).ReturnsAsync(activity);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetDetailAsync("bowlingavond", Guid.NewGuid(), false));
        }

        [Fact]
        public async Task GetDetailAsync_DraftForAdmin_ReturnsDetail()
        {
            Activity activity = TestData.CreateActivity(x => x.PublishedOn = null);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync("bowlingavond")).ReturnsAsync(activity);

            ActivityDetailResponse result = await _sut.GetDetailAsync("bowlingavond", Guid.NewGuid(), true);

            Assert.True(result.IsDraft);
        }

        [Fact]
        public async Task GetDetailAsync_ExternalVisitorOnInternalActivity_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity(x => x.ExternalAllowed = false);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync("bowlingavond")).ReturnsAsync(activity);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetDetailAsync("bowlingavond", Guid.Empty, false));
        }

        [Fact]
        public async Task GetDetailAsync_ExternalVisitor_DoesNotIncludeEloData()
        {
            Activity activity = TestData.CreateActivity(x => x.Categories = [new Category() { Id = Guid.NewGuid(), Name = "Darts" }]);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync("bowlingavond")).ReturnsAsync(activity);

            ActivityDetailResponse result = await _sut.GetDetailAsync("bowlingavond", Guid.Empty, false);

            Assert.Empty(result.Categories);
            Assert.False(result.HasSignedUp);
            _signUpRepositoryMock.Verify(r => r.HasSignedUpAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetDetailAsync_Employee_ReturnsSignUpStatusCategoriesAndPhotos()
        {
            Guid userId = Guid.NewGuid();
            Guid photoId = Guid.NewGuid();
            Activity activity = TestData.CreateActivity(x => x.Categories = [new Category() { Id = Guid.NewGuid(), Name = "Darts" }]);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync("bowlingavond")).ReturnsAsync(activity);
            _signUpRepositoryMock.Setup(r => r.HasSignedUpAsync(userId, activity.Id)).ReturnsAsync(true);
            _photoRepositoryMock.Setup(r => r.GetIdsByActivityIdAsync(activity.Id)).ReturnsAsync([photoId]);

            ActivityDetailResponse result = await _sut.GetDetailAsync("bowlingavond", userId, false);

            Assert.True(result.HasSignedUp);
            Assert.Single(result.Categories);
            Assert.Equal(photoId, Assert.Single(result.PhotoIds));
        }

        [Fact]
        public async Task GetPhotoAsync_NoPhoto_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            _photoRepositoryMock.Setup(r => r.GetFirstByActivityIdAsync(activityId)).ReturnsAsync((ActivityPhoto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetPhotoAsync(activityId));
        }

        [Fact]
        public async Task GetPhotoAsync_PhotoExists_ReturnsImage()
        {
            Guid activityId = Guid.NewGuid();
            ActivityPhoto photo = new ActivityPhoto() { Image = TestData.JpegBytes, ImageContentType = "image/jpeg" };
            _photoRepositoryMock.Setup(r => r.GetFirstByActivityIdAsync(activityId)).ReturnsAsync(photo);

            ActivityPhotoResponse result = await _sut.GetPhotoAsync(activityId);

            Assert.Equal(TestData.JpegBytes, result.Image);
            Assert.Equal("image/jpeg", result.ImageContentType);
        }

        [Fact]
        public async Task GetPhotoByIdAsync_PhotoNotFound_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            Guid photoId = Guid.NewGuid();
            _photoRepositoryMock.Setup(r => r.GetByIdAndActivityIdAsync(photoId, activityId)).ReturnsAsync((ActivityPhoto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetPhotoByIdAsync(activityId, photoId));
        }

        [Fact]
        public async Task AddPhotosAsync_ActivityNotFound_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.AddPhotosAsync(activityId, new AddActivityPhotosRequest() { Photos = [TestData.CreateImage()] }));
        }

        [Fact]
        public async Task AddPhotosAsync_ExistingPhotos_AddsNewPhotosAfterThem()
        {
            Activity activity = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _photoRepositoryMock.Setup(r => r.CountByActivityIdAsync(activity.Id)).ReturnsAsync(2);
            List<ActivityPhoto> added = [];
            _photoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ActivityPhoto>()))
                .Callback<ActivityPhoto>(added.Add)
                .ReturnsAsync((ActivityPhoto photo) => photo);

            await _sut.AddPhotosAsync(activity.Id, new AddActivityPhotosRequest() { Photos = [TestData.CreateImage(), TestData.CreateImage()] });

            Assert.Equal([2, 3], added.Select(x => x.SortOrder));
            Assert.All(added, x => Assert.Equal(activity.Id, x.ActivityId));
            _activityRepositoryMock.Verify(r => r.UpdateAsync(activity), Times.Once);
        }

        [Fact]
        public async Task AddPhotosAsync_MoreThanMaximum_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _photoRepositoryMock.Setup(r => r.CountByActivityIdAsync(activity.Id)).ReturnsAsync(10);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPhotosAsync(activity.Id, new AddActivityPhotosRequest() { Photos = [TestData.CreateImage()] }));

            _photoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ActivityPhoto>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePhotoOrderAsync_UnknownPhoto_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _photoRepositoryMock.Setup(r => r.GetIdsByActivityIdAsync(activity.Id)).ReturnsAsync([Guid.NewGuid()]);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdatePhotoOrderAsync(activity.Id, new UpdateActivityPhotoOrderRequest() { PhotoIds = [Guid.NewGuid()] }));
        }

        [Fact]
        public async Task UpdatePhotoOrderAsync_ValidOrder_SavesSortOrderPerPhoto()
        {
            Activity activity = TestData.CreateActivity();
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _photoRepositoryMock.Setup(r => r.GetIdsByActivityIdAsync(activity.Id)).ReturnsAsync([first, second]);

            await _sut.UpdatePhotoOrderAsync(activity.Id, new UpdateActivityPhotoOrderRequest() { PhotoIds = [second, first] });

            _photoRepositoryMock.Verify(r => r.UpdateSortOrderAsync(second, 0), Times.Once);
            _photoRepositoryMock.Verify(r => r.UpdateSortOrderAsync(first, 1), Times.Once);
        }

        [Fact]
        public async Task DeletePhotoAsync_PhotoNotFound_ThrowsKeyNotFoundException()
        {
            Activity activity = TestData.CreateActivity();
            Guid photoId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _photoRepositoryMock.Setup(r => r.GetByIdAndActivityIdAsync(photoId, activity.Id)).ReturnsAsync((ActivityPhoto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeletePhotoAsync(activity.Id, photoId));
        }

        [Fact]
        public async Task DeletePhotoAsync_PhotoExists_DeletesAndRenumbersTheRest()
        {
            Activity activity = TestData.CreateActivity();
            ActivityPhoto photo = new ActivityPhoto() { Id = Guid.NewGuid(), ActivityId = activity.Id };
            Guid remaining = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _photoRepositoryMock.Setup(r => r.GetByIdAndActivityIdAsync(photo.Id, activity.Id)).ReturnsAsync(photo);
            _photoRepositoryMock.Setup(r => r.GetIdsByActivityIdAsync(activity.Id)).ReturnsAsync([remaining]);

            await _sut.DeletePhotoAsync(activity.Id, photo.Id);

            _photoRepositoryMock.Verify(r => r.DeleteAsync(photo), Times.Once);
            _photoRepositoryMock.Verify(r => r.UpdateSortOrderAsync(remaining, 0), Times.Once);
        }

        [Fact]
        public async Task GetSignedUpAsync_ReturnsActivitiesOfTheUser()
        {
            Guid userId = Guid.NewGuid();
            Activity activity = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetSignedUpByUserIdAsync(userId)).ReturnsAsync([activity]);
            _signUpRepositoryMock.Setup(r => r.HasSignedUpAsync(userId, activity.Id)).ReturnsAsync(true);

            ActivityOverviewResponse result = Assert.Single(await _sut.GetSignedUpAsync(userId));

            Assert.Equal(activity.Id, result.Id);
            Assert.True(result.HasSignedIn);
        }

        [Fact]
        public async Task UpdateCategoriesAsync_ActivityNotFound_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activityId)).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateCategoriesAsync(activityId, new UpdateActivityCategoriesRequest()));
        }

        [Fact]
        public async Task UpdateCategoriesAsync_ValidCategories_ReplacesCategories()
        {
            Category old = new Category() { Id = Guid.NewGuid(), Name = "Pool", IsActive = true };
            Category darts = new Category() { Id = Guid.NewGuid(), Name = "Darts", IsActive = true };
            Activity activity = TestData.CreateActivity(x => x.Categories = [old]);
            UpdateActivityCategoriesRequest request = new UpdateActivityCategoriesRequest() { CategoryIds = [darts.Id] };
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activity.Id)).ReturnsAsync(activity);
            _categoryRepositoryMock.Setup(r => r.GetByIdsAsync(request.CategoryIds)).ReturnsAsync([darts]);

            await _sut.UpdateCategoriesAsync(activity.Id, request);

            Assert.Equal(darts, Assert.Single(activity.Categories));
            _activityRepositoryMock.Verify(r => r.UpdateAsync(activity), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoriesAsync_RemovesCategoryWithLinkedMatch_ThrowsArgumentException()
        {
            Category darts = new Category() { Id = Guid.NewGuid(), Name = "Darts", IsActive = true };
            Activity activity = TestData.CreateActivity(x =>
            {
                x.Categories = [darts];
                x.Matches = [new Match() { Id = Guid.NewGuid(), CategoryId = darts.Id, Category = darts }];
            });
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activity.Id)).ReturnsAsync(activity);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateCategoriesAsync(activity.Id, new UpdateActivityCategoriesRequest()));

            _activityRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Activity>()), Times.Never);
        }

        [Fact]
        public async Task GetParticipantsAsync_ReturnsOnlyEmployeesOrderedByName()
        {
            Activity activity = TestData.CreateActivity(x => x.SignUps =
            [
                TestData.CreateSignUp(x.Id, s => { s.UserId = Guid.NewGuid(); s.FullName = "Thijs"; }),
                TestData.CreateSignUp(x.Id, s => s.FullName = "Externe gast"),
                TestData.CreateSignUp(x.Id, s => { s.UserId = Guid.NewGuid(); s.FullName = "Anna"; })
            ]);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync(activity.Id.ToString())).ReturnsAsync(activity);

            List<ActivityParticipantResponse> result = (await _sut.GetParticipantsAsync(activity.Id)).ToList();

            Assert.Equal(["Anna", "Thijs"], result.Select(x => x.FullName));
        }

        [Fact]
        public async Task GetSignUpsAsync_ActivityNotFound_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetSignUpsAsync(activityId));
        }

        [Fact]
        public async Task GetSignUpsAsync_SplitsParticipantsAndWaitlist()
        {
            Activity activity = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _signUpRepositoryMock.Setup(r => r.GetByActivityIdAsync(activity.Id)).ReturnsAsync(
            [
                TestData.CreateSignUp(activity.Id, s => { s.FullName = "Bram"; s.HasPlusOne = true; }),
                TestData.CreateSignUp(activity.Id, s => { s.FullName = "Wachtende 2"; s.WaitlistPosition = 2; }),
                TestData.CreateSignUp(activity.Id, s => s.FullName = "Anna"),
                TestData.CreateSignUp(activity.Id, s => { s.FullName = "Wachtende 1"; s.WaitlistPosition = 1; })
            ]);

            ActivitySignUpListResponse result = await _sut.GetSignUpsAsync(activity.Id);

            Assert.Equal(["Anna", "Bram"], result.Participants.Select(x => x.FullName));
            Assert.Equal(["Wachtende 1", "Wachtende 2"], result.Waitlist.Select(x => x.FullName));
            Assert.Equal(2, result.TotalParticipants);
            Assert.Equal(1, result.TotalPlusOnes);
        }

        [Fact]
        public async Task InformParticipantsAsync_ActivityNotFound_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.InformParticipantsAsync(activityId));
        }

        [Fact]
        public async Task InformParticipantsAsync_OneEmailFails_CountsInformedAndFailed()
        {
            Activity activity = TestData.CreateActivity();
            User employee = TestData.CreateUser();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _signUpRepositoryMock.Setup(r => r.GetParticipantsByActivityIdAsync(activity.Id)).ReturnsAsync(
            [
                TestData.CreateSignUp(activity.Id, s => { s.User = employee; s.UserId = employee.Id; s.Email = string.Empty; }),
                TestData.CreateSignUp(activity.Id)
            ]);
            _emailServiceMock.Setup(e => e.SendActivityChangedAsync(employee.Email, employee.FullName, activity)).ReturnsAsync(true);
            _emailServiceMock.Setup(e => e.SendActivityChangedAsync("emma@extern.nl", "Emma Smit", activity)).ReturnsAsync(false);

            InformParticipantsResponse result = await _sut.InformParticipantsAsync(activity.Id);

            Assert.Equal(2, result.TotalParticipants);
            Assert.Equal(1, result.InformedParticipants);
            Assert.Equal(1, result.FailedParticipants);
        }

        [Fact]
        public async Task CancelAsync_ActivityNotFound_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CancelAsync(activityId));

            _activityRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Activity>()), Times.Never);
        }

        [Fact]
        public async Task CancelAsync_ActivityExists_DeletesActivityAndSignUpsAndEmailsEveryone()
        {
            Activity activity = TestData.CreateActivity();
            _activityRepositoryMock.Setup(r => r.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
            _signUpRepositoryMock.Setup(r => r.GetByActivityIdAsync(activity.Id)).ReturnsAsync(
            [
                TestData.CreateSignUp(activity.Id),
                TestData.CreateSignUp(activity.Id, s => s.WaitlistPosition = 1)
            ]);
            _emailServiceMock.Setup(e => e.SendActivityCancelledAsync(It.IsAny<string>(), It.IsAny<string>(), activity)).ReturnsAsync(true);

            InformParticipantsResponse result = await _sut.CancelAsync(activity.Id);

            _activityRepositoryMock.Verify(r => r.DeleteAsync(activity), Times.Once);
            _signUpRepositoryMock.Verify(r => r.DeleteByActivityIdAsync(activity.Id), Times.Once);
            _emailServiceMock.Verify(e => e.SendActivityCancelledAsync(It.IsAny<string>(), It.IsAny<string>(), activity), Times.Exactly(2));
            Assert.Equal(2, result.InformedParticipants);
            Assert.Equal(0, result.FailedParticipants);
        }

        [Fact]
        public async Task GetLinkableMatchesAsync_ActivityNotFound_ThrowsKeyNotFoundException()
        {
            Guid activityId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activityId)).ReturnsAsync((Activity?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetLinkableMatchesAsync(activityId));
        }

        [Fact]
        public async Task GetLinkableMatchesAsync_ReturnsUnlinkedMatchesOfTheActivityCategories()
        {
            Category darts = new Category() { Id = Guid.NewGuid(), Name = "Darts" };
            Activity activity = TestData.CreateActivity(x => x.Categories = [darts]);
            Match match = new Match() { Id = Guid.NewGuid(), CategoryId = darts.Id, Category = darts, MatchDate = DateTime.UtcNow };
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activity.Id)).ReturnsAsync(activity);
            _matchRepositoryMock.Setup(r => r.GetUnlinkedMatchesAsync(It.Is<IEnumerable<Guid>>(ids => ids.Single() == darts.Id), activity.StartDate.Date, activity.EndDate.Date.AddDays(1))).ReturnsAsync([match]);

            var result = Assert.Single(await _sut.GetLinkableMatchesAsync(activity.Id));

            Assert.Equal(match.Id, result.MatchId);
            Assert.Equal("Darts", result.CategoryName);
        }

        [Fact]
        public async Task LinkMatchAsync_MatchNotFound_ThrowsKeyNotFoundException()
        {
            Activity activity = TestData.CreateActivity();
            Guid matchId = Guid.NewGuid();
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activity.Id)).ReturnsAsync(activity);
            _matchRepositoryMock.Setup(r => r.GetByIdAsync(matchId)).ReturnsAsync((Match?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.LinkMatchAsync(activity.Id, new LinkMatchRequest() { MatchId = matchId }));
        }

        [Fact]
        public async Task LinkMatchAsync_CategoryNotOnActivity_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();
            Match match = new Match() { Id = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activity.Id)).ReturnsAsync(activity);
            _matchRepositoryMock.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.LinkMatchAsync(activity.Id, new LinkMatchRequest() { MatchId = match.Id }));

            _matchRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Match>()), Times.Never);
        }

        [Fact]
        public async Task LinkMatchAsync_ValidMatch_SetsActivityOnMatch()
        {
            Category darts = new Category() { Id = Guid.NewGuid(), Name = "Darts" };
            Activity activity = TestData.CreateActivity(x => x.Categories = [darts]);
            Match match = new Match() { Id = Guid.NewGuid(), CategoryId = darts.Id };
            _activityRepositoryMock.Setup(r => r.GetWithCategoriesAndMatchesAsync(activity.Id)).ReturnsAsync(activity);
            _matchRepositoryMock.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

            await _sut.LinkMatchAsync(activity.Id, new LinkMatchRequest() { MatchId = match.Id });

            Assert.Equal(activity.Id, match.ActivityId);
            _matchRepositoryMock.Verify(r => r.UpdateAsync(match), Times.Once);
        }

        [Fact]
        public async Task UnlinkMatchAsync_MatchOfOtherActivity_ThrowsArgumentException()
        {
            Match match = new Match() { Id = Guid.NewGuid(), ActivityId = Guid.NewGuid() };
            _matchRepositoryMock.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UnlinkMatchAsync(Guid.NewGuid(), match.Id));
        }

        [Fact]
        public async Task UnlinkMatchAsync_LinkedMatch_RemovesActivityFromMatch()
        {
            Guid activityId = Guid.NewGuid();
            Match match = new Match() { Id = Guid.NewGuid(), ActivityId = activityId };
            _matchRepositoryMock.Setup(r => r.GetByIdAsync(match.Id)).ReturnsAsync(match);

            await _sut.UnlinkMatchAsync(activityId, match.Id);

            Assert.Null(match.ActivityId);
            _matchRepositoryMock.Verify(r => r.UpdateAsync(match), Times.Once);
        }
    }
}
