using MaintenanceManagement.Application.Common.Interfaces.IDashboard;
using MaintenanceManagement.Application.Dashboard.Common;
using MaintenanceManagement.Application.Dashboard.DashboardQuery;

namespace MaintenanceManagement.UnitTests.Application.Dashboard
{
    public sealed class DashboardQueryHandlerBatchATests
    {
        private readonly Mock<IDashboardQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private DashboardQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_NullType_ThrowsArgumentException()
        {
            Func<Task> act = async () => await CreateSut().Handle(
                new DashboardQuery { Type = null },
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Handle_EmptyType_ThrowsArgumentException()
        {
            Func<Task> act = async () => await CreateSut().Handle(
                new DashboardQuery { Type = "" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Handle_InvalidType_ThrowsArgumentException()
        {
            Func<Task> act = async () => await CreateSut().Handle(
                new DashboardQuery { Type = "nonexistent" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Handle_WorkOrderSummary_CallsCorrectRepoMethod()
        {
            var chart = new ChartDto();
            _mockRepo
                .Setup(r => r.WorkOrderSummaryAsync(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chart);

            var result = await CreateSut().Handle(
                new DashboardQuery
                {
                    Type = "workOrderSummary",
                    FromDate = DateTime.Today,
                    ToDate = DateTime.Today
                },
                CancellationToken.None);

            result.Should().BeSameAs(chart);
            _mockRepo.Verify(
                r => r.WorkOrderSummaryAsync(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ItemConsumption_CallsCorrectRepoMethod()
        {
            var chart = new ChartDto();
            _mockRepo
                .Setup(r => r.ItemConsumptionSummaryAsync(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chart);

            var result = await CreateSut().Handle(
                new DashboardQuery
                {
                    Type = "itemConsumption",
                    FromDate = DateTime.Today,
                    ToDate = DateTime.Today
                },
                CancellationToken.None);

            result.Should().BeSameAs(chart);
        }
    }
}
