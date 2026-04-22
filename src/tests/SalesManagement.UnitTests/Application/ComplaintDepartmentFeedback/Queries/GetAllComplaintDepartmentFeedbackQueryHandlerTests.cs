using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedback;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Queries;

public sealed class GetAllComplaintDepartmentFeedbackQueryHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllComplaintDepartmentFeedbackQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockIpService.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetUserId()).Returns(7);
        var dtoList = new List<FeedbackListDto> { new FeedbackListDto { AssignmentId = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, 7))
            .ReturnsAsync((dtoList, 1));

        var result = await CreateSut().Handle(
            new GetAllComplaintDepartmentFeedbackQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ScopesToLoggedInUser_PassesUserId()
    {
        _mockIpService.Setup(s => s.GetUserId()).Returns(42);

        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, 42))
            .ReturnsAsync((new List<FeedbackListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllComplaintDepartmentFeedbackQuery
            {
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockQueryRepo.Verify(
            r => r.GetAllAsync(1, 10, null, null, 42),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetUserId()).Returns(7);
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, 7))
            .ReturnsAsync((new List<FeedbackListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllComplaintDepartmentFeedbackQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
