using Activadis.Application.DTOs.Activity;
using Activadis.Application.Validations.Activity;
using Activadis.Application.Validations.Auth;
using Activadis.Application.Validations.SignUp;
using Activadis.Application.Validations.User;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;
using Activadis.Shared.DTOs.Auth;
using Activadis.Shared.DTOs.SignUp;
using Activadis.Shared.DTOs.User;
using Microsoft.AspNetCore.Http;
using Xunit;
using Match = Activadis.Domain.Entities.Match;

namespace Activadis.UnitTests
{
    public class ValidationTests
    {
        private static CreateActivityRequest ActivityRequest(Action<CreateActivityRequest>? configure = null)
        {
            DateTime start = DateTime.UtcNow.AddDays(10);
            CreateActivityRequest request = new CreateActivityRequest()
            {
                Name = "Bowlingavond",
                Description = "Een avond bowlen.",
                Location = "Doetinchem",
                StartDate = start,
                EndDate = start.AddHours(3),
                SignUpDeadline = start.AddDays(-3),
                SignOutDeadline = start.AddDays(-2)
            };

            configure?.Invoke(request);
            return request;
        }

        [Fact]
        public void ActivityRequest_Valid_DoesNotThrow()
        {
            ActivityRequest().Validate();
        }

        public static TheoryData<Action<CreateActivityRequest>> InvalidActivityRequests => new()
        {
            x => x.Name = " ",
            x => x.Description = string.Empty,
            x => x.Location = string.Empty,
            x => x.CostPerPerson = -1,
            x => x.MinParticipants = -1,
            x => x.MaxParticipants = -1,
            x => x.StartDate = DateTime.MinValue,
            x => x.EndDate = DateTime.MinValue,
            x => x.EndDate = x.StartDate,
            x => x.SignUpDeadline = DateTime.MinValue,
            x => x.SignOutDeadline = DateTime.MinValue
        };

        [Theory]
        [MemberData(nameof(InvalidActivityRequests))]
        public void ActivityRequest_Invalid_ThrowsArgumentException(Action<CreateActivityRequest> configure)
        {
            Assert.Throws<ArgumentException>(() => ActivityRequest(configure).Validate());
        }

        [Fact]
        public void ValidatePhotos_ValidPhotos_DoesNotThrow()
        {
            List<IFormFile> photos = [TestData.CreateImage(), TestData.CreateImage("foto.png", "image/png")];

            photos.ValidatePhotos(0);
        }

        [Theory]
        [InlineData("foto.gif", "image/gif", 6)]
        [InlineData("foto.jpg", "image/jpeg", 0)]
        public void ValidatePhotos_InvalidPhoto_ThrowsArgumentException(string fileName, string contentType, int length)
        {
            List<IFormFile> photos = [TestData.CreateImage(fileName, contentType, new byte[length])];

            Assert.Throws<ArgumentException>(() => photos.ValidatePhotos(0));
        }

        [Fact]
        public void ValidatePhotos_AboveMaximum_ThrowsArgumentException()
        {
            List<IFormFile> photos = [TestData.CreateImage()];

            Assert.Throws<ArgumentException>(() => photos.ValidatePhotos(10));
        }

        [Fact]
        public void ValidateImage_Png_DoesNotThrow()
        {
            byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

            png.ValidateImage("foto.png");
        }

        [Fact]
        public void ValidateImage_Webp_DoesNotThrow()
        {
            byte[] webp = [.. "RIFF"u8.ToArray(), 0, 0, 0, 0, .. "WEBP"u8.ToArray(), 0];

            webp.ValidateImage("foto.webp");
        }

        [Fact]
        public void ValidateImage_NoImage_ThrowsArgumentException()
        {
            byte[] text = "Dit is geen foto"u8.ToArray();

            Assert.Throws<ArgumentException>(() => text.ValidateImage("foto.jpg"));
        }

        [Fact]
        public void ValidatePhotoOrder_SamePhotosInOtherOrder_DoesNotThrow()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();

            new List<Guid>() { second, first }.ValidatePhotoOrder([first, second]);
        }

        [Fact]
        public void ValidatePhotoOrder_DuplicatePhoto_ThrowsArgumentException()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() => new List<Guid>() { first, first }.ValidatePhotoOrder([first, second]));
        }

        [Fact]
        public void ValidateCategories_UnknownCategory_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new List<Guid>() { Guid.NewGuid() }.ValidateCategories([], []));
        }

        [Fact]
        public void ValidateCategories_InactiveButAlreadyLinked_DoesNotThrow()
        {
            Category category = new Category() { Id = Guid.NewGuid(), Name = "Darts", IsActive = false };

            new List<Guid>() { category.Id }.ValidateCategories([category], [category.Id]);
        }

        [Fact]
        public void ValidateCategory_CategoryNotOnActivity_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();

            Assert.Throws<ArgumentException>(() => activity.ValidateCategory(Guid.NewGuid()));
        }

        [Fact]
        public void ValidateCategoryRemoval_NoLinkedMatchInCategory_DoesNotThrow()
        {
            Guid categoryId = Guid.NewGuid();
            Activity activity = TestData.CreateActivity(x => x.Matches = [new Match() { CategoryId = categoryId }]);

            activity.ValidateCategoryRemoval([categoryId]);
        }

        [Fact]
        public void ValidateLink_AlreadyLinkedToThisActivity_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();
            Match match = new Match() { ActivityId = activity.Id };

            Assert.Throws<ArgumentException>(() => match.ValidateLink(activity));
        }

        [Fact]
        public void ValidateLink_LinkedToOtherActivity_ThrowsArgumentException()
        {
            Category darts = new Category() { Id = Guid.NewGuid(), Name = "Darts" };
            Activity activity = TestData.CreateActivity(x => x.Categories = [darts]);
            Match match = new Match() { ActivityId = Guid.NewGuid(), CategoryId = darts.Id };

            Assert.Throws<ArgumentException>(() => match.ValidateLink(activity));
        }

        [Fact]
        public void ValidateUnlink_LinkedToActivity_DoesNotThrow()
        {
            Guid activityId = Guid.NewGuid();

            new Match() { ActivityId = activityId }.ValidateUnlink(activityId);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("Kort1!", "Kort1!")]
        [InlineData("geenhoofdletter1!", "geenhoofdletter1!")]
        [InlineData("Geencijfer!", "Geencijfer!")]
        [InlineData("Geenteken1", "Geenteken1")]
        [InlineData("Geheim12!", "Anders12!")]
        public void SetPasswordRequest_Invalid_ThrowsArgumentException(string password, string confirmPassword)
        {
            SetPasswordRequest request = new SetPasswordRequest() { Password = password, ConfirmPassword = confirmPassword };

            Assert.Throws<ArgumentException>(() => request.Validate());
        }

        [Fact]
        public void SetPasswordRequest_Valid_DoesNotThrow()
        {
            new SetPasswordRequest() { Password = "Geheim12!", ConfirmPassword = "Geheim12!" }.Validate();
        }

        [Fact]
        public void SignUpRequest_ValidEmployee_DoesNotThrow()
        {
            Activity activity = TestData.CreateActivity();

            new SignUpRequest() { ActivityId = activity.Id }.Validate(activity, Guid.NewGuid(), 0, null);
        }

        public static TheoryData<Action<Activity>, bool, bool> InvalidSignUps => new()
        {
            { x => x.ExternalAllowed = false, true, false },
            { x => x.MaxParticipants = 0, false, false },
            { x => x.PlusOneAllowed = false, false, true },
            { x => x.SignUpDeadline = DateTime.UtcNow.AddMinutes(-1), false, false }
        };

        [Theory]
        [MemberData(nameof(InvalidSignUps))]
        public void SignUpRequest_ActivityDoesNotAllowIt_ThrowsArgumentException(Action<Activity> configure, bool isExternal, bool hasPlusOne)
        {
            Activity activity = TestData.CreateActivity(configure);
            SignUpRequest request = new SignUpRequest() { ActivityId = activity.Id, FullName = "Emma Smit", Email = "emma@extern.nl", HasPlusOne = hasPlusOne };

            Assert.Throws<ArgumentException>(() => request.Validate(activity, isExternal ? Guid.Empty : Guid.NewGuid(), 0, null));
        }

        [Fact]
        public void SignUpRequest_InactiveUser_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();
            User user = TestData.CreateUser(x => x.IsActive = false);

            Assert.Throws<ArgumentException>(() => new SignUpRequest().Validate(activity, user.Id, 0, user));
        }

        [Fact]
        public void SignUpRequest_ExternalWithoutName_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity();

            Assert.Throws<ArgumentException>(() => new SignUpRequest() { Email = "emma@extern.nl" }.Validate(activity, Guid.Empty, 0, null));
        }

        [Fact]
        public void SignUpRequest_NoActivity_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SignUpRequest().Validate(null, Guid.NewGuid(), 0, null));
        }

        [Fact]
        public void SignOutRequest_ExternalWithoutEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SignOutRequest().Validate(TestData.CreateActivity(), Guid.Empty));
        }

        [Fact]
        public void SignOutRequest_DeadlinePassed_ThrowsArgumentException()
        {
            Activity activity = TestData.CreateActivity(x => x.SignOutDeadline = DateTime.UtcNow.AddMinutes(-1));

            Assert.Throws<ArgumentException>(() => new SignOutRequest().Validate(activity, Guid.NewGuid()));
        }

        [Fact]
        public void EditSignUpRequest_NoActivity_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new EditSignUpRequest().Validate(null, Guid.NewGuid()));
        }

        [Fact]
        public void EditSignUpRequest_BeforeDeadline_DoesNotThrow()
        {
            new EditSignUpRequest().Validate(TestData.CreateActivity(), Guid.NewGuid());
        }

        private static CreateUserRequest UserRequest(Action<CreateUserRequest>? configure = null)
        {
            CreateUserRequest request = new CreateUserRequest() { FullName = "Daan Bakker", Email = "daan@covadis.nl", JobTitle = "Directeur", Role = "User" };
            configure?.Invoke(request);
            return request;
        }

        [Fact]
        public void CreateUserRequest_Valid_DoesNotThrow()
        {
            UserRequest().Validate(null);
        }

        public static TheoryData<Action<CreateUserRequest>> InvalidUserRequests => new()
        {
            x => x.FullName = " ",
            x => x.Email = string.Empty,
            x => x.Email = "geen-email",
            x => x.JobTitle = "Astronaut",
            x => x.Role = "Koning"
        };

        [Theory]
        [MemberData(nameof(InvalidUserRequests))]
        public void CreateUserRequest_Invalid_ThrowsArgumentException(Action<CreateUserRequest> configure)
        {
            Assert.Throws<ArgumentException>(() => UserRequest(configure).Validate(null));
        }

        [Fact]
        public void CreateUserRequest_EmailAlreadyUsed_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => UserRequest().Validate(TestData.CreateUser()));
        }

        [Fact]
        public void UpdateUserRequest_KeepsOwnEmail_DoesNotThrow()
        {
            User user = TestData.CreateUser();
            UpdateUserRequest request = new UpdateUserRequest() { Id = user.Id, FullName = "Daan", Email = user.Email, JobTitle = "Directeur", Role = "User" };

            request.Validate(user, user, 2);
        }

        [Fact]
        public void UpdateUserRequest_LastAdminLosesRole_ThrowsArgumentException()
        {
            User user = TestData.CreateUser(x => x.Role = UserRole.Admin);
            UpdateUserRequest request = new UpdateUserRequest() { Id = user.Id, FullName = "Daan", Email = user.Email, JobTitle = "Directeur", Role = "User" };

            Assert.Throws<ArgumentException>(() => request.Validate(user, user, 1));
        }

        [Fact]
        public void UpdateUserStatusRequest_DeactivateLastAdmin_ThrowsArgumentException()
        {
            User user = TestData.CreateUser(x => x.Role = UserRole.Admin);

            Assert.Throws<ArgumentException>(() => new UpdateUserStatusRequest() { IsActive = false }.Validate(user, Guid.NewGuid(), 1));
        }

        [Fact]
        public void UpdateUserStatusRequest_ActivateOwnAccount_DoesNotThrow()
        {
            User user = TestData.CreateUser(x => x.IsActive = false);

            new UpdateUserStatusRequest() { IsActive = true }.Validate(user, user.Id, 1);
        }
    }
}
