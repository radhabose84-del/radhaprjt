using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksByComplaint;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Queries;

public sealed class GetFeedbacksByComplaintIdQueryHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetFeedbacksByComplaintIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var data = new List<FeedbackListDto> { new FeedbackListDto { AssignmentId = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetByComplaintIdAsync(1))
            .ReturnsAsync(data);

        var result = await CreateSut().Handle(
            new GetFeedbacksByComplaintIdQuery { ComplaintHeaderId = 1 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetByComplaintIdAsync(99))
            .ReturnsAsync(new List<FeedbackListDto>());

        var result = await CreateSut().Handle(
            new GetFeedbacksByComplaintIdQuery { ComplaintHeaderId = 99 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
