using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAmendmentById;

namespace SalesManagement.UnitTests.Application.SalesOrderAmendment.Queries;

public sealed class GetSalesOrderAmendmentByIdQueryHandlerTests
{
    private readonly Mock<ISalesOrderAmendmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesOrderAmendmentByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsData()
    {
        var amendments = new List<SalesOrderAmendmentHeaderDto>
        {
            new() { Id = 1, SalesOrderHeaderId = 10, AmendmentNo = "SO001/AMD/1" }
        };
        _mockQueryRepo
            .Setup(r => r.GetBySalesOrderHeaderIdAsync(10))
            .ReturnsAsync(amendments);

        var result = await CreateSut().Handle(
            new GetSalesOrderAmendmentByIdQuery { SalesOrderHeaderId = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NoAmendments_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetBySalesOrderHeaderIdAsync(99))
            .ReturnsAsync(new List<SalesOrderAmendmentHeaderDto>());

        var result = await CreateSut().Handle(
            new GetSalesOrderAmendmentByIdQuery { SalesOrderHeaderId = 99 },
            CancellationToken.None);

        result.Data.Should().BeEmpty();
    }
}
