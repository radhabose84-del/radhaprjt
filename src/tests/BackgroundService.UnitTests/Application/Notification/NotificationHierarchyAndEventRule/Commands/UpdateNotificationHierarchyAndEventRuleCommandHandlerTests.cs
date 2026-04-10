using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.UnitTests.Application.Notification.NotificationHierarchyAndEventRule.Commands
{
    public sealed class UpdateNotificationHierarchyAndEventRuleCommandHandlerTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockHierarchyRepo = new(MockBehavior.Strict);
        private readonly Mock<INotificationEventRuleCommand> _mockEventRuleRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateNotificationHierarchyAndEventRuleCommandHandler CreateSut() =>
            new(_mockHierarchyRepo.Object, _mockEventRuleRepo.Object, _mockMapper.Object);

        private static UpdateNotificationHierarchyAndEventRuleCommand ValidCommand() =>
            new()
            {
                NotificationLevelHierarchyId = 1,
                NotificationConfigId = 1,
                TargetTypeId = 2,
                TargetId = 3,
                ApprovalModeId = 1,
                Description = "Updated",
                IsActive = 1,
                NotificationEventRules = new List<NotificationEventRuleDto>
                {
                    new() { NotificationChannelId = 1, RecipientTypeId = 1, TemplateId = 1 }
                }
            };

        private void SetupHappyPath()
        {
            var existing = new NotificationLevelHierarchy
            {
                Id = 1,
                Description = "Old",
                NotificationEventRules = new List<NotificationEventRule>
                {
                    new() { Id = 10, NotificationChannelId = 1 }
                }
            };

            _mockHierarchyRepo
                .Setup(r => r.GetByIdWithEventRuleAsync(1))
                .ReturnsAsync(existing);

            _mockEventRuleRepo
                .Setup(r => r.DeleteRangeAsync(It.IsAny<List<NotificationEventRule>>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<NotificationEventRule>(It.IsAny<NotificationEventRuleDto>()))
                .Returns(new NotificationEventRule { NotificationChannelId = 1 });

            _mockHierarchyRepo
                .Setup(r => r.UpdateAsync(It.IsAny<NotificationLevelHierarchy>()))
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
        public async Task Handle_HierarchyNotFound_ThrowsExceptionRules()
        {
            _mockHierarchyRepo
                .Setup(r => r.GetByIdWithEventRuleAsync(1))
                .ReturnsAsync((NotificationLevelHierarchy?)null);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsExceptionRules()
        {
            var existing = new NotificationLevelHierarchy
            {
                Id = 1,
                NotificationEventRules = new List<NotificationEventRule>()
            };

            _mockHierarchyRepo
                .Setup(r => r.GetByIdWithEventRuleAsync(1))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<NotificationEventRule>(It.IsAny<NotificationEventRuleDto>()))
                .Returns(new NotificationEventRule());

            _mockHierarchyRepo
                .Setup(r => r.UpdateAsync(It.IsAny<NotificationLevelHierarchy>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Failed to update*");
        }
    }
}
