using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.StockLedger.Queries.Batch2
{
    public sealed class GetCurrentStockQueryHandlerTests
    {
        private readonly Mock<IStockLedgerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCurrentStockQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WhenRepoReturnsData_ReturnsSuccess()
        {
            var dto = new CurrentStockDto { ItemCode = "I01", ItemName = "Item1" };
            _mockQueryRepo
                .Setup(r => r.GetSubStoresCurrentStock(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<CurrentStockDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCurrentStockQuery { OldUnitId = "U01", ItemCode = "I01", DepartmentId = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WhenRepoReturnsNull_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetSubStoresCurrentStock(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((CurrentStockDto?)null);

            var result = await CreateSut().Handle(
                new GetCurrentStockQuery { OldUnitId = "U01", ItemCode = "I01", DepartmentId = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WhenRepoReturnsData_PublishesAuditEvent()
        {
            var dto = new CurrentStockDto { ItemCode = "I01" };
            _mockQueryRepo
                .Setup(r => r.GetSubStoresCurrentStock(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<CurrentStockDto>(It.IsAny<object>())).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetCurrentStockQuery { OldUnitId = "U01", ItemCode = "I01", DepartmentId = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "SubStoresStock"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
