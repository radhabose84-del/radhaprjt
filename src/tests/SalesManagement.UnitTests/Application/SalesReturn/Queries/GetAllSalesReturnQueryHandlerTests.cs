using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Application.SalesReturn.Queries.GetAllSalesReturn;

namespace SalesManagement.UnitTests.Application.SalesReturn.Queries;

public sealed class GetAllSalesReturnQueryHandlerTests
{
    private readonly Mock<ISalesReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllSalesReturnQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<SalesReturnListDto> { new() { Id = 1 } }, 1));

        var result = await CreateSut().Handle(
            new GetAllSalesReturnQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<SalesReturnListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesReturnQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsPaginationMetadata()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(2, 5, "search"))
            .ReturnsAsync((new List<SalesReturnListDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesReturnQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
            CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }
}
