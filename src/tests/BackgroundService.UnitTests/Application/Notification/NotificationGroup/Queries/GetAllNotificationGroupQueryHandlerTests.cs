using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Queries.GetAllNotificationGroup;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroup.Queries
{
    public sealed class GetAllNotificationGroupQueryHandlerTests
    {
        private readonly Mock<INotificationGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetAllNotificationGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<NotificationGroupDto>
            {
                new() { Id = 1, GroupName = "TestGroup" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllNotificationGroupAsync(1, 15, null))
                .ReturnsAsync((dtoList, 1));

            _mockMapper
                .Setup(m => m.Map<List<NotificationGroupDto>>(It.IsAny<List<NotificationGroupDto>>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllNotificationGroupQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllNotificationGroupAsync(1, 15, null))
                .ReturnsAsync((new List<NotificationGroupDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<NotificationGroupDto>>(It.IsAny<List<NotificationGroupDto>>()))
                .Returns(new List<NotificationGroupDto>());

            var result = await CreateSut().Handle(
                new GetAllNotificationGroupQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
