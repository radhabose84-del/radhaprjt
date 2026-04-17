using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.GetNotificationDetail.UpdateNotificationStatus;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification
{
    public sealed class UpdateNotificationStatusHandlerTests
    {
        private readonly Mock<INotificationDetailRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateNotificationStatusHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsResult_WhenUpdateSucceeds()
        {
            var command = new UpdateNotificationStatus { Id = 10, ReadStatusId = 1 };
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationEventLog();

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationEventLog>(command))
                .Returns(entity);
            _mockRepo.Setup(r => r.UpdateAsync(10, entity)).ReturnsAsync(1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_Throws_WhenUpdateReturnsZero()
        {
            var command = new UpdateNotificationStatus { Id = 20, ReadStatusId = 1 };
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationEventLog();

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationEventLog>(command))
                .Returns(entity);
            _mockRepo.Setup(r => r.UpdateAsync(20, entity)).ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("Notification Log update failed.");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WhenUpdateSucceeds()
        {
            var command = new UpdateNotificationStatus { Id = 5, ReadStatusId = 1 };
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationEventLog { Id = 5, MessageText = "hi" };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationEventLog>(command))
                .Returns(entity);
            _mockRepo.Setup(r => r.UpdateAsync(5, entity)).ReturnsAsync(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.Module == "NotificationDetail" && e.ActionDetail == "Update"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
