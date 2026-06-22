using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotationAmendment;

namespace SalesManagement.UnitTests.Application.SalesQuotationAmendment.Queries;

public class GetAllSalesQuotationAmendmentQueryHandlerTests
{
    private readonly Mock<ISalesQuotationAmendmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAllSalesQuotationAmendmentQueryHandler CreateSut()
    {
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAllSalesQuotationAmendmentQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new List<SalesQuotationAmendmentHeaderDto> { new() { Id = 1, AmendmentNo = "Q1/AMD/1" } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

        var result = await CreateSut().Handle(new GetAllSalesQuotationAmendmentQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<SalesQuotationAmendmentHeaderDto>(), 0));

        var result = await CreateSut().Handle(new GetAllSalesQuotationAmendmentQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
