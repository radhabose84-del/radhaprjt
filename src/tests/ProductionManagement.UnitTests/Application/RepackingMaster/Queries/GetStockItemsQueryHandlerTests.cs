using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Stock;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Sales;
using ProductionManagement.Application.RepackingMaster.Queries.GetStockItems;

namespace ProductionManagement.UnitTests.Application.RepackingMaster.Queries
{
    public sealed class GetStockItemsQueryHandlerTests
    {
        private readonly Mock<ISalesStockLedgerService> _mockStockLedger = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Strict);
        private readonly Mock<IPackTypeLookup> _mockPackTypeLookup = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetStockItemsQueryHandler CreateSut() =>
            new(_mockStockLedger.Object, _mockItemLookup.Object, _mockPackTypeLookup.Object, _mockMediator.Object, _mockIpService.Object);

        private void SetupHappyPath()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            var stockItems = new List<StockItemSummaryDto>
            {
                new() { ItemId = 10, PackTypeId = 20, TotalPackedBags = 5, TotalNetWeight = 100m }
            };
            _mockStockLedger.Setup(s => s.GetStockItemsAsync(2026, 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stockItems);

            _mockItemLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 10, ItemName = "Test Item" } });

            _mockPackTypeLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PackTypeLookupDto> { new() { Id = 20, PackTypeName = "Test Pack" } });

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(
                new GetStockItemsQuery { ProductionYear = 2026 },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ItemName.Should().Be("Test Item");
            result[0].PackTypeName.Should().Be("Test Pack");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(
                new GetStockItemsQuery { ProductionYear = 2026 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionCode == "GetStockItemsQuery" &&
                        e.ActionDetail == "GetStockItems"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyStock_ReturnsEmptyList()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockStockLedger.Setup(s => s.GetStockItemsAsync(2026, 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockItemSummaryDto>());
            _mockItemLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>());
            _mockPackTypeLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PackTypeLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStockItemsQuery { ProductionYear = 2026 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
