using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.Notification.Queries;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Notification.Queries
{
    public sealed class NotificationsQueryHandlerTests
    {
        private readonly Mock<INotificationsQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<NotificationsQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private NotificationsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockLogger.Object, _mockMediator.Object);

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_EmptyOrNullUsername_ThrowsValidationException(string? username)
        {
            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(
                new NotificationRequest { Username = username },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Username required*");
        }

        [Fact]
        public async Task Handle_LastPasswordChangeDateNull_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetLastPasswordChangeDate("john"))
                .ReturnsAsync((DateTime?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new NotificationRequest { Username = "john" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Password last change date not found*");
        }

        [Fact]
        public async Task Handle_PasswordExpired_ThrowsValidationException()
        {
            // Password is 100 days old, expiry is 90 days
            _mockQueryRepo
                .Setup(r => r.GetLastPasswordChangeDate("john"))
                .ReturnsAsync(DateTime.Now.AddDays(-100));
            _mockQueryRepo
                .Setup(r => r.GetPasswordExpiryDays())
                .ReturnsAsync((90, 7));

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new NotificationRequest { Username = "john" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Your password has expired*");
        }

        [Fact]
        public async Task Handle_PasswordNearExpiry_ThrowsValidationExceptionWithDaysLeft()
        {
            // Password is 85 days old, expiry 90, alert window 7 → 5 days left, falls in alert window
            _mockQueryRepo
                .Setup(r => r.GetLastPasswordChangeDate("john"))
                .ReturnsAsync(DateTime.Now.AddDays(-85));
            _mockQueryRepo
                .Setup(r => r.GetPasswordExpiryDays())
                .ReturnsAsync((90, 7));

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new NotificationRequest { Username = "john" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*will expire*");
        }

        [Fact]
        public async Task Handle_PasswordStillValid_ThrowsValidationException()
        {
            // Password is 10 days old, expiry 90, alert 7 → still valid
            _mockQueryRepo
                .Setup(r => r.GetLastPasswordChangeDate("john"))
                .ReturnsAsync(DateTime.Now.AddDays(-10));
            _mockQueryRepo
                .Setup(r => r.GetPasswordExpiryDays())
                .ReturnsAsync((90, 7));

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new NotificationRequest { Username = "john" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*still valid*");
        }

        [Fact]
        public async Task Handle_ValidUsername_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetLastPasswordChangeDate("john"))
                .ReturnsAsync(DateTime.Now.AddDays(-10));
            _mockQueryRepo
                .Setup(r => r.GetPasswordExpiryDays())
                .ReturnsAsync((90, 7));

            var sut = CreateSut();
            try
            {
                await sut.Handle(new NotificationRequest { Username = "john" }, CancellationToken.None);
            }
            catch (ValidationException)
            {
                // expected — handler always throws
            }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
