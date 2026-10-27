using Activadis.Application.Interfaces;
using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.SignUp;
using Moq;
using Xunit;

namespace Activadis.UnitTests
{
    public class WaitlistServiceTests
    {
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<ISignUpRepository> _signUpRepositoryMock = new();
        private readonly Mock<IConfirmationService<ResignUpRequest>> _resignUpConfirmationMock = new();
        private readonly WaitlistService _sut;
        private readonly Guid _activityId = Guid.NewGuid();

        public WaitlistServiceTests()
        {
            _sut = new WaitlistService(_emailServiceMock.Object, _signUpRepositoryMock.Object, _resignUpConfirmationMock.Object);
        }

        private SignUpRequest SignUpRequest()
            => new SignUpRequest() { ActivityId = _activityId, FullName = "Emma Smit", Email = "emma@extern.nl" };

        [Fact]
        public async Task AddToWaitlistAsync_MailSent_AddsSignUpAtNextPosition()
        {
            _signUpRepositoryMock.Setup(r => r.GetLastPositionAsync(_activityId)).ReturnsAsync(2);
            _emailServiceMock.Setup(e => e.SendAddWaitlistConfirmationAsync("emma@extern.nl", "Emma Smit", 3)).ReturnsAsync(true);
            SignUp? added = null;
            _signUpRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SignUp>()))
                .Callback<SignUp>(signUp => added = signUp)
                .ReturnsAsync((SignUp signUp) => signUp);

            await _sut.AddToWaitlistAsync(SignUpRequest(), null);

            Assert.NotNull(added);
            Assert.Equal(3, added.WaitlistPosition);
            Assert.True(added.IsExternal);
        }

        [Fact]
        public async Task AddToWaitlistAsync_MailFails_ThrowsArgumentException()
        {
            _emailServiceMock.Setup(e => e.SendAddWaitlistConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddToWaitlistAsync(SignUpRequest(), null));

            _signUpRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SignUp>()), Times.Never);
        }

        [Fact]
        public async Task NotifyWaitlistAsync_FirstOnWaitlist_SendsResignUpConfirmation()
        {
            SignUp signUp = TestData.CreateSignUp(_activityId, x => { x.WaitlistPosition = 1; x.HasPlusOne = true; });
            _signUpRepositoryMock.Setup(r => r.GetFirstPositionAsync(_activityId)).ReturnsAsync(signUp);
            ResignUpRequest? sent = null;
            _resignUpConfirmationMock.Setup(c => c.SendConfirmationAsync(It.IsAny<ResignUpRequest>()))
                .Callback<ResignUpRequest>(request => sent = request);

            await _sut.NotifyWaitlistAsync(_activityId);

            Assert.NotNull(sent);
            Assert.Equal("emma@extern.nl", sent.Email);
            Assert.True(sent.HasPlusOne);
            Assert.Equal(_activityId, sent.ActivityId);
        }

        [Fact]
        public async Task NotifyWaitlistAsync_EmptyWaitlist_SendsNothing()
        {
            _signUpRepositoryMock.Setup(r => r.GetFirstPositionAsync(_activityId)).ReturnsAsync((SignUp?)null);

            await _sut.NotifyWaitlistAsync(_activityId);

            _resignUpConfirmationMock.Verify(c => c.SendConfirmationAsync(It.IsAny<ResignUpRequest>()), Times.Never);
        }
    }
}
