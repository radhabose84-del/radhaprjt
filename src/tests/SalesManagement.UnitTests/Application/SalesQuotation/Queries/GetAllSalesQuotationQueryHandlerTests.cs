using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotation;

namespace SalesManagement.UnitTests.Application.SalesQuotation.Queries;

public sealed class GetAllSalesQuotationQueryHandlerTests
{
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllSalesQuotationQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<SalesQuotationHeaderDto> { new() { Id = 1 } }, 1));

        var result = await CreateSut().Handle(
            new GetAllSalesQuotationQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<SalesQuotationHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesQuotationQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
