using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationConfig.Queries
{
    public sealed class GetAllNotificationConfigQueryHandlerTests
    {
        private readonly Mock<INotificationConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetAllNotificationConfigQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<NotificationConfigDto>
            {
                new() { Id = 1, ModuleName = "Test", NotificationEventTypeId = 1 }
            };
            var dynamicList = dtoList.Cast<dynamic>().ToList() as IEnumerable<dynamic>;

            _mockQueryRepo
                .Setup(r => r.GetAllNotificationConfigAsync(1, 15, null))
                .ReturnsAsync((dynamicList, 1));

            _mockMapper
                .Setup(m => m.Map<List<NotificationConfigDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllNotificationConfigQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = Enumerable.Empty<dynamic>();

            _mockQueryRepo
                .Setup(r => r.GetAllNotificationConfigAsync(1, 15, null))
                .ReturnsAsync((emptyList, 0));

            _mockMapper
                .Setup(m => m.Map<List<NotificationConfigDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(new List<NotificationConfigDto>());

            var result = await CreateSut().Handle(
                new GetAllNotificationConfigQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<NotificationConfigDto> { new() { Id = 1 } };
            var dynamicList = dtoList.Cast<dynamic>().ToList() as IEnumerable<dynamic>;

            _mockQueryRepo
                .Setup(r => r.GetAllNotificationConfigAsync(2, 5, "search"))
                .ReturnsAsync((dynamicList, 11));

            _mockMapper
                .Setup(m => m.Map<List<NotificationConfigDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllNotificationConfigQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
