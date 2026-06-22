using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAmendmentById;

namespace SalesManagement.UnitTests.Application.SalesQuotationAmendment.Queries;

public class GetSalesQuotationAmendmentByIdQueryHandlerTests
{
    private readonly Mock<ISalesQuotationAmendmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesQuotationAmendmentByIdQueryHandler CreateSut()
    {
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesQuotationAmendmentByIdQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAmendmentsForQuotation()
    {
        var data = new List<SalesQuotationAmendmentHeaderDto>
        {
            new() { Id = 1, SalesQuotationHeaderId = 7, AmendmentNo = "Q1/AMD/1" }
        };
        _mockQueryRepo.Setup(r => r.GetBySalesQuotationHeaderIdAsync(7)).ReturnsAsync(data);

        var result = await CreateSut().Handle(new GetSalesQuotationAmendmentByIdQuery { SalesQuotationHeaderId = 7 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NoAmendments_ReturnsEmptyList()
    {
        _mockQueryRepo.Setup(r => r.GetBySalesQuotationHeaderIdAsync(7)).ReturnsAsync(new List<SalesQuotationAmendmentHeaderDto>());

        var result = await CreateSut().Handle(new GetSalesQuotationAmendmentByIdQuery { SalesQuotationHeaderId = 7 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
