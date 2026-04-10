using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetPendingComplaint;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetPendingComplaintQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetPendingComplaintQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object,
            _mockWorkflowLookup.Object,
            _mockUserLookup.Object,
            _mockIpAddressService.Object,
            _mockMediator.Object);

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyTuple()
    {
        _mockQueryRepo
            .Setup(r => r.GetPendingComplaintsAsync(1, 10, null))
            .ReturnsAsync((new List<PendingComplaintListDto>(), 0));

        var (items, total) = await CreateSut().Handle(
            new GetPendingComplaintQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNoApproverMatch_FiltersAllAndReturnsEmpty()
    {
        var pending = new List<PendingComplaintListDto>
        {
            new PendingComplaintListDto { Id = 1 }
        };
        _mockQueryRepo
            .Setup(r => r.GetPendingComplaintsAsync(1, 10, null))
            .ReturnsAsync((pending, 1));

        _mockIpAddressService.Setup(s => s.GetUserId()).Returns(99);
        _mockWorkflowLookup
            .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Contracts.Dtos.Workflow.ApproverListDto>());

        var (items, total) = await CreateSut().Handle(
            new GetPendingComplaintQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }
}
