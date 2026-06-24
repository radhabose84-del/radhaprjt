using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetPendingApprovalJournals;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class GetPendingApprovalJournalsQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _workflow = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _userLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private GetPendingApprovalJournalsQueryHandler CreateSut() =>
            new(_query.Object, _ip.Object, _workflow.Object, _userLookup.Object, _mediator.Object);

        [Fact]
        public async Task Handle_FiltersToCurrentUserApprovals_AndSetsApproverName()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            _ip.Setup(s => s.GetUserId()).Returns(99);

            _query.Setup(r => r.GetPendingApprovalAsync(1, 50, 1))
                .ReturnsAsync((new List<JournalListItemDto> { new() { Id = 10 }, new() { Id = 20 } }, 2));

            // Voucher 10 → current user (99) is the approver; voucher 20 → someone else (42).
            _workflow.Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 10, ApproverValue = "99" },
                    new() { ModuleTransactionId = 20, ApproverValue = "42" }
                });

            _userLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserLookupDto> { new() { UserId = 99, UserName = "Approver Alice" } });

            var result = await CreateSut().Handle(new GetPendingApprovalJournalsQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().ContainSingle().Which.Id.Should().Be(10);
            result.Data![0].ApproverName.Should().Be("Approver Alice");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NoCandidates_ReturnsEmpty()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            _query.Setup(r => r.GetPendingApprovalAsync(1, 50, 1))
                .ReturnsAsync((new List<JournalListItemDto>(), 0));

            var result = await CreateSut().Handle(new GetPendingApprovalJournalsQuery(), CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
