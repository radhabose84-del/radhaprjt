using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationTemplate.Commands
{
    public sealed class CreateNotificationTemplateCommandHandlerTests
    {
        private readonly Mock<INotificationTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateNotificationTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateNotificationTemplateCommand ValidCommand() =>
            new()
            {
                NotificationTypeId = 1,
                NotificationConfigId = 1,
                SubjectTemplate = "Subject",
                HeaderTemplate = "Header",
                BodyTemplate = "Body",
                FooterTemplate = "Footer",
                LanguageCode = "en"
            };

        private void SetupHappyPath(int newId = 1)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationTemplate
            {
                Id = newId,
                NotificationConfigId = 1
            };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationTemplate>(It.IsAny<CreateNotificationTemplateCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationTemplate>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationTemplate>()),
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
                        e.ActionDetail == "Create" &&
                        e.Module == "NotificationTemplate"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationTemplate { Id = 0, NotificationConfigId = 1 };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationTemplate>(It.IsAny<CreateNotificationTemplateCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationTemplate>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Creation Failed*");
        }
    }
}
