using System.Data.Common;
using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ImportPO.Commands
{
    public sealed class CreateImportPOCommandHandlerTests
    {
        private readonly Mock<IImportPOCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateImportPOCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockPoDocs = new(MockBehavior.Loose);
        private readonly Mock<IImportPOQueryRepository> _mockImportQuery = new(MockBehavior.Loose);
        private readonly Mock<IBudgetAllocationLookup> _mockBudgetLookup = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSequence = new(MockBehavior.Loose);

        private CreateImportPOCommandHandler CreateSut() =>
            new(
                _mockRepo.Object, _mockMapper.Object, _mockIp.Object, _mockTz.Object,
                _mockLogger.Object, _mockMisc.Object, _mockPoDocs.Object,
                _mockImportQuery.Object, _mockBudgetLookup.Object,
                _mockDocSequence.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsNullReferenceException()
        {
            var command = new CreateImportPOCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public void Constructor_NullBudgetLookup_ThrowsArgumentNullException()
        {
            var act = () => new CreateImportPOCommandHandler(
                _mockRepo.Object, _mockMapper.Object, _mockIp.Object, _mockTz.Object,
                _mockLogger.Object, _mockMisc.Object, _mockPoDocs.Object,
                _mockImportQuery.Object, null!, _mockDocSequence.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("budgetAllocationLookup");
        }

        [Fact]
        public async Task Handle_ConsumesBudget_PassesNegativeDeltaEqualToPurchaseValue()
        {
            // Arrange — capture the deltaAmount the handler passes to the budget lookup.
            // The lookup SQL is `RemainingBalance += @DeltaAmount`, so a PO must pass a
            // negative delta to consume budget. Asserting the sign here protects against
            // regression of the fix that flipped the sign on this handler.
            const int unitId = 1;
            const int budgetGroupId = 7;
            const decimal purchaseValue = 1000m;

            var dto = new ImportPOCreateDto
            {
                UnitId = unitId,
                PODate = new DateTimeOffset(2026, 4, 15, 0, 0, 0, TimeSpan.Zero),
                BudgetGroupId = budgetGroupId,
                PurchaseValue = purchaseValue,
                FinancialYearId = 1
            };

            var entity = new PurchaseOrderHeader
            {
                BudgetGroupId = budgetGroupId,
                PurchaseValue = purchaseValue,
                PODate = dto.PODate,
                FinancialYearId = dto.FinancialYearId
            };

            _mockMapper.Setup(m => m.Map<PurchaseOrderHeader>(dto)).Returns(entity);
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("UTC");
            _mockIp.Setup(i => i.GetUnitId()).Returns(unitId);
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            _mockDocSequence.Setup(d => d.GetTransactionTypeIdAsync("CombinePO", "Purchase", unitId))
                .ReturnsAsync(1);
            _mockDocSequence.Setup(d => d.GenerateDocumentNumber(1))
                .ReturnsAsync(new List<string> { "IMP-001" });

            _mockRepo.Setup(r => r.CreateExecutionStrategy()).Returns(new ImmediateExecutionStrategy());

            var mockEfTx = new Mock<IDbContextTransaction>(MockBehavior.Loose);
            var mockConn = new Mock<DbConnection>(MockBehavior.Loose);
            var mockTx = new Mock<DbTransaction>(MockBehavior.Loose);
            _mockRepo.Setup(r => r.BeginTransactionWithConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((mockEfTx.Object, mockConn.Object, mockTx.Object));

            _mockRepo.Setup(r => r.CreateWithoutTransactionAsync(
                    It.IsAny<PurchaseOrderHeader>(), It.IsAny<ImportPOCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            decimal capturedDelta = 0m;
            _mockBudgetLookup.Setup(b => b.ApplyRemainingBalanceDeltaAsync(
                    It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<decimal>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Callback<int, DateOnly, int, int, decimal, int?, int?, int?, DbConnection, DbTransaction, CancellationToken>(
                    (_, _, _, _, delta, _, _, _, _, _, _) => capturedDelta = delta)
                .ReturnsAsync(true);

            // Act
            var result = await CreateSut().Handle(new CreateImportPOCommand { Data = dto }, CancellationToken.None);

            // Assert
            result.Should().Be(42);
            capturedDelta.Should().Be(-purchaseValue,
                "a PO commitment must DECREASE RemainingBalance, so the delta must be negative");
        }
    }
}
