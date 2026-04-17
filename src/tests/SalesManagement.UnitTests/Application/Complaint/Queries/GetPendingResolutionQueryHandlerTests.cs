using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetPendingResolution;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetPendingResolutionQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetPendingResolutionQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockWorkflowLookup.Object,
            _mockUserLookup.Object, _mockIpService.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_EmptyPendingList_ReturnsEmptyWithZeroCount()
    {
        _mockRepo
            .Setup(r => r.GetPendingResolutionAsync(1, 10, null))
            .ReturnsAsync((new List<PendingResolutionListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetPendingResolutionQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithData_FiltersToCurrentUser()
    {
        var pending = new List<PendingResolutionListDto>
        {
            new PendingResolutionListDto { Id = 1, ComplaintHeaderId = 10, ComplaintNumber = "CMP001" },
            new PendingResolutionListDto { Id = 2, ComplaintHeaderId = 20, ComplaintNumber = "CMP002" }
        };

        _mockRepo
            .Setup(r => r.GetPendingResolutionAsync(1, 10, null))
            .ReturnsAsync((pending, 2));

        _mockIpService.Setup(s => s.GetUserId()).Returns(5);
        _mockWorkflowLookup
            .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ApproverListDto>
            {
                new ApproverListDto
                {
                    ModuleTransactionId = 10,
                    ApproverValue = "5",
                    ApprovalRequestId = 200,
                    IsEdit = 1
                }
            });

        _mockUserLookup.Setup(u => u.GetAllUserAsync())
            .ReturnsAsync(new List<UserLookupDto>
            {
                new UserLookupDto { UserId = 5, UserName = "Resolver" }
            });

        var result = await CreateSut().Handle(
            new GetPendingResolutionQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].ComplaintHeaderId.Should().Be(10);
    }

    [Fact]
    public async Task Handle_NoMatchingApprover_ReturnsEmpty()
    {
        var pending = new List<PendingResolutionListDto>
        {
            new PendingResolutionListDto { Id = 1, ComplaintHeaderId = 10 }
        };

        _mockRepo
            .Setup(r => r.GetPendingResolutionAsync(1, 10, null))
            .ReturnsAsync((pending, 1));

        _mockIpService.Setup(s => s.GetUserId()).Returns(999);
        _mockWorkflowLookup
            .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ApproverListDto>
            {
                new ApproverListDto
                {
                    ModuleTransactionId = 10,
                    ApproverValue = "5",
                    ApprovalRequestId = 200,
                    IsEdit = 0
                }
            });

        var result = await CreateSut().Handle(
            new GetPendingResolutionQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockRepo
            .Setup(r => r.GetPendingResolutionAsync(1, 10, null))
            .ReturnsAsync((new List<PendingResolutionListDto>(), 0));

        await CreateSut().Handle(
            new GetPendingResolutionQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionName == "ResolutionPending"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
