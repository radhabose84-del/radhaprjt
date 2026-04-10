using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Queries.GetNotificationGroupAutoComplete;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroup.Queries
{
    public sealed class GetNotificationGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<INotificationGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var entities = new List<BackgroundService.Domain.Entities.Notification.NotificationGroup>
            {
                new() { Id = 1, GroupName = "TestGroup" }
            };
            var dtoList = new List<GetNotificationGroupAutoCompleteDto>
            {
                new() { Id = 1, GroupName = "TestGroup" }
            };

            _mockQueryRepo
                .Setup(r => r.GetNotificationGroupsAutoComplete("Test"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetNotificationGroupAutoCompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.NotificationGroup>>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetNotificationGroupAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].GroupName.Should().Be("TestGroup");
        }

        [Fact]
        public async Task Handle_NullSearch_UsesEmptyString()
        {
            _mockQueryRepo
                .Setup(r => r.GetNotificationGroupsAutoComplete(string.Empty))
                .ReturnsAsync(new List<BackgroundService.Domain.Entities.Notification.NotificationGroup>());

            _mockMapper
                .Setup(m => m.Map<List<GetNotificationGroupAutoCompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.NotificationGroup>>()))
                .Returns(new List<GetNotificationGroupAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetNotificationGroupAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
