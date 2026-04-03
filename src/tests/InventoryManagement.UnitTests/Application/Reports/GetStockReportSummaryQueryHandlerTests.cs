using AutoMapper;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.IReports.IStockReport;
using InventoryManagement.Application.Reports.StockReport;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.Reports
{
    public sealed class GetStockReportSummaryQueryHandlerTests
    {
        private readonly Mock<IStockReportQueryRepository> _mockStockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWHLookup = new(MockBehavior.Loose);
        private readonly Mock<IRackLookup> _mockRackLookup = new(MockBehavior.Loose);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Strict);

        private GetStockReportSummaryQueryHandler CreateSut() =>
            new(_mockStockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockWHLookup.Object, _mockRackLookup.Object, _mockBinLookup.Object,
                _mockMiscRepo.Object);

        private void SetupDefaults(List<StockSummaryDto> items)
        {
            _mockStockRepo.Setup(r => r.GetStockSummaryAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(items);
            _mockMapper.Setup(m => m.Map<List<StockSummaryDto>>(It.IsAny<object>())).Returns(items);
            _mockMiscRepo.Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<InventoryManagement.Domain.Entities.MiscMaster>());
            _mockWHLookup.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<WarehouseLookupDto>());
            _mockBinLookup.Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<BinLookupDto>());
            _mockRackLookup.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<RackLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupDefaults(new List<StockSummaryDto>());

            var result = await CreateSut().Handle(new GetStockReportSummaryQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsGetStockSummaryOnce()
        {
            SetupDefaults(new List<StockSummaryDto>());

            await CreateSut().Handle(new GetStockReportSummaryQuery(), CancellationToken.None);

            _mockStockRepo.Verify(r => r.GetStockSummaryAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Once);
        }
    }
}
