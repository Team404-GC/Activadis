using Activadis.Application.Interfaces;
using Activadis.Application.Services.Confirmations;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.SignUp;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Activadis.UnitTests
{
    public class ConfirmationServiceTests
    {
        private readonly Mock<IActivityRepository> _activityRepositoryMock = new();
        private readonly Mock<ISignUpRepository> _signUpRepositoryMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<IWaitlistService> _waitlistServiceMock = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Activity _activity = TestData.CreateActivity();
        private string? _token;

        public ConfirmationServiceTests()
        {
            _scopeFactory = TestData.CreateScopeFactory(services =>
            {
                services.AddSingleton(_activityRepositoryMock.Object);
                services.AddSingleton(_signUpRepositoryMock.Object);
                services.AddSingleton(_emailServiceMock.Object);
                services.AddSingleton(_waitlistServiceMock.Object);
            });

            _activityRepositoryMock.Setup(r => r.GetByIdAsync(_activity.Id)).ReturnsAsync(_activity);
            _activityRepositoryMock.Setup(r => r.GetDetailAsync(_activity.Id.ToString())).ReturnsAsync(_activity);

            // The token is only sent by mail, so it is taken from the mail call.
            _emailServiceMock.Setup(e => e.SendSignUpConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((_, _, token) => _token = token)
                .ReturnsAsync(true);
            _emailServiceMock.Setup(e => e.SendSignOutConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((_, _, token) => _token = token)
                .ReturnsAsync(true);
            _emailServiceMock.Setup(e => e.SendRemoveWaitlistConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((_, _, token) => _token = token)
                .ReturnsAsync(true);
            _emailServiceMock.Setup(e => e.SendEditSignUpConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((_, _, token) => _token = token)
                .ReturnsAsync(true);
            _emailServiceMock.Setup(e => e.SendResignUpConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((_, _, token) => _token = token)
                .ReturnsAsync(true);
        }

        private SignUpRequest SignUpRequest()
            => new SignUpRequest() { ActivityId = _activity.Id, FullName = "Emma Smit", Email = "emma@extern.nl" };

        [Fact]
        public async Task SendConfirmationAsync_MailSent_StoresRequestUnderToken()
        {
            SignUpConfirmationService sut = new SignUpConfirmationService(_scopeFactory);
            SignUpRequest request = SignUpRequest();

            await sut.SendConfirmationAsync(request);

            Assert.NotNull(_token);
            Assert.Equal(request, sut.GetRequestByToken(_token));
        }

        [Fact]
        public async Task SendConfirmationAsync_MailFails_DoesNotStoreToken()
        {
            _emailServiceMock.Setup(e => e.SendSignUpConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((_, _, token) => _token = token)
                .ReturnsAsync(false);
            SignUpConfirmationService sut = new SignUpConfirmationService(_scopeFactory);

            await sut.SendConfirmationAsync(SignUpRequest());

            Assert.NotNull(_token);
            Assert.Null(sut.GetRequestByToken(_token));
        }

        [Fact]
        public async Task UseConfirmationAsync_UnknownToken_ThrowsArgumentException()
        {
            SignUpConfirmationService sut = new SignUpConfirmationService(_scopeFactory);

            await Assert.ThrowsAsync<ArgumentException>(() => sut.UseConfirmationAsync("onbekend"));
        }

        [Fact]
        public async Task UseConfirmationAsync_TokenUsedTwice_ThrowsArgumentExceptionTheSecondTime()
        {
            SignUpConfirmationService sut = new SignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(SignUpRequest());

            await sut.UseConfirmationAsync(_token!);

            await Assert.ThrowsAsync<ArgumentException>(() => sut.UseConfirmationAsync(_token!));
        }

        [Fact]
        public async Task SignUpConfirmation_PlaceLeft_AddsExternalSignUp()
        {
            SignUpConfirmationService sut = new SignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(SignUpRequest());
            SignUp? added = null;
            _signUpRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SignUp>()))
                .Callback<SignUp>(signUp => added = signUp)
                .ReturnsAsync((SignUp signUp) => signUp);

            await sut.UseConfirmationAsync(_token!);

            Assert.NotNull(added);
            Assert.True(added.IsExternal);
            Assert.Null(added.WaitlistPosition);
        }

        [Fact]
        public async Task SignUpConfirmation_ActivityFull_AddsToWaitlistAndThrowsArgumentException()
        {
            SignUpConfirmationService sut = new SignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(SignUpRequest());
            _signUpRepositoryMock.Setup(r => r.CountByActivityIdAsync(_activity.Id)).ReturnsAsync(_activity.MaxParticipants);

            await Assert.ThrowsAsync<ArgumentException>(() => sut.UseConfirmationAsync(_token!));

            _waitlistServiceMock.Verify(w => w.AddToWaitlistAsync(It.IsAny<SignUpRequest>(), null), Times.Once);
            _signUpRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SignUp>()), Times.Never);
        }

        [Fact]
        public async Task SignUpConfirmation_DeadlinePassed_ThrowsArgumentException()
        {
            SignUpConfirmationService sut = new SignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(SignUpRequest());
            _activity.SignUpDeadline = DateTime.UtcNow.AddDays(-1);

            await Assert.ThrowsAsync<ArgumentException>(() => sut.UseConfirmationAsync(_token!));
        }

        [Fact]
        public async Task SignOutConfirmation_Participant_SendsSignOutMailAndDeletesSignUp()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id);
            _signUpRepositoryMock.Setup(r => r.GetByEmailAndActivityIdAsync("emma@extern.nl", _activity.Id)).ReturnsAsync(signUp);
            SignOutConfirmationService sut = new SignOutConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(new SignOutRequest() { ActivityId = _activity.Id, Email = "emma@extern.nl" });

            await sut.UseConfirmationAsync(_token!);

            _emailServiceMock.Verify(e => e.SendSignOutConfirmationAsync("emma@extern.nl", "Emma Smit", It.IsAny<string>()), Times.Once);
            _signUpRepositoryMock.Verify(r => r.DeleteAsync(signUp), Times.Once);
        }

        [Fact]
        public async Task SignOutConfirmation_OnWaitlist_SendsWaitlistMailAndAdvancesWaitlist()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id, x => x.WaitlistPosition = 2);
            _signUpRepositoryMock.Setup(r => r.GetByEmailAndActivityIdAsync("emma@extern.nl", _activity.Id)).ReturnsAsync(signUp);
            SignOutConfirmationService sut = new SignOutConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(new SignOutRequest() { ActivityId = _activity.Id, Email = "emma@extern.nl" });

            await sut.UseConfirmationAsync(_token!);

            _emailServiceMock.Verify(e => e.SendRemoveWaitlistConfirmationAsync("emma@extern.nl", "Emma Smit", It.IsAny<string>()), Times.Once);
            _signUpRepositoryMock.Verify(r => r.DeleteAsync(signUp), Times.Once);
            _signUpRepositoryMock.Verify(r => r.AdvanceWaitlistAsync(2, _activity.Id), Times.Once);
        }

        [Fact]
        public async Task SignOutConfirmation_NotSignedUp_DoesNotStoreToken()
        {
            _signUpRepositoryMock.Setup(r => r.GetByEmailAndActivityIdAsync(It.IsAny<string>(), _activity.Id)).ReturnsAsync((SignUp?)null);
            SignOutConfirmationService sut = new SignOutConfirmationService(_scopeFactory);

            await sut.SendConfirmationAsync(new SignOutRequest() { ActivityId = _activity.Id, Email = "emma@extern.nl" });

            Assert.Null(_token);
            _emailServiceMock.Verify(e => e.SendSignOutConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditSignUpConfirmation_ValidToken_UpdatesSignUp()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id);
            _signUpRepositoryMock.Setup(r => r.GetFullNameByEmailAndActivityIdAsync("emma@extern.nl", _activity.Id)).ReturnsAsync("Emma Smit");
            _signUpRepositoryMock.Setup(r => r.GetByEmailAndActivityIdAsync("emma@extern.nl", _activity.Id)).ReturnsAsync(signUp);
            EditSignUpConfirmationService sut = new EditSignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(new EditSignUpRequest() { ActivityId = _activity.Id, Email = "emma@extern.nl", Comment = "Iets later", HasPlusOne = true });

            await sut.UseConfirmationAsync(_token!);

            Assert.Equal("Iets later", signUp.Comment);
            Assert.True(signUp.HasPlusOne);
            _signUpRepositoryMock.Verify(r => r.UpdateAsync(signUp), Times.Once);
        }

        [Fact]
        public async Task EditSignUpConfirmation_DeadlinePassed_ThrowsArgumentException()
        {
            _signUpRepositoryMock.Setup(r => r.GetFullNameByEmailAndActivityIdAsync("emma@extern.nl", _activity.Id)).ReturnsAsync("Emma Smit");
            EditSignUpConfirmationService sut = new EditSignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(new EditSignUpRequest() { ActivityId = _activity.Id, Email = "emma@extern.nl" });
            _activity.SignOutDeadline = DateTime.UtcNow.AddDays(-1);

            await Assert.ThrowsAsync<ArgumentException>(() => sut.UseConfirmationAsync(_token!));

            _signUpRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SignUp>()), Times.Never);
        }

        [Fact]
        public async Task ResignUpConfirmation_PlaceLeft_MovesSignUpFromWaitlist()
        {
            SignUp signUp = TestData.CreateSignUp(_activity.Id, x => x.WaitlistPosition = 1);
            _signUpRepositoryMock.Setup(r => r.GetByEmailAndActivityIdAsync("emma@extern.nl", _activity.Id)).ReturnsAsync(signUp);
            ResignUpConfirmationService sut = new ResignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(new ResignUpRequest() { ActivityId = _activity.Id, FullName = "Emma Smit", Email = "emma@extern.nl", HasPlusOne = true });

            await sut.UseConfirmationAsync(_token!);

            Assert.Null(signUp.WaitlistPosition);
            Assert.True(signUp.HasPlusOne);
            _signUpRepositoryMock.Verify(r => r.AdvanceWaitlistAsync(1, _activity.Id), Times.Once);
        }

        [Fact]
        public async Task ResignUpConfirmation_ActivityFull_ThrowsArgumentException()
        {
            _activity.MaxParticipants = 1;
            _activity.SignUps = [TestData.CreateSignUp(_activity.Id)];
            ResignUpConfirmationService sut = new ResignUpConfirmationService(_scopeFactory);
            await sut.SendConfirmationAsync(new ResignUpRequest() { ActivityId = _activity.Id, FullName = "Emma Smit", Email = "emma@extern.nl" });

            await Assert.ThrowsAsync<ArgumentException>(() => sut.UseConfirmationAsync(_token!));

            _signUpRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SignUp>()), Times.Never);
        }
    }
}
