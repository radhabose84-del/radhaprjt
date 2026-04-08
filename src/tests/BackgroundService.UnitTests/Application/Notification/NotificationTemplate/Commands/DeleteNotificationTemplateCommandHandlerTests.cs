using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationTemplate.Commands
{
    public sealed class DeleteNotificationTemplateCommandHandlerTests
    {
        private readonly Mock<INotificationTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteNotificationTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationTemplate { Id = 1, NotificationConfigId = 1 };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationTemplate>(It.IsAny<DeleteNotificationTemplateCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationTemplate>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResult()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(new DeleteNotificationTemplateCommand { Id = 1 }, CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(new DeleteNotificationTemplateCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationTemplate>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsZero_ThrowsExceptionRules()
        {
            SetupHappyPath(0);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteNotificationTemplateCommand { Id = 99 }, CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }
    }
}
