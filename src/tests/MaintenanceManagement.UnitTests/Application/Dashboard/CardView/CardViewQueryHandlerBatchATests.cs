using MaintenanceManagement.Application.Common.Interfaces.IDashboard;
using MaintenanceManagement.Application.Dashboard.CardView;
using MaintenanceManagement.Application.Dashboard.DashboardQuery;

namespace MaintenanceManagement.UnitTests.Application.Dashboard.CardView
{
    public sealed class CardViewQueryHandlerBatchATests
    {
        private readonly Mock<IDashboardQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private CardViewQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsCardViewDto()
        {
            var dto = new CardViewDto();
            _mockRepo
                .Setup(r => r.GetCardDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new CardViewQuery
                {
                    FromDate = DateTime.Today.AddDays(-7),
                    ToDate = DateTime.Today,
                    DepartmentId = "1",
                    MachineGroupId = "1"
                },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var dto = new CardViewDto();
            _mockRepo
                .Setup(r => r.GetCardDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(dto);

            await CreateSut().Handle(
                new CardViewQuery
                {
                    FromDate = DateTime.Today,
                    ToDate = DateTime.Today,
                    DepartmentId = "d",
                    MachineGroupId = "m"
                },
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetCardDashboardAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    "CardView",
                    "d",
                    "m"),
                Times.Once);
        }

        [Fact]
        public void Handler_ImplementsIRequestHandler()
        {
            typeof(MediatR.IRequestHandler<CardViewQuery, CardViewDto>)
                .IsAssignableFrom(typeof(CardViewQueryHandler))
                .Should().BeTrue();
        }
    }
}
