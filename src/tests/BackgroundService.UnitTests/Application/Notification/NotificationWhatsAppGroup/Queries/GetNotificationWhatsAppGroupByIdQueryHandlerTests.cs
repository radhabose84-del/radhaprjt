using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupById;

namespace BackgroundService.UnitTests.Application.Notification.NotificationWhatsAppGroup.Queries
{
    public sealed class GetNotificationWhatsAppGroupByIdQueryHandlerTests
    {
        private readonly Mock<INotificationWhatsAppGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetNotificationWhatsAppGroupByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockLookupRepo.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = new NotificationWhatsAppGroupDto { Id = 1, GroupName = "TestGroup", DepartmentId = 1, UnitId = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockLookupRepo
                .Setup(r => r.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 1, "Dept1" } });

            _mockLookupRepo
                .Setup(r => r.GetUnitsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 1, "Unit1" } });

            var result = await CreateSut().Handle(
                new GetNotificationWhatsAppGroupByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.GroupName.Should().Be("TestGroup");
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((NotificationWhatsAppGroupDto?)null);

            var result = await CreateSut().Handle(
                new GetNotificationWhatsAppGroupByIdQuery { Id = 99 },
                CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
