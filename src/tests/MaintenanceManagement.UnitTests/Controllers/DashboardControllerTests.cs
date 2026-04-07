using MaintenanceManagement.Application.Dashboard.CardView;
using MaintenanceManagement.Application.Dashboard.Common;
using MaintenanceManagement.Application.Dashboard.DashboardQuery;
using MaintenanceManagement.Presentation.Controllers.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class DashboardControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DashboardController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetWorkOrderSummary_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto());

            var result = await CreateSut().GetWorkOrderSummary(new DashboardQuery());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetItemConsumption_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto());

            var result = await CreateSut().GetItemConsumption(new DashboardQuery());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMaintenanceHoursDept_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto());

            var result = await CreateSut().GetMaintenanceHoursDept(new DashboardQuery());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMaintenanceHours_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto());

            var result = await CreateSut().GetMaintenanceHours(new DashboardQuery());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetItemConsumptionDept_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto());

            var result = await CreateSut().GetItemConsumptionDept(new DashboardQuery());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetItemConsumptionMachineGroup_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto());

            var result = await CreateSut().GetItemConsumptionMachineGroup(new DashboardQuery());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCardDashboard_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CardViewQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CardViewDto());

            var result = await CreateSut().GetCardDashboard(new CardViewQuery());
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
