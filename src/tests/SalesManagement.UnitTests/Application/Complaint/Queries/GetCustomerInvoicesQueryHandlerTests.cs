using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetCustomerInvoices;

namespace SalesManagement.UnitTests.Application.Complaint.Queries;

public sealed class GetCustomerInvoicesQueryHandlerTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private GetCustomerInvoicesQueryHandler CreateSut() => new(_mockQueryRepo.Object);

    [Fact]
    public async Task Handle_ReturnsInvoices()
    {
        var invoices = new List<CustomerInvoiceDto>
        {
            new CustomerInvoiceDto { Id = 1, InvoiceNo = "INV001" }
        };

        _mockQueryRepo
            .Setup(r => r.GetCustomerInvoicesAsync(1))
            .ReturnsAsync(invoices);

        var result = await CreateSut().Handle(
            new GetCustomerInvoicesQuery { CustomerId = 1 }, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].InvoiceNo.Should().Be("INV001");
    }

    [Fact]
    public async Task Handle_NoInvoices_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetCustomerInvoicesAsync(99))
            .ReturnsAsync(new List<CustomerInvoiceDto>());

        var result = await CreateSut().Handle(
            new GetCustomerInvoicesQuery { CustomerId = 99 }, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
