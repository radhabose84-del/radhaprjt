using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationConfig.Commands
{
    public sealed class UpdateNotificationConfigCommandHandlerTests
    {
        private readonly Mock<INotificationConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateNotificationConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateNotificationConfigCommand ValidCommand() =>
            new() { Id = 1, ModuleName = "UpdatedModule", NotificationEventTypeId = 2, IsActive = 1 };

        private void SetupHappyPath(int result = 1)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationConfig
            {
                Id = 1,
                ModuleName = "UpdatedModule"
            };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationConfig>(It.IsAny<UpdateNotificationConfigCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationConfig>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResult()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationConfig>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "NotificationConfig"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsExceptionRules()
        {
            SetupHappyPath(0);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*update failed*");
        }
    }
}
