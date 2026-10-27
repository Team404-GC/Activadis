using Activadis.Application.DTOs;
using Activadis.Application.Extensions;
using Activadis.Domain.Entities;
using Activadis.Domain.Filters;
using Activadis.Shared.DTOs.Activity;
using Activadis.Shared.DTOs.SignUp;
using Xunit;

namespace Activadis.UnitTests
{
    public class ExtensionTests
    {
        [Theory]
        [InlineData(0, 5, false)]
        [InlineData(3, 2, false)]
        [InlineData(3, 3, true)]
        public void IsFull_ReturnsWhetherAllSpotsAreTaken(int maxParticipants, int signUps, bool expected)
        {
            Activity activity = TestData.CreateActivity(x =>
            {
                x.MaxParticipants = maxParticipants;
                x.SignUps = Enumerable.Range(0, signUps).Select(_ => TestData.CreateSignUp(x.Id)).ToList();
            });

            Assert.Equal(expected, activity.IsFull());
        }

        [Fact]
        public void AvailableSpots_ReturnsMaxMinusSignUps()
        {
            Activity activity = TestData.CreateActivity(x => x.SignUps = [TestData.CreateSignUp(x.Id)]);

            Assert.Equal(9, activity.AvailableSpots());
        }

        [Fact]
        public void IsOpenForSignUp_UpcomingActivityWithSpots_ReturnsTrue()
        {
            Assert.True(TestData.CreateActivity().IsOpenForSignUp());
        }

        [Fact]
        public void IsOpenForSignUp_DeadlinePassed_ReturnsFalse()
        {
            Activity activity = TestData.CreateActivity(x => x.SignUpDeadline = DateTime.UtcNow.AddMinutes(-1));

            Assert.False(activity.IsOpenForSignUp());
        }

        [Fact]
        public void HasTakenPlace_EndDateInPast_ReturnsTrue()
        {
            Activity activity = TestData.CreateActivity(x => x.EndDate = DateTime.UtcNow.AddMinutes(-1));

            Assert.True(activity.HasTakenPlace());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void ToFilter_IncludePastOnlyForAdmin(bool isAdmin, bool expected)
        {
            ActivityFilter result = new ActivityFilterRequest() { IncludePast = true }.ToFilter(isAdmin);

            Assert.Equal(expected, result.IncludePast);
        }

        [Fact]
        public void ToFilter_TrimsNameAndConvertsTime()
        {
            ActivityFilterRequest request = new ActivityFilterRequest() { Name = "  bowlen ", TimeFrom = new TimeOnly(18, 30), TimeTo = new TimeOnly(21, 0), UtcOffsetMinutes = 60, SortDescending = true };

            ActivityFilter result = request.ToFilter(false);

            Assert.Equal("bowlen", result.Name);
            Assert.Equal(new TimeSpan(18, 30, 0), result.TimeFrom);
            Assert.Equal(new TimeSpan(21, 0, 0), result.TimeTo);
            Assert.Equal(60, result.UtcOffsetMinutes);
            Assert.True(result.SortDescending);
        }

        [Fact]
        public void ToFilter_EmptyName_ReturnsNull()
        {
            Assert.Null(new ActivityFilterRequest() { Name = " " }.ToFilter(false).Name);
        }

        [Theory]
        [InlineData(2026, 1, 15, 12, "15 januari 2026, 13:00")]
        [InlineData(2026, 7, 1, 8, "1 juli 2026, 10:00")]
        public void ToDisplayDateTime_ShowsDutchTimeAndMonth(int year, int month, int day, int hour, string expected)
        {
            DateTime date = new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc);

            Assert.Equal(expected, date.ToDisplayDateTime());
        }

        [Fact]
        public void ToDisplayPeriod_SameDay_ShowsEndTimeOnly()
        {
            DateTime start = new DateTime(2026, 1, 15, 17, 0, 0, DateTimeKind.Utc);

            Assert.Equal("15 januari 2026, 18:00 - 21:00", DateExtensions.ToDisplayPeriod(start, start.AddHours(3)));
        }

        [Fact]
        public void ToDisplayPeriod_OtherDay_ShowsBothDates()
        {
            DateTime start = new DateTime(2026, 1, 15, 17, 0, 0, DateTimeKind.Utc);

            Assert.Equal("15 januari 2026, 18:00 - 16 januari 2026, 18:00", DateExtensions.ToDisplayPeriod(start, start.AddDays(1)));
        }

        [Fact]
        public void ToSignUp_External_HasNoUser()
        {
            SignUpRequest request = new SignUpRequest() { FullName = "Emma Smit", Email = "emma@extern.nl", ActivityId = Guid.NewGuid() };

            SignUp result = request.ToSignUp(Guid.Empty, 2);

            Assert.True(result.IsExternal);
            Assert.Null(result.UserId);
            Assert.Equal(2, result.WaitlistPosition);
            Assert.Equal(request.ActivityId, result.ActivityId);
        }

        [Fact]
        public void ToSignUp_Employee_HasUser()
        {
            Guid userId = Guid.NewGuid();

            SignUp result = new SignUpRequest().ToSignUp(userId);

            Assert.False(result.IsExternal);
            Assert.Equal(userId, result.UserId);
        }

        [Theory]
        [InlineData("hash", true)]
        [InlineData("", false)]
        public void HasPassword_ReturnsWhetherPasswordIsSet(string hashedPassword, bool expected)
        {
            User user = TestData.CreateUser(x => x.HashedPassword = hashedPassword);

            Assert.Equal(expected, user.HasPassword());
        }

        [Fact]
        public void PasswordSetupExpired_NoExpiry_ReturnsTrue()
        {
            Assert.True(TestData.CreateUser().PasswordSetupExpired());
        }

        [Fact]
        public void PasswordSetupExpired_ExpiryInFuture_ReturnsFalse()
        {
            User user = TestData.CreateUser(x => x.PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(5));

            Assert.False(user.PasswordSetupExpired());
        }

        [Fact]
        public void ConfirmationHasExpired_ExpiresInPast_ReturnsTrue()
        {
            ConfirmationDTO<string> confirmation = new ConfirmationDTO<string>("verzoek", DateTime.UtcNow.AddSeconds(-1));

            Assert.True(confirmation.HasExpired());
        }

        [Fact]
        public void ConfirmationHasExpired_ExpiresInFuture_ReturnsFalse()
        {
            ConfirmationDTO<string> confirmation = new ConfirmationDTO<string>("verzoek", DateTime.UtcNow.AddMinutes(5));

            Assert.False(confirmation.HasExpired());
        }

        [Fact]
        public void ToDetailResponse_WithoutElo_LeavesCategoriesEmpty()
        {
            Activity activity = TestData.CreateActivity(x => x.Categories = [new Category() { Id = Guid.NewGuid(), Name = "Darts" }]);

            ActivityDetailResponse result = activity.ToDetailResponse(true, false, []);

            Assert.True(result.HasSignedUp);
            Assert.Empty(result.Categories);
        }
    }
}
