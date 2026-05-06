using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using AutoMapper;
using Contracts.Interfaces.Lookups.Budget;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Update;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ImportPO.Commands
{
    public sealed class UpdateImportPOCommandHandlerTests
    {
        private readonly Mock<IImportPOCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockPoDocs = new(MockBehavior.Loose);
        private readonly Mock<IBudgetAllocationLookup> _mockBudgetLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateImportPOCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateImportPOCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockPoDocs.Object,
                _mockBudgetLookup.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsValidationException()
        {
            var command = new UpdateImportPOCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Body required*");
        }

        [Fact]
        public void Constructor_NullBudgetLookup_ThrowsArgumentNullException()
        {
            var act = () => new UpdateImportPOCommandHandler(
                _mockRepo.Object, _mockMapper.Object, _mockPoDocs.Object,
                null!, _mockLogger.Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("budgetAllocationLookup");
        }

        [Fact]
        public async Task Handle_SameBudgetGroup_PassesDeltaEqualToOldMinusNewValue()
        {
            // PO value goes from 100 → 250 in the same budget group.
            // Net change in commitment is +150 (more consumed), so the delta passed
            // to the lookup must be -150 (RemainingBalance decreases by 150).
            var captured = SetupAndCaptureSingleDelta(
                existing: new PurchaseOrderHeader
                {
                    Id = 5, BudgetGroupId = 1, PurchaseValue = 100m,
                    PODate = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)
                },
                incoming: new PurchaseOrderHeader
                {
                    Id = 5, BudgetGroupId = 1, PurchaseValue = 250m,
                    PODate = new DateTimeOffset(2026, 4, 15, 0, 0, 0, TimeSpan.Zero)
                });

            await CreateSut().Handle(new UpdateImportPOCommand { Data = MakeUpdateDto(5) }, CancellationToken.None);

            captured.Calls.Should().HaveCount(1);
            captured.Calls[0].Delta.Should().Be(-150m,
                "increasing PO value from 100 to 250 commits 150 more, so RemainingBalance must decrease by 150");
        }

        [Fact]
        public async Task Handle_DifferentBudgetGroup_RefundsOldGroupAndConsumesNewGroup()
        {
            // PO value 200 moved from BG=1 → BG=2. The old group must be refunded
            // (+200) and the new group consumed (-200).
            var captured = SetupAndCaptureSingleDelta(
                existing: new PurchaseOrderHeader
                {
                    Id = 9, BudgetGroupId = 1, PurchaseValue = 200m,
                    PODate = new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero)
                },
                incoming: new PurchaseOrderHeader
                {
                    Id = 9, BudgetGroupId = 2, PurchaseValue = 200m,
                    PODate = new DateTimeOffset(2026, 3, 20, 0, 0, 0, TimeSpan.Zero)
                });

            await CreateSut().Handle(new UpdateImportPOCommand { Data = MakeUpdateDto(9) }, CancellationToken.None);

            captured.Calls.Should().HaveCount(2);
            captured.Calls[0].BudgetGroupId.Should().Be(1);
            captured.Calls[0].Delta.Should().Be(+200m, "refund of old group should INCREASE RemainingBalance");
            captured.Calls[1].BudgetGroupId.Should().Be(2);
            captured.Calls[1].Delta.Should().Be(-200m, "consumption on new group should DECREASE RemainingBalance");
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private sealed record CapturedCall(int BudgetGroupId, decimal Delta);

        private sealed class CapturedCalls
        {
            public List<CapturedCall> Calls { get; } = new();
        }

        private CapturedCalls SetupAndCaptureSingleDelta(
            PurchaseOrderHeader existing,
            PurchaseOrderHeader incoming)
        {
            var captured = new CapturedCalls();

            _mockRepo.Setup(r => r.GetAggregateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockMapper.Setup(m => m.Map<PurchaseOrderHeader>(It.IsAny<ImportPOUpdateDto>()))
                .Returns(incoming);

            _mockRepo.Setup(r => r.CreateExecutionStrategy()).Returns(new ImmediateExecutionStrategy());

            var mockEfTx = new Mock<IDbContextTransaction>(MockBehavior.Loose);
            var mockConn = new Mock<DbConnection>(MockBehavior.Loose);
            var mockTx = new Mock<DbTransaction>(MockBehavior.Loose);
            _mockRepo.Setup(r => r.BeginTransactionWithConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((mockEfTx.Object, mockConn.Object, mockTx.Object));

            _mockRepo.Setup(r => r.UpdateWithoutTransactionAsync(
                    It.IsAny<PurchaseOrderHeader>(), It.IsAny<ImportPOUpdateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(incoming.Id);

            _mockBudgetLookup.Setup(b => b.ApplyRemainingBalanceDeltaAsync(
                    It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<decimal>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Callback<int, DateOnly, int, int, decimal, int?, int?, int?, DbConnection, DbTransaction, CancellationToken>(
                    (bg, _, _, _, delta, _, _, _, _, _, _) => captured.Calls.Add(new CapturedCall(bg, delta)))
                .ReturnsAsync(true);

            return captured;
        }

        private static ImportPOUpdateDto MakeUpdateDto(int id) =>
            new() { Id = id, PONumber = "IMP-001" };
    }
}
