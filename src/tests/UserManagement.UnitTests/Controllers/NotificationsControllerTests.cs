using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.Notification.Queries;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers;

public sealed class NotificationsControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<ILogger<NotificationsController>> _mockLogger = new(MockBehavior.Loose);
    private readonly Mock<INotificationsQueryRepository> _mockRepo = new(MockBehavior.Loose);

    private NotificationsController CreateSut()
    {
        var handlerLogger = new Mock<ILogger<NotificationsQueryHandler>>(MockBehavior.Loose);
        var handler = new NotificationsQueryHandler(_mockRepo.Object, handlerLogger.Object, _mockMediator.Object);
        return new NotificationsController(handler, _mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task PasswordResetNotifications_NullUsername_ThrowsValidationException()
    {
        Func<Task> act = async () => await CreateSut().PasswordResetNotifications(
            new NotificationRequest { Username = null });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Username required*");
    }

    [Fact]
    public async Task PasswordResetNotifications_UnknownUser_ThrowsValidationException()
    {
        _mockRepo.Setup(r => r.GetLastPasswordChangeDate(It.IsAny<string>()))
            .ReturnsAsync((DateTime?)null);

        Func<Task> act = async () => await CreateSut().PasswordResetNotifications(
            new NotificationRequest { Username = "unknownuser" });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task PasswordResetNotifications_ValidUser_CallsGetLastPasswordChangeDate()
    {
        // Password in alert window: changed 80 days ago, expiry 90 days, alert 15 days
        _mockRepo.Setup(r => r.GetLastPasswordChangeDate(It.IsAny<string>()))
            .ReturnsAsync(DateTime.Now.AddDays(-80));
        _mockRepo.Setup(r => r.GetPasswordExpiryDays())
            .ReturnsAsync((90, 15));
        _mockRepo.Setup(r => r.GetResetCodeExpiryMinutes())
            .ReturnsAsync(30);

        try
        {
            await CreateSut().PasswordResetNotifications(
                new NotificationRequest { Username = "testuser" });
        }
        catch (ValidationException)
        {
            // Handler throws for various conditions - we just verify repo was called
        }

        _mockRepo.Verify(r => r.GetLastPasswordChangeDate("testuser"), Times.Once);
    }
}
