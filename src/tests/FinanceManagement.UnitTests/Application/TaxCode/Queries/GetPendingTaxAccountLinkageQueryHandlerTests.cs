using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Application.TaxCode.Queries.GetPendingTaxAccountLinkage;

namespace FinanceManagement.UnitTests.Application.TaxCode.Queries
{
    public sealed class GetPendingTaxAccountLinkageQueryHandlerTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Strict);
        private readonly Mock<IUserLookup> _mockUser = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private const int CurrentUserId = 7;

        public GetPendingTaxAccountLinkageQueryHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(CurrentUserId);
        }

        private GetPendingTaxAccountLinkageQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockWorkflow.Object, _mockUser.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ShowsOnlyRows_CurrentUserCanApprove_AndResolvesApproverName()
        {
            var data = new List<PendingTaxAccountLinkageDto>
            {
                new() { Id = 3, GlAccountId = 412, OldTaxCode = "GST-IN-5", NewTaxCode = "GST-IN-12", Status = "PENDING" },
                new() { Id = 4, GlAccountId = 500, OldTaxCode = "GST-OUT-5", NewTaxCode = "GST-OUT-12", Status = "PENDING" }
            };
            _mockQueryRepo.Setup(r => r.GetPendingLinkagesAsync(1, 10, null, 1)).ReturnsAsync((data, 2));

            // Row 3 is assigned to the current user (7); row 4 to someone else (9).
            _mockWorkflow.Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 3, ApproverValue = "7", Status = "Pending" },
                    new() { ModuleTransactionId = 4, ApproverValue = "9", Status = "Pending" }
                });

            _mockUser.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserLookupDto> { new() { UserId = 7, UserName = "Priya Sharma" } });

            var result = await CreateSut().Handle(new GetPendingTaxAccountLinkageQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);                       // only the row the current user can approve
            result.Data![0].Id.Should().Be(3);
            result.Data[0].Approver1Name.Should().Be("Priya Sharma");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NoRowsForCurrentUser_ReturnsEmpty()
        {
            var data = new List<PendingTaxAccountLinkageDto>
            {
                new() { Id = 3, GlAccountId = 412, Status = "PENDING" }
            };
            _mockQueryRepo.Setup(r => r.GetPendingLinkagesAsync(1, 10, null, 1)).ReturnsAsync((data, 1));
            _mockWorkflow.Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 3, ApproverValue = "99", Status = "Pending" }   // not the current user
                });

            var result = await CreateSut().Handle(new GetPendingTaxAccountLinkageQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
