using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.IReports.IStockReport;
using InventoryManagement.Application.Reports.StockReportDivisionwise;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.Reports
{
    public sealed class GetStockReportDivsionwiseSummaryQueryHandlerTests
    {
        private readonly Mock<IStockReportQueryRepository> _mockStockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWHLookup = new(MockBehavior.Loose);
        private readonly Mock<IRackLookup> _mockRackLookup = new(MockBehavior.Loose);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Strict);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetStockReportDivsionwiseSummaryQueryHandler CreateSut() =>
            new(_mockStockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockWHLookup.Object, _mockRackLookup.Object, _mockBinLookup.Object,
                _mockMiscRepo.Object, _mockUnitLookup.Object);

        private void SetupDefaults(List<StockSummaryDivsionwiseDto> items)
        {
            _mockStockRepo.Setup(r => r.GetStockReportDivisionSummaryAsync(
                    It.IsAny<List<int>>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(items);
            _mockMapper.Setup(m => m.Map<List<StockSummaryDivsionwiseDto>>(It.IsAny<object>())).Returns(items);
            _mockMiscRepo.Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<InventoryManagement.Domain.Entities.MiscMaster>());
            _mockWHLookup.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<WarehouseLookupDto>());
            _mockBinLookup.Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<BinLookupDto>());
            _mockRackLookup.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<RackLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupDefaults(new List<StockSummaryDivsionwiseDto>());

            var result = await CreateSut().Handle(new GetStockReportDivsionwiseSummaryQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsGetStockReportDivisionSummaryOnce()
        {
            SetupDefaults(new List<StockSummaryDivsionwiseDto>());

            await CreateSut().Handle(new GetStockReportDivsionwiseSummaryQuery(), CancellationToken.None);

            _mockStockRepo.Verify(r => r.GetStockReportDivisionSummaryAsync(
                It.IsAny<List<int>>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsGetMiscMasterOnce()
        {
            SetupDefaults(new List<StockSummaryDivsionwiseDto>());

            await CreateSut().Handle(new GetStockReportDivsionwiseSummaryQuery(), CancellationToken.None);

            _mockMiscRepo.Verify(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
        }
    }
}
