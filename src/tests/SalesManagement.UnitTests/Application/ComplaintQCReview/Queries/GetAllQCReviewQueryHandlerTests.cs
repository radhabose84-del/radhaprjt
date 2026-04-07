using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Application.ComplaintQCReview.Queries.GetAllQCReview;

namespace SalesManagement.UnitTests.Application.ComplaintQCReview.Queries;

public sealed class GetAllQCReviewQueryHandlerTests
{
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllQCReviewQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var dtoList = new List<QCReviewListDto> { new QCReviewListDto { Id = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null))
            .ReturnsAsync((dtoList, 1));

        var result = await CreateSut().Handle(
            new GetAllQCReviewQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReturnsPaginationMetadata()
    {
        var dtoList = new List<QCReviewListDto> { new QCReviewListDto { Id = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(2, 5, "search", "Pending"))
            .ReturnsAsync((dtoList, 11));

        var result = await CreateSut().Handle(
            new GetAllQCReviewQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search", StatusFilter = "Pending" },
            CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(11);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null))
            .ReturnsAsync((new List<QCReviewListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllQCReviewQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
