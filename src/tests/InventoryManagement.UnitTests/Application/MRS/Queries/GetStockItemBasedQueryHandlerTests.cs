using AutoMapper;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.MRS.Queries.GetStockItemBased;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetStockItemBasedQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWHLookup = new(MockBehavior.Loose);
        private readonly Mock<IMrsEntryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetStockItemBasedQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockWHLookup.Object, _mockQueryRepo.Object);

        private void SetupDefaults(List<GetStockItemDto> items)
        {
            _mockQueryRepo.Setup(r => r.GetStockDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(items);
            _mockMapper.Setup(m => m.Map<List<GetStockItemDto>>(It.IsAny<object>())).Returns(items);
            _mockWHLookup
                .Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<WarehouseLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsListOfStockItems()
        {
            var items = new List<GetStockItemDto> { new GetStockItemDto { WarehouseId = 0 } };
            SetupDefaults(items);

            var result = await CreateSut().Handle(
                new GetStockItemBasedQuery { ItemId = 1, WarehouseId = 0 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupDefaults(new List<GetStockItemDto>());

            var result = await CreateSut().Handle(
                new GetStockItemBasedQuery(),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsGetStockDetailsOnce()
        {
            SetupDefaults(new List<GetStockItemDto>());

            await CreateSut().Handle(new GetStockItemBasedQuery { ItemId = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetStockDetails(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}
