using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetAllNotificationWhatsAppGroup;

namespace BackgroundService.UnitTests.Application.Notification.NotificationWhatsAppGroup.Queries
{
    public sealed class GetAllNotificationWhatsAppGroupQueryHandlerTests
    {
        private readonly Mock<INotificationWhatsAppGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetAllNotificationWhatsAppGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockLookupRepo.Object);

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var items = new List<NotificationWhatsAppGroupDto>
            {
                new() { Id = 1, GroupName = "TestGroup", DepartmentId = 1, UnitId = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<NotificationWhatsAppGroupListFilterDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 1));

            _mockLookupRepo
                .Setup(r => r.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 1, "Dept1" } });

            _mockLookupRepo
                .Setup(r => r.GetUnitsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 1, "Unit1" } });

            var (resultItems, totalCount, pageNumber, pageSize) = await CreateSut().Handle(
                new GetAllNotificationWhatsAppGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            resultItems.Should().HaveCount(1);
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<NotificationWhatsAppGroupListFilterDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<NotificationWhatsAppGroupDto>(), 0));

            var (resultItems, totalCount, _, _) = await CreateSut().Handle(
                new GetAllNotificationWhatsAppGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            resultItems.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }
}
