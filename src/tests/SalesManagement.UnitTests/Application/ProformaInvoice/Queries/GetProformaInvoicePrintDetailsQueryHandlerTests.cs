using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoicePrintDetails;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Queries;

public sealed class GetProformaInvoicePrintDetailsQueryHandlerTests
{
    private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetProformaInvoicePrintDetailsQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    private static ProformaInvoicePrintDto BuildPrintDto() => new()
    {
        Company = new ProformaInvoicePrintCompanyDto { Name = "Test Company" },
        Header = new ProformaInvoicePrintHeaderDto { PiNumber = "PI001", PiDate = "2026-04-17" },
        Items = new List<ProformaInvoicePrintItemDto>
        {
            new() { SNo = 1, ProductName = "Test Product", QuantityKg = 100m, Rate = 50m, Amount = 5000m }
        },
        Totals = new ProformaInvoicePrintTotalsDto { InvoiceAmount = 5500m }
    };

    [Fact]
    public async Task Handle_ValidId_ReturnsPrintDto()
    {
        var printDto = BuildPrintDto();
        _mockQueryRepo
            .Setup(r => r.GetPrintDetailsAsync(1))
            .ReturnsAsync(printDto);

        var query = new GetProformaInvoicePrintDetailsQuery(1);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Company!.Name.Should().Be("Test Company");
        result.Header!.PiNumber.Should().Be("PI001");
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        _mockQueryRepo
            .Setup(r => r.GetPrintDetailsAsync(9999))
            .ReturnsAsync((ProformaInvoicePrintDto?)null);

        var query = new GetProformaInvoicePrintDetailsQuery(9999);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetPrintDetailsAsync(9999))
            .ReturnsAsync((ProformaInvoicePrintDto?)null);

        var query = new GetProformaInvoicePrintDetailsQuery(9999);

        await CreateSut().Handle(query, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ValidId_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetPrintDetailsAsync(1))
            .ReturnsAsync(BuildPrintDto());

        var query = new GetProformaInvoicePrintDetailsQuery(1);

        await CreateSut().Handle(query, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetPrintDetails" &&
                    e.ActionCode == "PROFORMA_INVOICE_PRINT" &&
                    e.Module == "ProformaInvoice"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_CallsRepositoryOnce()
    {
        _mockQueryRepo
            .Setup(r => r.GetPrintDetailsAsync(1))
            .ReturnsAsync(BuildPrintDto());

        var query = new GetProformaInvoicePrintDetailsQuery(1);

        await CreateSut().Handle(query, CancellationToken.None);

        _mockQueryRepo.Verify(r => r.GetPrintDetailsAsync(1), Times.Once);
    }

    [Fact]
    public async Task Handle_AuditEventContainsRequestId()
    {
        _mockQueryRepo
            .Setup(r => r.GetPrintDetailsAsync(42))
            .ReturnsAsync(BuildPrintDto());

        var query = new GetProformaInvoicePrintDetailsQuery(42);

        await CreateSut().Handle(query, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionName == "42"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
