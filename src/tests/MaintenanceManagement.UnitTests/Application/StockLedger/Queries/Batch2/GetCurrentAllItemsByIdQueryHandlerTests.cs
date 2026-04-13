using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentAllItemsById;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.StockLedger.Queries.Batch2
{
    public sealed class GetCurrentAllItemsByIdQueryHandlerTests
    {
        private readonly Mock<IStockLedgerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCurrentAllItemsByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(List<StockItemCodeDto>? dtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetAllItemCodes(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<StockItemCodeDto>());
            _mockMapper
                .Setup(m => m.Map<List<StockItemCodeDto>>(It.IsAny<object>()))
                .Returns(dtos ?? new List<StockItemCodeDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResponse()
        {
            SetupHappyPath(new List<StockItemCodeDto> { new() { ItemCode = "I01", ItemName = "Item1" } });

            var result = await CreateSut().Handle(
                new GetCurrentAllItemsByIdQuery { OldUnitcode = "U01", DepartmentId = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetCurrentAllItemsByIdQuery { OldUnitcode = "U01", DepartmentId = 1 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllItemCodes("U01", 1), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetCurrentAllItemsByIdQuery { OldUnitcode = "U01", DepartmentId = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "StockItemMaster"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
