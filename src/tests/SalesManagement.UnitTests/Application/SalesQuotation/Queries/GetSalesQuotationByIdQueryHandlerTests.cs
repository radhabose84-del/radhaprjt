using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationById;

namespace SalesManagement.UnitTests.Application.SalesQuotation.Queries;

public sealed class GetSalesQuotationByIdQueryHandlerTests
{
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesQuotationByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsDto()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new SalesQuotationHeaderDto { Id = 1, CustomerId = 1 });

        var result = await CreateSut().Handle(
            new GetSalesQuotationByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((SalesQuotationHeaderDto?)null);

        var result = await CreateSut().Handle(
            new GetSalesQuotationByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }
}
