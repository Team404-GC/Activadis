using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Helpers;
using Activadis.Domain.Models;
using Moq;
using Xunit;

namespace Activadis.UnitTests
{
    public class EmailServiceTests
    {
        private readonly Mock<IEmailSender> _emailSenderMock = new();
        private readonly EmailService _sut;
        private EmailMessage? _sent;

        public EmailServiceTests()
        {
            _sut = new EmailService(_emailSenderMock.Object);

            _emailSenderMock.Setup(s => s.SendAsync(It.IsAny<EmailMessage>()))
                .Callback<EmailMessage>(message => _sent = message)
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task SendSignUpConfirmationAsync_SendsMailWithTokenToReceiver()
        {
            bool result = await _sut.SendSignUpConfirmationAsync("emma@extern.nl", "Emma Smit", "abc123");

            Assert.True(result);
            Assert.NotNull(_sent);
            Assert.Equal("emma@extern.nl", _sent.ToEmail);
            Assert.Equal("Emma Smit", _sent.ToName);
            Assert.Contains("abc123", _sent.HtmlBody);
            Assert.Contains("abc123", _sent.TextBody);
        }

        [Fact]
        public async Task SendAddWaitlistConfirmationAsync_MailContainsPosition()
        {
            await _sut.SendAddWaitlistConfirmationAsync("emma@extern.nl", "Emma Smit", 3);

            Assert.NotNull(_sent);
            Assert.Contains("3", _sent.TextBody);
        }

        [Fact]
        public async Task SendActivityCancelledAsync_MailContainsActivityName()
        {
            Activity activity = TestData.CreateActivity();

            await _sut.SendActivityCancelledAsync("emma@extern.nl", "Emma Smit", activity);

            Assert.NotNull(_sent);
            Assert.Contains("Bowlingavond", _sent.HtmlBody);
        }

        [Fact]
        public async Task SendActivityChangedAsync_MailContainsActivityName()
        {
            Activity activity = TestData.CreateActivity();

            await _sut.SendActivityChangedAsync("emma@extern.nl", "Emma Smit", activity);

            Assert.NotNull(_sent);
            Assert.Contains("Bowlingavond", _sent.TextBody);
        }

        [Fact]
        public async Task SendTestAsync_SendsPasswordSetupMail()
        {
            await _sut.SendTestAsync("daan@covadis.nl", "Daan Bakker");

            Assert.NotNull(_sent);
            Assert.Equal("daan@covadis.nl", _sent.ToEmail);
            Assert.Contains("voorbeeld=true", _sent.HtmlBody);
        }

        [Fact]
        public async Task SendSignOutConfirmationAsync_SenderFails_ReturnsFalse()
        {
            _emailSenderMock.Setup(s => s.SendAsync(It.IsAny<EmailMessage>())).ReturnsAsync(false);

            bool result = await _sut.SendSignOutConfirmationAsync("emma@extern.nl", "Emma Smit", "abc123");

            Assert.False(result);
        }
    }
}
