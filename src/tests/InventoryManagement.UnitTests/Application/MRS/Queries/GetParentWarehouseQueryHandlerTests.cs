using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.MRS.Queries.GetParentWarehouse;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetParentWarehouseQueryHandlerTests
    {
        private readonly Mock<IWarehouseLookup> _mockWHLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetParentWarehouseQueryHandler CreateSut() =>
            new(_mockWHLookup.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WarehouseNotFound_ReturnsDtoWithParentIdZero()
        {
            _mockWHLookup
                .Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<WarehouseLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetParentWarehouseQuery { WarehouseId = 1 }, CancellationToken.None);

            result.ParentWarehouseId.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WarehouseExistsWithParent_ReturnsDtoWithParentId()
        {
            var warehouse = new WarehouseLookupDto { Id = 1, WarehouseName = "WH1", ParentWarehouseId = 5 };
            var parent = new WarehouseLookupDto { Id = 5, WarehouseName = "Parent WH" };

            _mockWHLookup
                .SetupSequence(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto> { warehouse })
                .ReturnsAsync(new List<WarehouseLookupDto> { parent });
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetParentWarehouseQuery { WarehouseId = 1 }, CancellationToken.None);

            result.ParentWarehouseId.Should().Be(5);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            // Audit event is only published when warehouse exists AND has a parent
            var warehouse = new WarehouseLookupDto { Id = 99, WarehouseName = "WH99", ParentWarehouseId = 5 };
            var parent = new WarehouseLookupDto { Id = 5, WarehouseName = "Parent WH" };

            _mockWHLookup
                .SetupSequence(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto> { warehouse })
                .ReturnsAsync(new List<WarehouseLookupDto> { parent });
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetParentWarehouseQuery { WarehouseId = 99 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
