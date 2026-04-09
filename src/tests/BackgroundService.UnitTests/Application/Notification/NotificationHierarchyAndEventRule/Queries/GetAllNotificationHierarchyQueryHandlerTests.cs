using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetAllNotificationHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.UnitTests.Application.Notification.NotificationHierarchyAndEventRule.Queries
{
    public sealed class GetAllNotificationHierarchyQueryHandlerTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetAllNotificationHierarchyQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<NotificationLevelHierarchy>
            {
                new() { Id = 1, Description = "Test" }
            };
            var dtoList = new List<NotificationHierarchyAndEventRuleDto>
            {
                new() { Id = 1, Description = "Test" }
            };

            _mockRepo
                .Setup(r => r.GetAllWithEventRuleAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<NotificationHierarchyAndEventRuleDto>>(It.IsAny<List<NotificationLevelHierarchy>>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllNotificationHierarchyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo
                .Setup(r => r.GetAllWithEventRuleAsync(1, 10, null))
                .ReturnsAsync((new List<NotificationLevelHierarchy>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<NotificationHierarchyAndEventRuleDto>>(It.IsAny<List<NotificationLevelHierarchy>>()))
                .Returns(new List<NotificationHierarchyAndEventRuleDto>());

            var result = await CreateSut().Handle(
                new GetAllNotificationHierarchyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
