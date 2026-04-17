using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetPendingQCReview;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetPendingQCReviewQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetPendingQCReviewQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockWorkflowLookup.Object,
            _mockUserLookup.Object, _mockIpService.Object, _mockMediator.Object);

    private void SetupWorkflowAndUsers(int userId, int complaintHeaderId)
    {
        _mockIpService.Setup(s => s.GetUserId()).Returns(userId);

        _mockWorkflowLookup
            .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ApproverListDto>
            {
                new ApproverListDto
                {
                    ModuleTransactionId = complaintHeaderId,
                    ApproverValue = userId.ToString(),
                    ApprovalRequestId = 100,
                    IsEdit = 1
                }
            });

        _mockUserLookup.Setup(u => u.GetAllUserAsync())
            .ReturnsAsync(new List<UserLookupDto>
            {
                new UserLookupDto { UserId = userId, UserName = "TestUser" }
            });
    }

    [Fact]
    public async Task Handle_EmptyPendingList_ReturnsEmptyWithZeroCount()
    {
        _mockRepo
            .Setup(r => r.GetPendingQCReviewAsync(1, 10, null))
            .ReturnsAsync((new List<PendingQCReviewListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetPendingQCReviewQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithData_FiltersToCurrentUser()
    {
        var pending = new List<PendingQCReviewListDto>
        {
            new PendingQCReviewListDto { Id = 1, ComplaintHeaderId = 10, ComplaintNumber = "CMP001" },
            new PendingQCReviewListDto { Id = 2, ComplaintHeaderId = 20, ComplaintNumber = "CMP002" }
        };

        _mockRepo
            .Setup(r => r.GetPendingQCReviewAsync(1, 10, null))
            .ReturnsAsync((pending, 2));

        // Only approve complaint 10 for user 5
        _mockIpService.Setup(s => s.GetUserId()).Returns(5);
        _mockWorkflowLookup
            .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ApproverListDto>
            {
                new ApproverListDto
                {
                    ModuleTransactionId = 10,
                    ApproverValue = "5",
                    ApprovalRequestId = 100,
                    IsEdit = 1
                }
            });

        _mockUserLookup.Setup(u => u.GetAllUserAsync())
            .ReturnsAsync(new List<UserLookupDto>
            {
                new UserLookupDto { UserId = 5, UserName = "Approver" }
            });

        var result = await CreateSut().Handle(
            new GetPendingQCReviewQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].ComplaintHeaderId.Should().Be(10);
    }

    [Fact]
    public async Task Handle_NoMatchingApprover_ReturnsEmpty()
    {
        var pending = new List<PendingQCReviewListDto>
        {
            new PendingQCReviewListDto { Id = 1, ComplaintHeaderId = 10 }
        };

        _mockRepo
            .Setup(r => r.GetPendingQCReviewAsync(1, 10, null))
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
                    ApprovalRequestId = 100,
                    IsEdit = 0
                }
            });

        var result = await CreateSut().Handle(
            new GetPendingQCReviewQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockRepo
            .Setup(r => r.GetPendingQCReviewAsync(1, 10, null))
            .ReturnsAsync((new List<PendingQCReviewListDto>(), 0));

        await CreateSut().Handle(
            new GetPendingQCReviewQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionName == "QCReviewPending"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
