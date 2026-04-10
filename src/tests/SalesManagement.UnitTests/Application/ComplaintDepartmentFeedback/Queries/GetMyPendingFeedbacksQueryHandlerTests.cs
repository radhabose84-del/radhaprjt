using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetMyPendingFeedbacks;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Queries;

public sealed class GetMyPendingFeedbacksQueryHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetMyPendingFeedbacksQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockIpService.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetUserId()).Returns(42);

        var data = new List<MyPendingFeedbackDto>
        {
            new MyPendingFeedbackDto { AssignmentId = 1 }
        };
        _mockQueryRepo
            .Setup(r => r.GetMyPendingAsync(42))
            .ReturnsAsync(data);

        var result = await CreateSut().Handle(new GetMyPendingFeedbacksQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoPending_ReturnsEmptyList()
    {
        _mockIpService.Setup(s => s.GetUserId()).Returns(42);

        _mockQueryRepo
            .Setup(r => r.GetMyPendingAsync(42))
            .ReturnsAsync(new List<MyPendingFeedbackDto>());

        var result = await CreateSut().Handle(new GetMyPendingFeedbacksQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
