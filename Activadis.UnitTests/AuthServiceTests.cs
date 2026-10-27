using Activadis.Application.Interfaces;
using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Helpers;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.Auth;
using Moq;
using System.Security.Authentication;
using Xunit;

namespace Activadis.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IPassword> _passwordMock = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _sut = new AuthService(_userRepositoryMock.Object, _tokenServiceMock.Object, _passwordMock.Object);

            _tokenServiceMock.Setup(t => t.HashPasswordSetupToken("token")).Returns("token-hash");
        }

        private static LoginRequest LoginRequest()
            => new LoginRequest() { Email = "daan@covadis.nl", Password = "Geheim1!" };

        private static SetPasswordRequest SetPasswordRequest()
            => new SetPasswordRequest() { Token = "token", Password = "Geheim1!", ConfirmPassword = "Geheim1!" };

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            User user = TestData.CreateUser();
            Token token = new Token() { JWT = "jwt" };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _passwordMock.Setup(p => p.Validate(user.HashedPassword, "Geheim1!")).Returns(true);
            _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns(token);

            Token result = await _sut.LoginAsync(LoginRequest());

            Assert.Equal(token, result);
        }

        [Fact]
        public async Task LoginAsync_UnknownEmail_ThrowsAuthenticationException()
        {
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<AuthenticationException>(() => _sut.LoginAsync(LoginRequest()));
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ThrowsAuthenticationException()
        {
            User user = TestData.CreateUser(x => x.IsActive = false);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _passwordMock.Setup(p => p.Validate(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            await Assert.ThrowsAsync<AuthenticationException>(() => _sut.LoginAsync(LoginRequest()));
        }

        [Fact]
        public async Task LoginAsync_NoPasswordSetYet_ThrowsAuthenticationException()
        {
            User user = TestData.CreateUser(x => x.HashedPassword = string.Empty);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _passwordMock.Setup(p => p.Validate(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            await Assert.ThrowsAsync<AuthenticationException>(() => _sut.LoginAsync(LoginRequest()));
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsAuthenticationException()
        {
            User user = TestData.CreateUser();
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _passwordMock.Setup(p => p.Validate(user.HashedPassword, "Geheim1!")).Returns(false);

            await Assert.ThrowsAsync<AuthenticationException>(() => _sut.LoginAsync(LoginRequest()));

            _tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task IsActiveAsync_ExistingUser_ReturnsIsActive(bool isActive, bool expected)
        {
            User user = TestData.CreateUser(x => x.IsActive = isActive);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            bool result = await _sut.IsActiveAsync(user.Id);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task IsActiveAsync_UserNotFound_ReturnsFalse()
        {
            Guid userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            bool result = await _sut.IsActiveAsync(userId);

            Assert.False(result);
        }

        [Fact]
        public async Task CheckPasswordSetupAsync_UnknownToken_ThrowsArgumentException()
        {
            _userRepositoryMock.Setup(r => r.GetByPasswordSetupTokenHashAsync("token-hash")).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CheckPasswordSetupAsync("token"));
        }

        [Fact]
        public async Task CheckPasswordSetupAsync_ExpiredToken_ThrowsArgumentException()
        {
            User user = TestData.CreateUser(x => x.PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1));
            _userRepositoryMock.Setup(r => r.GetByPasswordSetupTokenHashAsync("token-hash")).ReturnsAsync(user);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CheckPasswordSetupAsync("token"));
        }

        [Fact]
        public async Task CheckPasswordSetupAsync_ValidToken_DoesNotThrow()
        {
            User user = TestData.CreateUser(x => x.PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(10));
            _userRepositoryMock.Setup(r => r.GetByPasswordSetupTokenHashAsync("token-hash")).ReturnsAsync(user);

            await _sut.CheckPasswordSetupAsync("token");
        }

        [Fact]
        public async Task SetPasswordAsync_ValidRequest_SavesHashedPasswordAndClearsToken()
        {
            User user = TestData.CreateUser(x =>
            {
                x.HashedPassword = string.Empty;
                x.PasswordSetupTokenHash = "token-hash";
                x.PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);
            });
            _userRepositoryMock.Setup(r => r.GetByPasswordSetupTokenHashAsync("token-hash")).ReturnsAsync(user);
            _passwordMock.Setup(p => p.Hash("Geheim1!")).Returns("nieuwe-hash");

            await _sut.SetPasswordAsync(SetPasswordRequest());

            Assert.Equal("nieuwe-hash", user.HashedPassword);
            Assert.Null(user.PasswordSetupTokenHash);
            Assert.Null(user.PasswordSetupTokenExpiresAt);
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task SetPasswordAsync_PasswordsDoNotMatch_ThrowsArgumentException()
        {
            User user = TestData.CreateUser(x => x.PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(10));
            _userRepositoryMock.Setup(r => r.GetByPasswordSetupTokenHashAsync("token-hash")).ReturnsAsync(user);
            SetPasswordRequest request = SetPasswordRequest();
            request.ConfirmPassword = "Anders1!";

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.SetPasswordAsync(request));

            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
