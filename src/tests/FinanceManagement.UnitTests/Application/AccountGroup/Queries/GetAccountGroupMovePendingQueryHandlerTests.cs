using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupMovePending;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Queries
{
    public sealed class GetAccountGroupMovePendingQueryHandlerTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IWorkflowLookup> _mockWf = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountGroupMovePendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockWf.Object, _mockIp.Object, _mockMediator.Object);

        private static GetAccountGroupMovePendingQuery Query() =>
            new() { PageNumber = 1, PageSize = 10 };

        private static AccountGroupMovePendingDto Row(int changeRequestId, int accountGroupId, string code) =>
            new() { ChangeRequestId = changeRequestId, AccountGroupId = accountGroupId, GroupCode = code, NewParentAccountGroupId = 5 };

        private static ApproverListDto Wf(int moduleTxnId, string approverValue, int approvalRequestId) =>
            new() { ModuleTransactionId = moduleTxnId, ApproverValue = approverValue, ApprovalRequestId = approvalRequestId, IsEdit = 0 };

        [Fact]
        public async Task Handle_NoPendingRows_ReturnsEmpty_WithoutCallingWorkflow()
        {
            _mockRepo.Setup(r => r.GetMovePendingAsync(1, 10, null))
                .ReturnsAsync((new List<AccountGroupMovePendingDto>(), 0));

            var result = await CreateSut().Handle(Query(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            _mockWf.Verify(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_FiltersToCurrentApprover_AndEnrichesApprovalRequestId()
        {
            _mockIp.Setup(s => s.GetUserId()).Returns(396);   // logged-in approver (FC)
            _mockRepo.Setup(r => r.GetMovePendingAsync(1, 10, null))
                .ReturnsAsync((new List<AccountGroupMovePendingDto> { Row(11, 3, "A-CA-INV"), Row(12, 4, "A-NCA-PPE") }, 2));

            // Group 3 is pending with me (396); group 4 is pending with someone else (999).
            _mockWf.Setup(w => w.GetApproverListAsync("Account Group Hierarchy", It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto> { Wf(3, "396", 50), Wf(4, "999", 51) });

            var result = await CreateSut().Handle(Query(), CancellationToken.None);

            result.Data.Should().ContainSingle();
            var row = result.Data![0];
            row.AccountGroupId.Should().Be(3);
            row.ApproverId.Should().Be(396);
            row.ApprovalRequestHeaderId.Should().Be(50);   // needed by the approve/reject endpoint
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo.Setup(r => r.GetMovePendingAsync(1, 10, null))
                .ReturnsAsync((new List<AccountGroupMovePendingDto>(), 0));

            await CreateSut().Handle(Query(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
