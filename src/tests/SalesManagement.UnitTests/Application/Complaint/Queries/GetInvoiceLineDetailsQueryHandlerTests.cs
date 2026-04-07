using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetInvoiceLineDetails;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetInvoiceLineDetailsQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private GetInvoiceLineDetailsQueryHandler CreateSut() => new(_mockQueryRepo.Object);

    [Fact]
    public async Task Handle_ReturnsDetails()
    {
        var details = new List<InvoiceLineDetailDto>
        {
            new InvoiceLineDetailDto { InvoiceHeaderId = 1, ItemId = 10 }
        };

        _mockQueryRepo
            .Setup(r => r.GetInvoiceLineDetailsAsync(1))
            .ReturnsAsync(details);

        var result = await CreateSut().Handle(
            new GetInvoiceLineDetailsQuery { InvoiceHeaderId = 1 }, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NoDetails_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetInvoiceLineDetailsAsync(99))
            .ReturnsAsync(new List<InvoiceLineDetailDto>());

        var result = await CreateSut().Handle(
            new GetInvoiceLineDetailsQuery { InvoiceHeaderId = 99 }, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
