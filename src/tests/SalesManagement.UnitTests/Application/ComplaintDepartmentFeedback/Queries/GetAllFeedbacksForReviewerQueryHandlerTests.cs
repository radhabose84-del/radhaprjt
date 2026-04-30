using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedbacksForReviewer;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Queries;

public sealed class GetAllFeedbacksForReviewerQueryHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllFeedbacksForReviewerQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_PassesNullResponsiblePersonId_ToBypassUserFilter()
    {
        var data = new List<FeedbackListDto>
        {
            new() { AssignmentId = 22, FeedbackId = 18, ResponsiblePersonId = 376 },
            new() { AssignmentId = 24, FeedbackId = 14, ResponsiblePersonId = 396 }
        };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, null))
            .ReturnsAsync((data, 2));

        var result = await CreateSut().Handle(
            new GetAllFeedbacksForReviewerQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);

        // Critical: verify the handler sent null for ResponsiblePersonId — that's
        // what bypasses the user-scope filter in the repo SQL.
        _mockQueryRepo.Verify(
            r => r.GetAllAsync(1, 10, null, null, null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PassesSearchAndStatusFilters_Through()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(2, 5, "CMP", "Submitted", null))
            .ReturnsAsync((new List<FeedbackListDto>(), 0));

        await CreateSut().Handle(
            new GetAllFeedbacksForReviewerQuery
            {
                PageNumber = 2, PageSize = 5,
                SearchTerm = "CMP", StatusFilter = "Submitted"
            },
            CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetAllAsync(2, 5, "CMP", "Submitted", null),
            Times.Once);
    }
}
