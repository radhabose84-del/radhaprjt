using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationConfig.Commands
{
    public sealed class DeleteNotificationConfigCommandHandlerTests
    {
        private readonly Mock<INotificationConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteNotificationConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationConfig { Id = 1 };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationConfig>(It.IsAny<DeleteNotificationConfigCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationConfig>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResult()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(new DeleteNotificationConfigCommand { Id = 1 }, CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(new DeleteNotificationConfigCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationConfig>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(new DeleteNotificationConfigCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "NotificationConfig "),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsZero_ThrowsExceptionRules()
        {
            SetupHappyPath(0);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteNotificationConfigCommand { Id = 99 }, CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }
    }
}
