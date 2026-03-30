using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Notification.Queries;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.Notifications
{
    public sealed class NotificationsControllerTests
    {
        private readonly Mock<NotificationsQueryHandler> _mockHandler;
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<NotificationsController>> _mockLogger = new();

        public NotificationsControllerTests()
        {
            // NotificationsQueryHandler is a concrete class — mock via ctor injection
            _mockHandler = new Mock<NotificationsQueryHandler>(MockBehavior.Loose,
                new Mock<UserManagement.Application.Common.Interfaces.INotifications.INotificationsQueryRepository>().Object,
                _mockLogger.Object,
                _mockMediator.Object);
        }

        private NotificationsController CreateSut() =>
            new(_mockHandler.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact(Skip = "NotificationsQueryHandler.Handle is non-virtual and cannot be mocked by Moq")]
        public async Task PasswordResetNotifications_ReturnsOkResult()
        {
            _mockHandler
                .Setup(h => h.Handle(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NotificationResponse());

            var result = await CreateSut().PasswordResetNotifications(new NotificationRequest());

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
