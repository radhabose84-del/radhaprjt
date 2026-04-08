using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupByDepartment;

namespace BackgroundService.UnitTests.Application.Notification.NotificationWhatsAppGroup.Queries
{
    public sealed class GetNotificationWhatsAppGroupByDepartmentQueryHandlerTests
    {
        private readonly Mock<INotificationWhatsAppGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetNotificationWhatsAppGroupByDepartmentQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockLookupRepo.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<NotificationWhatsAppGroupAutoCompleteDto>
            {
                new() { Id = 1, GroupName = "TestGroup", DepartmentId = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetByDepartmentAsync(1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            _mockLookupRepo
                .Setup(r => r.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 1, "Dept1" } });

            var result = await CreateSut().Handle(
                new GetNotificationWhatsAppGroupByDepartmentQuery { DepartmentId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmpty()
        {
            _mockQueryRepo
                .Setup(r => r.GetByDepartmentAsync(99, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationWhatsAppGroupAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetNotificationWhatsAppGroupByDepartmentQuery { DepartmentId = 99 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
