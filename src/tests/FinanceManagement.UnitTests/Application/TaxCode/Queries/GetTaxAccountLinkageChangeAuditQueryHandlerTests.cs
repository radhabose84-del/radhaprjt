using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageChangeAudit;

namespace FinanceManagement.UnitTests.Application.TaxCode.Queries
{
    public sealed class GetTaxAccountLinkageChangeAuditQueryHandlerTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Strict);
        private readonly Mock<IUserLookup> _mockUser = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        public GetTaxAccountLinkageChangeAuditQueryHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        private GetTaxAccountLinkageChangeAuditQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockWorkflow.Object, _mockUser.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsAllChangeRows_AndResolvesBothApprovers()
        {
            var data = new List<PendingTaxAccountLinkageDto>
            {
                new() { Id = 1, GlAccountId = 412, OldTaxCode = "GST-IN-5",  NewTaxCode = "GST-IN-12",  Status = "APPROVED" },
                new() { Id = 3, GlAccountId = 101, OldTaxCode = "GST-IN-5",  NewTaxCode = "GST-IN-12",  Status = "PENDING" }
            };
            _mockQueryRepo.Setup(r => r.GetChangeAuditLinkagesAsync(1, 20, null, 1, null)).ReturnsAsync((data, 2));

            // Row 1 (approved) has both approvers; row 3 (pending) has none assigned yet → stays "—".
            _mockWorkflow.Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 1, ApproverValue = "7", Status = "Approved" },
                    new() { ModuleTransactionId = 1, ApproverValue = "8", Status = "Approved" }
                });

            _mockUser.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserLookupDto>
                {
                    new() { UserId = 7, UserName = "Priya Sharma" },
                    new() { UserId = 8, UserName = "Vikram Mehta" }
                });

            var result = await CreateSut().Handle(
                new GetTaxAccountLinkageChangeAuditQuery { PageNumber = 1, PageSize = 20 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);                       // ALL change rows, not filtered to current user
            result.TotalCount.Should().Be(2);

            var approved = result.Data!.Single(r => r.Id == 1);
            approved.Approver1Name.Should().Be("Priya Sharma");
            approved.Approver2Name.Should().Be("Vikram Mehta");

            var pending = result.Data!.Single(r => r.Id == 3);
            pending.Approver1Name.Should().BeNull();                  // "—" while pending
            pending.Approver2Name.Should().BeNull();
        }

        [Fact]
        public async Task Handle_EmptyResult_SkipsApproverLookups()
        {
            _mockQueryRepo.Setup(r => r.GetChangeAuditLinkagesAsync(1, 20, null, 1, 2))
                .ReturnsAsync((new List<PendingTaxAccountLinkageDto>(), 0));

            var result = await CreateSut().Handle(
                new GetTaxAccountLinkageChangeAuditQuery { PageNumber = 1, PageSize = 20, StatusId = 2 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            _mockWorkflow.Verify(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()), Times.Never);
        }
    }
}
