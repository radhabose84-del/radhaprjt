using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReports.IStockReport;
using PurchaseManagement.Application.Reports.StockReport;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.Reports.StockReport
{
    public sealed class GetStockReportSummaryQueryHandlerTests
    {
        private readonly Mock<IStockReportQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseToleranceLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterLookup> _mockMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<IRackLookup> _mockRackLookup = new(MockBehavior.Loose);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private GetStockReportSummaryQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockWarehouseLookup.Object, _mockItemLookup.Object, _mockMiscLookup.Object,
                _mockRackLookup.Object, _mockBinLookup.Object, _mockUomLookup.Object);

        private void SetupEmptyLookups()
        {
            _mockMiscLookup.Setup(l => l.GetMiscMasterByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Inventory.MiscMasterLookupDto>());
            _mockItemLookup.Setup(l => l.GetByIdsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Inventory.ItemPurchaseToleranceLookupDto>)new List<Contracts.Dtos.Lookups.Inventory.ItemPurchaseToleranceLookupDto>());
            _mockWarehouseLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Warehouse.WarehouseLookupDto>)new List<Contracts.Dtos.Lookups.Warehouse.WarehouseLookupDto>());
            _mockBinLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Warehouse.BinLookupDto>)new List<Contracts.Dtos.Lookups.Warehouse.BinLookupDto>());
            _mockRackLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Warehouse.RackLookupDto>)new List<Contracts.Dtos.Lookups.Warehouse.RackLookupDto>());
            _mockUomLookup.Setup(l => l.GetByIdsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>)new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>());
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var query = new GetStockReportSummaryQuery();
            _mockRepo.Setup(r => r.GetStockSummaryAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<StockSummaryDto>());
            _mockMapper.Setup(m => m.Map<List<StockSummaryDto>>(It.IsAny<object>()))
                .Returns(new List<StockSummaryDto>());
            SetupEmptyLookups();

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ValidQuery_PublishesAuditEvent()
        {
            var query = new GetStockReportSummaryQuery();
            _mockRepo.Setup(r => r.GetStockSummaryAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<StockSummaryDto>());
            _mockMapper.Setup(m => m.Map<List<StockSummaryDto>>(It.IsAny<object>()))
                .Returns(new List<StockSummaryDto>());
            SetupEmptyLookups();

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsEnrichedList()
        {
            var query = new GetStockReportSummaryQuery();
            _mockRepo.Setup(r => r.GetStockSummaryAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<StockSummaryDto>());
            var dtos = new List<StockSummaryDto>
            {
                new StockSummaryDto { ItemId = 1, WarehouseId = 1, StorageTypeId = 1, TargetId = 1, UomId = 1 }
            };
            _mockMapper.Setup(m => m.Map<List<StockSummaryDto>>(It.IsAny<object>()))
                .Returns(dtos);
            SetupEmptyLookups();

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
