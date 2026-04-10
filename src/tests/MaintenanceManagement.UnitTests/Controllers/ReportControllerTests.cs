using Contracts.Common;
using MaintenanceManagement.Application.Reports.GeneratorConsumption;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Application.Reports.GetCurrentAllStockItems;
using MaintenanceManagement.Application.Reports.GetStockLegerReport;
using MaintenanceManagement.Application.Reports.MaintenanceRequestReport;
using MaintenanceManagement.Application.Reports.MaterialPlanningReport;
using MaintenanceManagement.Application.Reports.MRS;
using MaintenanceManagement.Application.Reports.PowerConsumption;
using MaintenanceManagement.Application.Reports.ScheduleReport;
using MaintenanceManagement.Application.Reports.WorkOderCheckListReport;
using MaintenanceManagement.Application.Reports.WorkOrderItemConsuption;
using MaintenanceManagement.Application.Reports.WorkOrderReport;
using MaintenanceManagement.Presentation.Controllers.Reports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class ReportControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private ReportController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task WorkOrderReport_ValidDates_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<WorkOrderReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkOrderReportDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().WorkOrderReportAsync("2025-01-01", "2025-12-31", 1, null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task WorkOrderReport_InvalidFromDate_ReturnsBadRequest()
        {
            var result = await CreateSut().WorkOrderReportAsync("invalid", "2025-12-31", 1, null);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ItemConsumption_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<WorkOrderIssueQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkOrderIssueDto>> { IsSuccess = true, Message = "Success", Data = new() });

            var result = await CreateSut().GetAllItemConsuption("2025-01-01", "2025-12-31");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SubStoresStockLedger_ValidParams_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetStockLegerReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StockLedgerReportDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetSubStoresStockLedger("U001", new DateTime(2025, 4, 1), new DateTime(2025, 6, 30), 1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SubStoresStockLedger_EmptyUnitCode_ReturnsBadRequest()
        {
            var result = await CreateSut().GetSubStoresStockLedger("", DateTime.Now, DateTime.Now, 1);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SubStoresStockLedger_FromDateAfterToDate_ReturnsBadRequest()
        {
            var result = await CreateSut().GetSubStoresStockLedger("U001", new DateTime(2025, 12, 31), new DateTime(2025, 1, 1), 1);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CurrentStock_Success_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCurrentAllStockItemsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrentStockDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetAllStockItemDetails("U001", 1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CurrentStock_NotFound_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCurrentAllStockItemsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrentStockDto>> { IsSuccess = false, Message = "Not found" });

            var result = await CreateSut().GetAllStockItemDetails("U001", 1);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task RequestReport_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<RequestReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RequestReportDto>> { IsSuccess = true, Message = "Success", Data = new() });

            var result = await CreateSut().MaintenanceReportAsync(null, null, null, null, null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task WorkOrderChecklistReport_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<WorkOderCheckListReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkOderCheckListReportDto>> { IsSuccess = true, Message = "Success", Data = new() });

            var result = await CreateSut().WorkOrderChecklistReportAsync(null, null, null, null, null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task MRSReport_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<MRSReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MRSReportDto>> { IsSuccess = true, Message = "Success", Data = new() });

            var result = await CreateSut().GetMRSReport("2025-01-01", "2025-12-31", "U001");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task MRSReport_NullUnitCode_ReturnsBadRequest()
        {
            var result = await CreateSut().GetMRSReport("2025-01-01", "2025-12-31", null);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SchedulerReport_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ScheduleReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ScheduleReportDto>> { IsSuccess = true, Message = "Success", Data = new() });

            var result = await CreateSut().SchedulerReportAsync(null, null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task MaterialPlanningReport_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<MaterialPlanningReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MaterialPlanningReportDto>> { IsSuccess = true, Message = "Success", Data = new() });

            var result = await CreateSut().MaterialPlanningReportAsync(null, null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PowerConsumptionReport_WithData_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<PowerConsumptionReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PowerReportDto>> { IsSuccess = true, Message = "Success", Data = new() { new() } });

            var result = await CreateSut().AssetTransferReportAsync(null, null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PowerConsumptionReport_NoData_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<PowerConsumptionReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PowerReportDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().AssetTransferReportAsync(null, null);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GeneratorConsumptionReport_WithData_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GeneratorConsumptionReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GeneratorReportDto>> { IsSuccess = true, Message = "Success", Data = new() { new() } });

            var result = await CreateSut().GeneratorConsumptionReportAsync(null, null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GeneratorConsumptionReport_NoData_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GeneratorConsumptionReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GeneratorReportDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GeneratorConsumptionReportAsync(null, null);
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
