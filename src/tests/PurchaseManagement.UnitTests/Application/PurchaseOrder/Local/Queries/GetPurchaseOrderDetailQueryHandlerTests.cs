using Contracts.Dtos.Budget;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderDetail;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Queries
{
    public sealed class GetPurchaseOrderDetailQueryHandlerTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _repo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _partyLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyDetailLookup> _partyDetailLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _currencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _uomLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _itemLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _deptLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _companyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _unitLookup = new(MockBehavior.Loose);
        private readonly Mock<IBudgetAllocationLookup> _budgetLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _workflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _userLookup = new(MockBehavior.Loose);

        private GetPurchaseOrderDetailQueryHandler CreateSut() =>
            new(_repo.Object, _ip.Object, _partyLookup.Object, _partyDetailLookup.Object,
                _currencyLookup.Object, _uomLookup.Object, _itemLookup.Object, _deptLookup.Object,
                _companyLookup.Object, _unitLookup.Object, _budgetLookup.Object,
                _workflowLookup.Object, _userLookup.Object);

        private static PurchaseOrderDetailDto Header() => new()
        {
            Id = 1,
            PONumber = "PO/2025/LOC/0001",
            PODate = new DateTimeOffset(2025, 5, 10, 0, 0, 0, TimeSpan.Zero),
            VendorId = 10,
            CurrencyId = 0,
            StatusCode = "Pending",
            POMethodCode = "Local",
            BudgetGroupId = 7,
            BudgetMonthId = 3,
            BudgetRequestById = 2,
        };

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _repo.Setup(r => r.GetDetailByIdAsync(99, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PurchaseOrderDetailDto?)null);

            var result = await CreateSut().Handle(new GetPurchaseOrderDetailQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_BuildsAllSummaryPanels()
        {
            _repo.Setup(r => r.GetDetailByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Header());
            _repo.Setup(r => r.GetInvoicedTotalAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(125000m);
            _repo.Setup(r => r.HasAnyGrnAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _repo.Setup(r => r.HasAnyBillEntryAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _partyLookup.Setup(p => p.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 10, PartyName = "Sri Lakshmi Textiles" });
            _partyDetailLookup.Setup(p => p.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyDetailLookupDto { Id = 10, GSTNumber = "33AABCS1234F1Z2", Phone = "9876543210" });

            _budgetLookup.Setup(b => b.GetAllocationSummaryAsync(
                    7, It.IsAny<DateOnly>(), 3, 2, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BudgetAllocationSummaryDto { ApprovedAmount = 100000m, RemainingBalance = 32000m });

            _workflowLookup.Setup(w => w.GetApproverListAsync("Local Purchase Order", It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 1, ApproverValue = "5", Status = "Awaiting" }
                });
            _userLookup.Setup(u => u.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = 5, UserName = "Suresh Babu" });

            var result = await CreateSut().Handle(new GetPurchaseOrderDetailQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Detail.VendorName.Should().Be("Sri Lakshmi Textiles");
            result.Detail.VendorGSTIN.Should().Be("33AABCS1234F1Z2");

            result.Summary.Outstanding.InvoicedAmount.Should().Be(125000m);

            result.Summary.Budget.ApprovedAmount.Should().Be(100000m);
            result.Summary.Budget.UtilisedPercent.Should().Be(68m);   // (100000-32000)/100000*100
            result.Summary.Budget.IsPositive.Should().BeTrue();
            result.Summary.Budget.HasAllocation.Should().BeTrue();

            result.Summary.Approval.ApproverName.Should().Be("Suresh Babu");
            result.Summary.Approval.Status.Should().Be("Pending");

            result.Summary.Progress.Steps.Should().HaveCount(5);
            result.Summary.Progress.Steps.Single(s => s.Step == "GRN Received").Status.Should().Be("Completed");
            result.Summary.Progress.Steps.Single(s => s.Step == "Invoice Matched").Status.Should().Be("Pending");
            result.Summary.Progress.Steps.Single(s => s.Step == "Payment Done").Status.Should().Be("Pending");
            // Completed: PO Created + GRN Received = 2 of 5 => 40%
            result.Summary.Progress.PercentComplete.Should().Be(40);
        }

        [Fact]
        public async Task Handle_NoBudgetGroup_LeavesBudgetUnpopulated()
        {
            var header = Header();
            header.BudgetGroupId = null;
            _repo.Setup(r => r.GetDetailByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(header);

            var result = await CreateSut().Handle(new GetPurchaseOrderDetailQuery(1), CancellationToken.None);

            result!.Summary.Budget.HasAllocation.Should().BeFalse();
            result.Summary.Budget.UtilisedPercent.Should().BeNull();
        }
    }
}
