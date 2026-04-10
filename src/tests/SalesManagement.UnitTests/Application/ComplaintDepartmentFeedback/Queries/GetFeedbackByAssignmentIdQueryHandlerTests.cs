using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackByAssignment;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Queries;

public sealed class GetFeedbackByAssignmentIdQueryHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetFeedbackByAssignmentIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingAssignment_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetByAssignmentIdAsync(1))
            .ReturnsAsync(new ComplaintDepartmentFeedbackDto { Id = 1, AssignmentId = 1 });

        var result = await CreateSut().Handle(
            new GetFeedbackByAssignmentIdQuery { AssignmentId = 1 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NoFeedback_ReturnsNotSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetByAssignmentIdAsync(99))
            .ReturnsAsync((ComplaintDepartmentFeedbackDto?)null);

        var result = await CreateSut().Handle(
            new GetFeedbackByAssignmentIdQuery { AssignmentId = 99 }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("No feedback found for this assignment.");
    }
}
