using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock;
using MaintenanceManagement.Application.MainStoreStock.Queries.GetItemStockbyId;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MainStoreStock.Queries.Batch2
{
    public sealed class GetItemStockbyIdQueryHandlerTests
    {
        private readonly Mock<IMainStoreStockQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemStockbyIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResult_ReturnsDto()
        {
            var dto = new MainStoreItemStockDto { StockQty = 100 };
            _mockQueryRepo
                .Setup(r => r.GetByItemCodeIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<MainStoreItemStockDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemStockbyIdQuery { OldUnitcode = "U01", ItemCode = "I01" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.StockQty.Should().Be(100);
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            var dto = new MainStoreItemStockDto();
            _mockQueryRepo
                .Setup(r => r.GetByItemCodeIdAsync("U01", "I01"))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<MainStoreItemStockDto>(It.IsAny<object>()))
                .Returns(dto);

            await CreateSut().Handle(
                new GetItemStockbyIdQuery { OldUnitcode = "U01", ItemCode = "I01" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByItemCodeIdAsync("U01", "I01"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dto = new MainStoreItemStockDto();
            _mockQueryRepo
                .Setup(r => r.GetByItemCodeIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<MainStoreItemStockDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetItemStockbyIdQuery { OldUnitcode = "U01", ItemCode = "I01" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MainstoreStockItemsFetched"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
