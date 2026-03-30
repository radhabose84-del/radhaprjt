using BudgetManagement.Application.BudgetAllocation.Command.UpsertOnApproval;
using BudgetManagement.Application.BudgetAllocation.Command.Update;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.UnitTests.TestData;

namespace BudgetManagement.UnitTests.Application.BudgetAllocation.Commands
{
    public sealed class UpsertBudgetAllocationOnApprovalCommandHandlerTests
    {
        private readonly Mock<IBudgetRequestCommandRepository> _mockBudgetRequestRepo = new(MockBehavior.Strict);
        private readonly Mock<IBudgetAllocationQueryRepository> _mockAllocationQuery = new(MockBehavior.Strict);
        private readonly Mock<IBudgetAllocationCommandRepository> _mockAllocationCmd = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);

        private UpsertBudgetAllocationOnApprovalCommandHandler CreateSut() =>
            new(_mockBudgetRequestRepo.Object, _mockAllocationQuery.Object, _mockAllocationCmd.Object, _mockMisc.Object);

        [Fact]
        public async Task Handle_NullBudgetRequest_ReturnsFalse()
        {
            _mockBudgetRequestRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetRequest?)null);

            var result = await CreateSut().Handle(
                BudgetAllocationBuilders.ValidUpsertCommand(1), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_BudgetRequestWithNullRequestById_ReturnsFalse()
        {
            var entity = BudgetRequestBuilders.ValidEntity();
            entity.RequestById = null;

            _mockBudgetRequestRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMisc
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscMaster?)null);

            var result = await CreateSut().Handle(
                BudgetAllocationBuilders.ValidUpsertCommand(1), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExistingAllocation_UpdatesRemainingBalance()
        {
            var budgetRequest = BudgetRequestBuilders.ValidEntity();
            budgetRequest.RequestById = 1;
            budgetRequest.RequestAmount = 5000m;
            budgetRequest.FromDate = DateOnly.FromDateTime(DateTime.Today);
            budgetRequest.ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

            var existingAllocation = BudgetAllocationBuilders.ValidEntity();
            existingAllocation.RemainingBalance = 10000m;

            _mockBudgetRequestRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(budgetRequest);

            _mockMisc
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockAllocationCmd
                .Setup(r => r.GetByKeyAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAllocation);

            _mockAllocationCmd
                .Setup(r => r.UpdateRemainingBalanceAsync(existingAllocation.Id, 15000m, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(
                BudgetAllocationBuilders.ValidUpsertCommand(1), CancellationToken.None);

            result.Should().BeTrue();
            _mockAllocationCmd.Verify(
                r => r.UpdateRemainingBalanceAsync(existingAllocation.Id, 15000m, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoExistingAllocation_CreatesNew()
        {
            var budgetRequest = BudgetRequestBuilders.ValidEntity();
            budgetRequest.RequestById = 1;
            budgetRequest.RequestAmount = 5000m;
            budgetRequest.FromDate = DateOnly.FromDateTime(DateTime.Today);
            budgetRequest.ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

            _mockBudgetRequestRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(budgetRequest);

            _mockMisc
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 2 });

            _mockAllocationCmd
                .Setup(r => r.GetByKeyAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetAllocation?)null);

            _mockAllocationCmd
                .Setup(r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetAllocation>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(
                BudgetAllocationBuilders.ValidUpsertCommand(1), CancellationToken.None);

            result.Should().BeTrue();
            _mockAllocationCmd.Verify(
                r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetAllocation>()),
                Times.Once);
        }
    }
}
