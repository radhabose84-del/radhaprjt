using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationTemplate.Commands
{
    public sealed class UpdateNotificationTemplateCommandHandlerTests
    {
        private readonly Mock<INotificationTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateNotificationTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateNotificationTemplateCommand ValidCommand() =>
            new()
            {
                Id = 1,
                NotificationTypeId = 1,
                NotificationConfigId = 1,
                SubjectTemplate = "Updated",
                HeaderTemplate = "Header",
                BodyTemplate = "Body",
                FooterTemplate = "Footer",
                LanguageCode = "en",
                IsActive = 1
            };

        private void SetupHappyPath(int result = 1)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationTemplate { Id = 1, NotificationConfigId = 1 };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationTemplate>(It.IsAny<UpdateNotificationTemplateCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationTemplate>()))
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
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationTemplate>()),
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
