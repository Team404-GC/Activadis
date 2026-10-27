using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;
using Activadis.Shared.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Activadis.UnitTests
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly TokenService _sut;

        public TokenServiceTests()
        {
            _sut = new TokenService(_configurationMock.Object);

            _configurationMock.Setup(c => c["JWT:Key"]).Returns("een-geheime-sleutel-van-minimaal-32-tekens!");
            _configurationMock.Setup(c => c["JWT:Issuer"]).Returns("Activadis");
            _configurationMock.Setup(c => c["JWT:ExpiryMinutes"]).Returns("60");
        }

        [Fact]
        public void GenerateToken_ValidConfiguration_ReturnsTokenWithUserClaims()
        {
            User user = TestData.CreateUser(x => { x.Slug = "daan-bakker"; x.Role = UserRole.Admin; });

            Token result = _sut.GenerateToken(user);

            JsonWebToken jwt = new JsonWebTokenHandler().ReadJsonWebToken(result.JWT);
            Assert.Equal("Activadis", jwt.Issuer);
            Assert.Contains(jwt.Claims, x => x.Value == user.Id.ToString());
            Assert.Contains(jwt.Claims, x => x.Type == "slug" && x.Value == "daan-bakker");
            Assert.Contains(jwt.Claims, x => x.Value == "Admin");
            Assert.InRange(result.ExpiresOn, DateTime.UtcNow.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
        }

        [Theory]
        [InlineData("JWT:Key", "")]
        [InlineData("JWT:Issuer", null)]
        [InlineData("JWT:ExpiryMinutes", " ")]
        [InlineData("JWT:ExpiryMinutes", "geen getal")]
        [InlineData("JWT:ExpiryMinutes", "0")]
        public void GenerateToken_InvalidConfiguration_ThrowsArgumentException(string key, string? value)
        {
            _configurationMock.Setup(c => c[key]).Returns(value);

            Assert.Throws<ArgumentException>(() => _sut.GenerateToken(TestData.CreateUser()));
        }

        [Fact]
        public void GeneratePasswordSetupToken_ReturnsUniqueHexTokens()
        {
            string first = _sut.GeneratePasswordSetupToken();
            string second = _sut.GeneratePasswordSetupToken();

            Assert.Equal(32, first.Length);
            Assert.Matches("^[0-9A-F]+$", first);
            Assert.NotEqual(first, second);
        }

        [Fact]
        public void HashPasswordSetupToken_SameToken_ReturnsSameHash()
        {
            string first = _sut.HashPasswordSetupToken("token");
            string second = _sut.HashPasswordSetupToken("token");

            Assert.Equal(first, second);
            Assert.Equal(64, first.Length);
            Assert.NotEqual(first, _sut.HashPasswordSetupToken("ander-token"));
        }
    }
}
