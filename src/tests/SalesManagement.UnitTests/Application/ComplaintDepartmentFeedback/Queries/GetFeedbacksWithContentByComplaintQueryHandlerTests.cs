using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksWithContentByComplaint;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Queries;

public sealed class GetFeedbacksWithContentByComplaintQueryHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetFeedbacksWithContentByComplaintQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsAllAssignments_WithSubmittedAndPendingMix()
    {
        var data = new List<ComplaintFeedbackFullDto>
        {
            new() { AssignmentId = 1, FeedbackId = 100, IsMandatory = true, RootCauseText = "Yarn defect" },
            new() { AssignmentId = 2, FeedbackId = null, IsMandatory = true }   // Pending
        };
        _mockQueryRepo.Setup(r => r.GetByComplaintIdWithContentAsync(42)).ReturnsAsync(data);

        var result = await CreateSut().Handle(
            new GetFeedbacksWithContentByComplaintQuery(42), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data!.Single(d => d.AssignmentId == 2).FeedbackId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoAssignments_ReturnsEmpty()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintIdWithContentAsync(99))
            .ReturnsAsync(new List<ComplaintFeedbackFullDto>());

        var result = await CreateSut().Handle(
            new GetFeedbacksWithContentByComplaintQuery(99), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
