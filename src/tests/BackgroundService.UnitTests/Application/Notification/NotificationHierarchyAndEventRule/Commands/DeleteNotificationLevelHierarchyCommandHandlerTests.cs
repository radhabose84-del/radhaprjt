using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.UnitTests.Application.Notification.NotificationHierarchyAndEventRule.Commands
{
    public sealed class DeleteNotificationLevelHierarchyCommandHandlerTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockHierarchyRepo = new(MockBehavior.Strict);
        private readonly Mock<INotificationEventRuleCommand> _mockEventRuleRepo = new(MockBehavior.Strict);

        private DeleteNotificationLevelHierarchyCommandHandler CreateSut() =>
            new(_mockHierarchyRepo.Object, _mockEventRuleRepo.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var hierarchy = new NotificationLevelHierarchy { Id = 1, Description = "Test" };

            _mockHierarchyRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(hierarchy);
            _mockEventRuleRepo.Setup(r => r.DeleteByHierarchyIdAsync(1)).ReturnsAsync(true);
            _mockHierarchyRepo.Setup(r => r.DeleteAsync(hierarchy)).ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new DeleteNotificationLevelHierarchyCommand { Id = 1 },
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_HierarchyNotFound_ThrowsExceptionRules()
        {
            _mockHierarchyRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((NotificationLevelHierarchy?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteNotificationLevelHierarchyCommand { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            var hierarchy = new NotificationLevelHierarchy { Id = 1 };

            _mockHierarchyRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(hierarchy);
            _mockEventRuleRepo.Setup(r => r.DeleteByHierarchyIdAsync(1)).ReturnsAsync(true);
            _mockHierarchyRepo.Setup(r => r.DeleteAsync(hierarchy)).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteNotificationLevelHierarchyCommand { Id = 1 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Failed to delete*");
        }
    }
}
