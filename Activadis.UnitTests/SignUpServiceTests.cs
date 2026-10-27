using Activadis.Application.Interfaces;
using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.SignUp;
using Moq;
using Xunit;

namespace Activadis.UnitTests
{
    public class SignUpServiceTests
    {
        private readonly Mock<IActivityRepository> _activityRepositoryMock = new();
        private readonly Mock<ISignUpRepository> _signUpRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IConfirmationService<SignOutRequest>> _signOutConfirmationMock = new();
        private readonly Mock<IConfirmationService<SignUpRequest>> _signUpConfirmationMock = new();
        private readonly Mock<IWaitlistService> _waitlistServiceMock = new();
        private readonly Mock<IConfirmationService<EditSignUpRequest>> _editSignUpConfirmationMock = new();
        private readonly SignUpService _sut;
        private readonly Activity _activity = TestData.CreateActivity();
        private readonly Guid _userId = Guid.NewGuid();

        public SignUpServiceTests()
        {
            _sut = new SignUpService(
                _activityRepositoryMock.Object,
                _signUpRepositoryMock.Object,
                _userRepositoryMock.Object,
                _signOutConfirmationMock.Object,
                _signUpConfirmationMock.Object,
                _waitlistServiceMock.Object,
                _editSignUpConfirmationMock.Object);

            _activityRepositoryMock.Setup(r => r.GetByIdAsync(_activity.Id)).ReturnsAsync(_activity);
        }

        [Fact]
        public async Task GetSignUpAsync_Employee_ReturnsSignUpOfUser()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id, x => { x.Comment = "Vegetarisch"; x.HasPlusOne = true; });
            _signUpRepositoryMock.Setup(r => r.GetByUserIdAndActivityIdAsync(_userId, _activity.Id)).ReturnsAsync(signUp);

            SignUpDTO result = await _sut.GetSignUpAsync(new GetSignUpRequest() { ActivityId = _activity.Id }, _userId);

            Assert.Equal("Vegetarisch", result.Comment);
            Assert.True(result.HasPlusOne);
        }

        [Fact]
        public async Task GetSignUpAsync_ExternalWithoutSignUp_ThrowsKeyNotFoundException()
        {
            _signUpRepositoryMock.Setup(r => r.GetByEmailAndActivityIdAsync("emma@extern.nl", _activity.Id)).ReturnsAsync((SignUp?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetSignUpAsync(new GetSignUpRequest() { Email = "emma@extern.nl", ActivityId = _activity.Id }, Guid.Empty));
        }

        [Fact]
        public async Task SignUpAsync_Employee_AddsSignUp()
        {
            SignUp? added = null;
            _signUpRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SignUp>()))
                .Callback<SignUp>(signUp => added = signUp)
                .ReturnsAsync((SignUp signUp) => signUp);

            await _sut.SignUpAsync(new SignUpRequest() { ActivityId = _activity.Id, HasPlusOne = true }, _userId);

            Assert.NotNull(added);
            Assert.Equal(_userId, added.UserId);
            Assert.False(added.IsExternal);
            Assert.True(added.HasPlusOne);
            _signUpConfirmationMock.Verify(c => c.SendConfirmationAsync(It.IsAny<SignUpRequest>()), Times.Never);
        }

        [Fact]
        public async Task SignUpAsync_External_SendsConfirmationInsteadOfSaving()
        {
            SignUpRequest request = new SignUpRequest() { ActivityId = _activity.Id, FullName = "Emma Smit", Email = "emma@extern.nl" };

            await _sut.SignUpAsync(request, Guid.Empty);

            _signUpConfirmationMock.Verify(c => c.SendConfirmationAsync(request), Times.Once);
            _signUpRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SignUp>()), Times.Never);
        }

        [Fact]
        public async Task SignUpAsync_ActivityFull_ThrowsArgumentException()
        {
            _signUpRepositoryMock.Setup(r => r.CountByActivityIdAsync(_activity.Id)).ReturnsAsync(_activity.MaxParticipants);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.SignUpAsync(new SignUpRequest() { ActivityId = _activity.Id }, _userId));
        }

        [Fact]
        public async Task SignUpAsync_AlreadySignedUp_ThrowsArgumentException()
        {
            _signUpRepositoryMock.Setup(r => r.GetByUserIdAndActivityIdIncludingDeletedAsync(_userId, _activity.Id)).ReturnsAsync(TestData.CreateSignUp(_activity.Id));

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.SignUpAsync(new SignUpRequest() { ActivityId = _activity.Id }, _userId));
        }

        [Fact]
        public async Task SignUpAsync_PreviouslySignedOut_RestoresSignUp()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id, x => x.DeletedAt = DateTime.UtcNow.AddDays(-1));
            _signUpRepositoryMock.Setup(r => r.GetByUserIdAndActivityIdIncludingDeletedAsync(_userId, _activity.Id)).ReturnsAsync(signUp);

            await _sut.SignUpAsync(new SignUpRequest() { ActivityId = _activity.Id, Comment = "Weer terug" }, _userId);

            Assert.Null(signUp.DeletedAt);
            Assert.Equal("Weer terug", signUp.Comment);
            _signUpRepositoryMock.Verify(r => r.UpdateAsync(signUp), Times.Once);
        }

        [Fact]
        public async Task EditSignUpAsync_Employee_UpdatesSignUp()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id);
            _signUpRepositoryMock.Setup(r => r.GetByUserIdAndActivityIdAsync(_userId, _activity.Id)).ReturnsAsync(signUp);

            await _sut.EditSignUpAsync(new EditSignUpRequest() { ActivityId = _activity.Id, Comment = "Later", HasPlusOne = true }, _userId);

            Assert.Equal("Later", signUp.Comment);
            Assert.True(signUp.HasPlusOne);
            _signUpRepositoryMock.Verify(r => r.UpdateAsync(signUp), Times.Once);
        }

        [Fact]
        public async Task EditSignUpAsync_External_SendsConfirmation()
        {
            EditSignUpRequest request = new EditSignUpRequest() { ActivityId = _activity.Id, Email = "emma@extern.nl" };

            await _sut.EditSignUpAsync(request, Guid.Empty);

            _editSignUpConfirmationMock.Verify(c => c.SendConfirmationAsync(request), Times.Once);
        }

        [Fact]
        public async Task EditSignUpAsync_OnWaitlist_ThrowsArgumentException()
        {
            _signUpRepositoryMock.Setup(r => r.GetByUserIdAndActivityIdAsync(_userId, _activity.Id)).ReturnsAsync(TestData.CreateSignUp(_activity.Id, x => x.WaitlistPosition = 1));

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.EditSignUpAsync(new EditSignUpRequest() { ActivityId = _activity.Id }, _userId));
        }

        [Fact]
        public async Task SignOutAsync_Employee_DeletesSignUpAndNotifiesWaitlist()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id);
            _signUpRepositoryMock.Setup(r => r.GetByUserIdAndActivityIdAsync(_userId, _activity.Id)).ReturnsAsync(signUp);

            await _sut.SignOutAsync(new SignOutRequest() { ActivityId = _activity.Id }, _userId);

            _signUpRepositoryMock.Verify(r => r.DeleteAsync(signUp), Times.Once);
            _waitlistServiceMock.Verify(w => w.NotifyWaitlistAsync(_activity.Id), Times.Once);
        }

        [Fact]
        public async Task SignOutAsync_EmployeeNotSignedUp_ThrowsArgumentException()
        {
            _signUpRepositoryMock.Setup(r => r.GetByUserIdAndActivityIdAsync(_userId, _activity.Id)).ReturnsAsync((SignUp?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.SignOutAsync(new SignOutRequest() { ActivityId = _activity.Id }, _userId));

            _waitlistServiceMock.Verify(w => w.NotifyWaitlistAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task SignOutAsync_External_SendsConfirmation()
        {
            SignOutRequest request = new SignOutRequest() { ActivityId = _activity.Id, Email = "emma@extern.nl" };

            await _sut.SignOutAsync(request, Guid.Empty);

            _signOutConfirmationMock.Verify(c => c.SendConfirmationAsync(request), Times.Once);
            _signUpRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<SignUp>()), Times.Never);
        }

        [Fact]
        public async Task SignOutAsync_DeadlinePassed_ThrowsArgumentException()
        {
            _activity.SignOutDeadline = DateTime.UtcNow.AddDays(-1);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.SignOutAsync(new SignOutRequest() { ActivityId = _activity.Id }, _userId));
        }
    }
}
