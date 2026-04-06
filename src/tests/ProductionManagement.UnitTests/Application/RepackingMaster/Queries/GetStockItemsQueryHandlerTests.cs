using Contracts.Dtos.Stock;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Sales;
using ProductionManagement.Application.RepackingMaster.Queries.GetStockItems;

namespace ProductionManagement.UnitTests.Application.RepackingMaster.Queries
{
    public sealed class GetStockItemsQueryHandlerTests
    {
        private readonly Mock<ISalesStockLedgerService> _mockStockLedger = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetStockItemsQueryHandler CreateSut() =>
            new(_mockStockLedger.Object, _mockIpService.Object);

        private void SetupHappyPath()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            var stockItems = new List<StockItemSummaryDto>
            {
                new() { ItemId = 10, ItemName = "Test Item", TotalBags = 5, TotalNetWeight = 100m }
            };
            _mockStockLedger.Setup(s => s.GetStockItemsAsync(2026, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stockItems);
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
        }

        [Fact]
        public async Task Handle_EmptyStock_ReturnsEmptyList()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockStockLedger.Setup(s => s.GetStockItemsAsync(2026, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockItemSummaryDto>());

            var result = await CreateSut().Handle(
                new GetStockItemsQuery { ProductionYear = 2026 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_UsesUnitIdFromIpService()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(5);
            _mockStockLedger.Setup(s => s.GetStockItemsAsync(2026, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockItemSummaryDto>());

            await CreateSut().Handle(
                new GetStockItemsQuery { ProductionYear = 2026 },
                CancellationToken.None);

            _mockStockLedger.Verify(
                s => s.GetStockItemsAsync(2026, 5, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
