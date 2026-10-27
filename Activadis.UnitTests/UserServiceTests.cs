using Activadis.Application.Interfaces;
using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.User;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Activadis.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<ISignUpRepository> _signUpRepositoryMock = new();
        private readonly Mock<IWaitlistService> _waitlistServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _sut = new UserService(
                _userRepositoryMock.Object,
                _signUpRepositoryMock.Object,
                _waitlistServiceMock.Object,
                _tokenServiceMock.Object,
                _emailServiceMock.Object,
                _configurationMock.Object);

            _tokenServiceMock.Setup(t => t.GeneratePasswordSetupToken()).Returns("token");
            _tokenServiceMock.Setup(t => t.HashPasswordSetupToken("token")).Returns("token-hash");
            _userRepositoryMock.Setup(r => r.CountActiveByRoleAsync(UserRole.Admin)).ReturnsAsync(2);
        }

        private static CreateUserRequest CreateRequest()
            => new CreateUserRequest() { FullName = " Daan Bakker ", Email = " daan@covadis.nl ", JobTitle = "Directeur", Role = "User" };

        private static UpdateUserRequest UpdateRequest(User user)
            => new UpdateUserRequest() { Id = user.Id, FullName = "Daan de Bakker", Email = "daan.bakker@covadis.nl", JobTitle = "Testengineer", Role = "Admin" };

        [Fact]
        public async Task CreateAsync_ValidRequest_SendsInvitationAndAddsUser()
        {
            _emailServiceMock.Setup(e => e.SendPasswordSetupAsync("daan@covadis.nl", "Daan Bakker", "token")).ReturnsAsync(true);
            User? added = null;
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => added = user)
                .ReturnsAsync((User user) => user);

            await _sut.CreateAsync(CreateRequest());

            Assert.NotNull(added);
            Assert.Equal("Daan Bakker", added.FullName);
            Assert.Equal("daan@covadis.nl", added.Email);
            Assert.Equal("token-hash", added.PasswordSetupTokenHash);
            Assert.True(added.PasswordSetupTokenExpiresAt > DateTime.UtcNow);
            Assert.Equal(string.Empty, added.HashedPassword);
        }

        [Fact]
        public async Task CreateAsync_EmailNotSent_ThrowsArgumentExceptionAndDoesNotAddUser()
        {
            _emailServiceMock.Setup(e => e.SendPasswordSetupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(CreateRequest()));

            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_EmailAlreadyUsed_ThrowsArgumentException()
        {
            _userRepositoryMock.Setup(r => r.GetByEmailAsync("daan@covadis.nl")).ReturnsAsync(TestData.CreateUser());

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(CreateRequest()));

            _emailServiceMock.Verify(e => e.SendPasswordSetupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsUsersOrderedByName()
        {
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(
            [
                TestData.CreateUser(x => x.FullName = "Thijs"),
                TestData.CreateUser(x => x.FullName = "Anna")
            ]);

            IEnumerable<UserResponse> result = await _sut.GetAllAsync();

            Assert.Equal(["Anna", "Thijs"], result.Select(x => x.FullName));
        }

        [Fact]
        public async Task GetByIdentifierAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            _userRepositoryMock.Setup(r => r.GetByIdentifierAsync("onbekend")).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdentifierAsync("onbekend"));
        }

        [Fact]
        public async Task GetByIdentifierAsync_UserExists_ReturnsUser()
        {
            User user = TestData.CreateUser(x => x.Role = UserRole.Admin);
            _userRepositoryMock.Setup(r => r.GetByIdentifierAsync("daan-bakker")).ReturnsAsync(user);

            UserResponse result = await _sut.GetByIdentifierAsync("daan-bakker");

            Assert.Equal(user.Id, result.Id);
            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public async Task UpdateAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            User user = TestData.CreateUser();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(UpdateRequest(user)));
        }

        [Fact]
        public async Task UpdateAsync_ValidRequest_UpdatesUser()
        {
            User user = TestData.CreateUser();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            await _sut.UpdateAsync(UpdateRequest(user));

            Assert.Equal("Daan de Bakker", user.FullName);
            Assert.Equal("daan.bakker@covadis.nl", user.Email);
            Assert.Equal("Testengineer", user.JobTitle);
            Assert.Equal(UserRole.Admin, user.Role);
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_EmailOfOtherUser_ThrowsArgumentException()
        {
            User user = TestData.CreateUser();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync("daan.bakker@covadis.nl")).ReturnsAsync(TestData.CreateUser());

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(UpdateRequest(user)));

            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            Guid userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateStatusAsync(userId, Guid.NewGuid(), new UpdateUserStatusRequest()));
        }

        [Fact]
        public async Task UpdateStatusAsync_Activate_DoesNotTouchSignUps()
        {
            User user = TestData.CreateUser(x => x.IsActive = false);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            await _sut.UpdateStatusAsync(user.Id, Guid.NewGuid(), new UpdateUserStatusRequest() { IsActive = true });

            Assert.True(user.IsActive);
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _signUpRepositoryMock.Verify(r => r.GetUpcomingByUserIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_Deactivate_DeletesUpcomingSignUpsAndNotifiesWaitlist()
        {
            User user = TestData.CreateUser();
            SignUp first = TestData.CreateSignUp(Guid.NewGuid());
            SignUp second = TestData.CreateSignUp(Guid.NewGuid());
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _signUpRepositoryMock.Setup(r => r.GetUpcomingByUserIdAsync(user.Id)).ReturnsAsync([first, second]);

            await _sut.UpdateStatusAsync(user.Id, Guid.NewGuid(), new UpdateUserStatusRequest() { IsActive = false });

            Assert.False(user.IsActive);
            _signUpRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<SignUp>()), Times.Exactly(2));
            _waitlistServiceMock.Verify(w => w.NotifyWaitlistAsync(first.ActivityId), Times.Once);
            _waitlistServiceMock.Verify(w => w.NotifyWaitlistAsync(second.ActivityId), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_DeactivateOwnAccount_ThrowsArgumentException()
        {
            User user = TestData.CreateUser();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateStatusAsync(user.Id, user.Id, new UpdateUserStatusRequest() { IsActive = false }));

            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
