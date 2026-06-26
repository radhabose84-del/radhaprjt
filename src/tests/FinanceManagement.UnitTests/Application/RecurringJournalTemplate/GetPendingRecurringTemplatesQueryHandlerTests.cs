using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetPendingRecurringTemplates;
using MediatR;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate
{
    public sealed class GetPendingRecurringTemplatesQueryHandlerTests
    {
        private readonly Mock<IRecurringJournalTemplateQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _workflow = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _userLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private GetPendingRecurringTemplatesQueryHandler CreateSut() =>
            new(_query.Object, _ip.Object, _workflow.Object, _userLookup.Object, _mediator.Object);

        [Fact]
        public async Task Handle_FiltersToCurrentUsersApprovals()
        {
            _ip.Setup(s => s.GetUserId()).Returns(99);
            _query.Setup(r => r.GetPendingApprovalAsync(1, 50))
                .ReturnsAsync((new List<RecurringJournalTemplateHeaderDto> { new() { Id = 10 }, new() { Id = 20 } }, 2));
            _workflow.Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 10, ApproverValue = "99", ApprovalRequestId = 500, IsEdit = 1 },
                    new() { ModuleTransactionId = 20, ApproverValue = "42" }
                });
            _userLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserLookupDto> { new() { UserId = 99, UserName = "Approver Bob" } });

            var result = await CreateSut().Handle(new GetPendingRecurringTemplatesQuery(), CancellationToken.None);

            var row = result.Data.Should().ContainSingle().Which;
            row.Id.Should().Be(10);
            row.ApproverId.Should().Be(99);
            row.ApproverName.Should().Be("Approver Bob");
            row.ApprovalRequestHeaderId.Should().Be(500);
            row.IsEdit.Should().Be(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NoCandidates_ReturnsEmpty()
        {
            _query.Setup(r => r.GetPendingApprovalAsync(1, 50))
                .ReturnsAsync((new List<RecurringJournalTemplateHeaderDto>(), 0));

            var result = await CreateSut().Handle(new GetPendingRecurringTemplatesQuery(), CancellationToken.None);

            result.Data.Should().BeEmpty();
        }
    }
}
