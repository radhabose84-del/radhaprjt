using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.UnitTests.Application.Notification.NotificationHierarchyAndEventRule.Queries
{
    public sealed class GetNotificationHierarchyByIdQueryHandlerTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationHierarchyByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = new NotificationLevelHierarchy { Id = 1, Description = "Test" };
            var dto = new NotificationHierarchyAndEventRuleDto { Id = 1, Description = "Test" };

            _mockRepo.Setup(r => r.GetByIdWithEventRuleAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<NotificationHierarchyAndEventRuleDto>(entity)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetNotificationHierarchyByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsExceptionRules()
        {
            _mockRepo
                .Setup(r => r.GetByIdWithEventRuleAsync(99))
                .ReturnsAsync((NotificationLevelHierarchy?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetNotificationHierarchyByIdQuery { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }
    }
}
