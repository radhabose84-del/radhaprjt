using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupAutoComplete;

namespace BackgroundService.UnitTests.Application.Notification.NotificationWhatsAppGroup.Queries
{
    public sealed class GetNotificationWhatsAppGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<INotificationWhatsAppGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetNotificationWhatsAppGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockLookupRepo.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<NotificationWhatsAppGroupAutoCompleteDto>
            {
                new() { Id = 1, GroupName = "TestGroup", DepartmentId = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetAutoCompleteAsync("Test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            _mockLookupRepo
                .Setup(r => r.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 1, "Dept1" } });

            var result = await CreateSut().Handle(
                new GetNotificationWhatsAppGroupAutoCompleteQuery { SearchTerm = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmpty()
        {
            _mockQueryRepo
                .Setup(r => r.GetAutoCompleteAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationWhatsAppGroupAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetNotificationWhatsAppGroupAutoCompleteQuery { SearchTerm = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
