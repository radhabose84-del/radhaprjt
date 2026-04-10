using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.UnitTests.Application.Notification.NotificationHierarchyAndEventRule.Commands
{
    public sealed class InsertNotificationHierarchyAndEventRuleCommandHandlerTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockHierarchyRepo = new(MockBehavior.Strict);
        private readonly Mock<INotificationEventRuleCommand> _mockEventRuleRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.InsertNotificationEventRule.InsertNotificationHierarchyAndEventRuleCommandHandler CreateSut() =>
            new(_mockHierarchyRepo.Object, _mockEventRuleRepo.Object, _mockMapper.Object);

        private static InsertNotificationHierarchyAndEventRuleCommand ValidCommand() =>
            new(new NotificationHierarchyAndEventRuleDto
            {
                NotificationConfigId = 1,
                TargetTypeId = 2,
                TargetId = 3,
                ApprovalModeId = 1,
                Description = "Test",
                NotificationEventRules = new List<NotificationEventRuleDto>
                {
                    new() { NotificationChannelId = 1, RecipientTypeId = 1, TemplateId = 1 }
                }
            });

        private void SetupHappyPath()
        {
            _mockMapper
                .Setup(m => m.Map<NotificationLevelHierarchy>(It.IsAny<NotificationHierarchyAndEventRuleDto>()))
                .Returns(new NotificationLevelHierarchy { Id = 0, Description = "Test" });

            _mockMapper
                .Setup(m => m.Map<NotificationEventRule>(It.IsAny<NotificationEventRuleDto>()))
                .Returns(new NotificationEventRule { NotificationChannelId = 1 });

            _mockHierarchyRepo
                .Setup(r => r.InsertAsync(It.IsAny<NotificationLevelHierarchy>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsInsertAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockHierarchyRepo.Verify(
                r => r.InsertAsync(It.IsAny<NotificationLevelHierarchy>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InsertFails_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<NotificationLevelHierarchy>(It.IsAny<NotificationHierarchyAndEventRuleDto>()))
                .Returns(new NotificationLevelHierarchy());

            _mockMapper
                .Setup(m => m.Map<NotificationEventRule>(It.IsAny<NotificationEventRuleDto>()))
                .Returns(new NotificationEventRule());

            _mockHierarchyRepo
                .Setup(r => r.InsertAsync(It.IsAny<NotificationLevelHierarchy>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Failed to insert*");
        }
    }
}
