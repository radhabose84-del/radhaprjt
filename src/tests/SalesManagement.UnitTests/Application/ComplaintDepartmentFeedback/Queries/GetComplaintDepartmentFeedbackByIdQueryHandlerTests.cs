using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackById;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Queries;

public sealed class GetComplaintDepartmentFeedbackByIdQueryHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetComplaintDepartmentFeedbackByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new ComplaintDepartmentFeedbackDto { Id = 1 });

        var result = await CreateSut().Handle(
            new GetComplaintDepartmentFeedbackByIdQuery { Id = 1 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNotSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((ComplaintDepartmentFeedbackDto?)null);

        var result = await CreateSut().Handle(
            new GetComplaintDepartmentFeedbackByIdQuery { Id = 99 }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Feedback not found.");
    }
}
