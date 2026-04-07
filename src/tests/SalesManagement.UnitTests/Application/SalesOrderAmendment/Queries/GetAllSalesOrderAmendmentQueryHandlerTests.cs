using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrderAmendment;

namespace SalesManagement.UnitTests.Application.SalesOrderAmendment.Queries;

public sealed class GetAllSalesOrderAmendmentQueryHandlerTests
{
    private readonly Mock<ISalesOrderAmendmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllSalesOrderAmendmentQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<SalesOrderAmendmentHeaderDto> { new() { Id = 1 } }, 1));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderAmendmentQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<SalesOrderAmendmentHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderAmendmentQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
