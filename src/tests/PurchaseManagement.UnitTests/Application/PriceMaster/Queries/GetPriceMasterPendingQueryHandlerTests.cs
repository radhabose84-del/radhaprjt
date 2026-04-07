using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.PriceMaster.Queries
{
    public sealed class GetPriceMasterPendingQueryHandlerTests
    {
        private readonly Mock<IPriceMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        public GetPriceMasterPendingQueryHandlerTests()
        {
            // Default empty returns for all lookups — handler doesn't null-check results
            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>());
            _mockUomLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());
            _mockCurrencyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());
            _mockUserLookup
                .Setup(l => l.GetAllUserAsync())
                .ReturnsAsync(new List<UserLookupDto>());
            _mockWorkflowLookup
                .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>());
        }

        private GetPriceMasterPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockItemLookup.Object, _mockUomLookup.Object,
                _mockCurrencyLookup.Object, _mockMediator.Object, _mockWorkflowLookup.Object,
                _mockUserLookup.Object, _mockIpService.Object);

        private void SetupCurrentUser(int userId = 1)
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(userId);
        }

        private void SetupEmptyRepo()
        {
            _mockRepo
                .Setup(r => r.GetPriceMasterPendingAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((new List<PriceMasterPendingGroupDto>(), 0));
        }

        private void SetupRepoWithRows(List<PriceMasterPendingGroupDto> rows, int total = 0)
        {
            _mockRepo
                .Setup(r => r.GetPriceMasterPendingAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((rows, total));
        }

        private void SetupWorkflowApprovers(List<ApproverListDto> approvers)
        {
            _mockWorkflowLookup
                .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(approvers);
        }

        private void SetupUsers(List<UserLookupDto> users)
        {
            _mockUserLookup
                .Setup(u => u.GetAllUserAsync())
                .ReturnsAsync(users);
        }

        private static PriceMasterPendingGroupDto BuildPendingRow(int id = 1, int itemId = 10, int uomId = 3)
        {
            return new PriceMasterPendingGroupDto
            {
                Id = id,
                ItemId = itemId,
                VendorId = 20,
                UomId = uomId,
                ValidFrom = new DateOnly(2025, 1, 1),
                CreatedDate = DateTimeOffset.UtcNow,
                Lines = new List<PriceMasterPendingDto>
                {
                    new() { QuantityFrom = 1, QuantityTo = 100, UnitPrice = 50m, CurrencyId = 5 }
                }
            };
        }

        private static ApproverListDto BuildApprover(int moduleTransactionId, int approverId, int approvalRequestId = 100)
        {
            return new ApproverListDto
            {
                ModuleTransactionId = moduleTransactionId,
                ApproverValue = approverId.ToString(),
                ApprovalRequestId = approvalRequestId,
                IsEdit = 0,
                Status = "Pending",
                ApproverBinding = "User"
            };
        }

        // --- Empty result ---

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupCurrentUser(1);
            SetupEmptyRepo();

            var result = await CreateSut().Handle(
                new GetPriceMasterPendingQuery(), CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_EmptyResult_PublishesAuditEvent()
        {
            SetupCurrentUser(1);
            SetupEmptyRepo();

            await CreateSut().Handle(new GetPriceMasterPendingQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- Workflow filtering ---

        [Fact]
        public async Task Handle_FiltersRowsByCurrentUserApprovalRights()
        {
            SetupCurrentUser(42);
            var rows = new List<PriceMasterPendingGroupDto>
            {
                BuildPendingRow(id: 1),
                BuildPendingRow(id: 2)
            };
            SetupRepoWithRows(rows, 2);

            // Only row 1 has user 42 as approver
            SetupWorkflowApprovers(new List<ApproverListDto>
            {
                BuildApprover(moduleTransactionId: 1, approverId: 42),
                BuildApprover(moduleTransactionId: 2, approverId: 99)
            });
            SetupUsers(new List<UserLookupDto>
            {
                new() { UserId = 42, UserName = "TestUser" }
            });

            var result = await CreateSut().Handle(
                new GetPriceMasterPendingQuery(), CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.Items[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NoApprovalRights_ReturnsEmpty()
        {
            SetupCurrentUser(42);
            SetupRepoWithRows(new List<PriceMasterPendingGroupDto> { BuildPendingRow(id: 1) }, 1);
            SetupWorkflowApprovers(new List<ApproverListDto>
            {
                BuildApprover(moduleTransactionId: 1, approverId: 99) // different user
            });

            var result = await CreateSut().Handle(
                new GetPriceMasterPendingQuery(), CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        // --- Enrichment ---

        [Fact]
        public async Task Handle_EnrichesItemName()
        {
            SetupCurrentUser(1);
            SetupRepoWithRows(new List<PriceMasterPendingGroupDto> { BuildPendingRow(id: 1, itemId: 10) }, 1);
            SetupWorkflowApprovers(new List<ApproverListDto> { BuildApprover(1, 1) });
            SetupUsers(new List<UserLookupDto> { new() { UserId = 1, UserName = "Admin" } });

            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 10, ItemName = "Widget", ItemCode = "W001" } });

            var result = await CreateSut().Handle(
                new GetPriceMasterPendingQuery(), CancellationToken.None);

            result.Items[0].ItemName.Should().Be("Widget");
        }

        [Fact]
        public async Task Handle_EnrichesUomName()
        {
            SetupCurrentUser(1);
            SetupRepoWithRows(new List<PriceMasterPendingGroupDto> { BuildPendingRow(id: 1, uomId: 3) }, 1);
            SetupWorkflowApprovers(new List<ApproverListDto> { BuildApprover(1, 1) });
            SetupUsers(new List<UserLookupDto> { new() { UserId = 1, UserName = "Admin" } });

            _mockUomLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new() { Id = 3, UOMName = "Kilogram", Code = "KG" } });

            var result = await CreateSut().Handle(
                new GetPriceMasterPendingQuery(), CancellationToken.None);

            result.Items[0].UOM.Should().Be("Kilogram");
        }

        [Fact]
        public async Task Handle_EnrichesCurrencyOnLines()
        {
            SetupCurrentUser(1);
            SetupRepoWithRows(new List<PriceMasterPendingGroupDto> { BuildPendingRow(id: 1) }, 1);
            SetupWorkflowApprovers(new List<ApproverListDto> { BuildApprover(1, 1) });
            SetupUsers(new List<UserLookupDto> { new() { UserId = 1, UserName = "Admin" } });

            _mockCurrencyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto> { new() { CurrencyId = 5, Code = "EUR" } });

            var result = await CreateSut().Handle(
                new GetPriceMasterPendingQuery(), CancellationToken.None);

            result.Items[0].Lines[0].CurrencyName.Should().Be("EUR");
        }

        [Fact]
        public async Task Handle_EnrichesApproverName()
        {
            SetupCurrentUser(1);
            SetupRepoWithRows(new List<PriceMasterPendingGroupDto> { BuildPendingRow(id: 1) }, 1);
            SetupWorkflowApprovers(new List<ApproverListDto> { BuildApprover(1, 1, approvalRequestId: 200) });
            SetupUsers(new List<UserLookupDto> { new() { UserId = 1, UserName = "John Doe" } });

            var result = await CreateSut().Handle(
                new GetPriceMasterPendingQuery(), CancellationToken.None);

            result.Items[0].ApproverId.Should().Be(1);
            result.Items[0].ApproverName.Should().Be("John Doe");
            result.Items[0].ApprovalRequestHeaderId.Should().Be(200);
        }

        // --- Audit ---

        [Fact]
        public async Task Handle_WithData_PublishesAuditEventOnce()
        {
            SetupCurrentUser(1);
            SetupRepoWithRows(new List<PriceMasterPendingGroupDto> { BuildPendingRow(id: 1) }, 1);
            SetupWorkflowApprovers(new List<ApproverListDto> { BuildApprover(1, 1) });
            SetupUsers(new List<UserLookupDto> { new() { UserId = 1, UserName = "Admin" } });

            await CreateSut().Handle(new GetPriceMasterPendingQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetAll-Pending"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
