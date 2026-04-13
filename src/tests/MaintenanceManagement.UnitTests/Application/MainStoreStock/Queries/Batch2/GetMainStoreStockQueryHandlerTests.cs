using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock;
using MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStock;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MainStoreStock.Queries.Batch2
{
    public sealed class GetMainStoreStockQueryHandlerTests
    {
        private readonly Mock<IMainStoreStockQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMainStoreStockQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(List<MainStoresStockDto>? dtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetStockDetails(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MainStoresStockDto>());
            _mockMapper
                .Setup(m => m.Map<List<MainStoresStockDto>>(It.IsAny<object>()))
                .Returns(dtos ?? new List<MainStoresStockDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            SetupHappyPath(new List<MainStoresStockDto> { new() });

            var result = await CreateSut().Handle(
                new GetMainStoreStockQuery { OldUnitcode = "U01", GroupCode = "G01" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMainStoreStockQuery { OldUnitcode = "U01", GroupCode = "G01" },
                CancellationToken.None);
            _mockQueryRepo.Verify(r => r.GetStockDetails("U01", "G01"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMainStoreStockQuery { OldUnitcode = "U01", GroupCode = "G01" },
                CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MainStoresStock"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
