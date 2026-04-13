using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock;
using MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStockItems;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MainStoreStock.Queries.Batch2
{
    public sealed class GetMainStoreStockItemsQueryHandlerTests
    {
        private readonly Mock<IMainStoreStockQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMainStoreStockItemsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(List<MainStoresStockItemsDto>? dtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetStockItemsCodes(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MainStoresStockItemsDto>());
            _mockMapper
                .Setup(m => m.Map<List<MainStoresStockItemsDto>>(It.IsAny<object>()))
                .Returns(dtos ?? new List<MainStoresStockItemsDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            SetupHappyPath(new List<MainStoresStockItemsDto> { new() });

            var result = await CreateSut().Handle(
                new GetMainStoreStockItemsQuery { OldUnitcode = "U01", GroupCode = "G01" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMainStoreStockItemsQuery { OldUnitcode = "U01", GroupCode = "G01" },
                CancellationToken.None);
            _mockQueryRepo.Verify(r => r.GetStockItemsCodes("U01", "G01"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMainStoreStockItemsQuery { OldUnitcode = "U01", GroupCode = "G01" },
                CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MainStoresStockItemCodes"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
