using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Application.ComplaintResolution.Queries.GetAllResolution;

namespace SalesManagement.UnitTests.Application.ComplaintResolution.Queries;

public sealed class GetAllResolutionQueryHandlerTests
{
    private readonly Mock<IComplaintResolutionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllResolutionQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var dtoList = new List<ResolutionListDto> { new ResolutionListDto { ComplaintHeaderId = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null))
            .ReturnsAsync((dtoList, 1));

        var result = await CreateSut().Handle(
            new GetAllResolutionQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReturnsPaginationMetadata()
    {
        var dtoList = new List<ResolutionListDto> { new ResolutionListDto { ComplaintHeaderId = 1 } };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(2, 5, "search", "Open"))
            .ReturnsAsync((dtoList, 11));

        var result = await CreateSut().Handle(
            new GetAllResolutionQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search", StatusFilter = "Open" },
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
            .ReturnsAsync((new List<ResolutionListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllResolutionQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
