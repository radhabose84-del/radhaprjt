using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaPayment;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Application.ProformaInvoice.Queries.GetAllProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceAutoComplete;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceById;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceBySalesOrder;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class ProformaInvoiceControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private ProformaInvoiceController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllProformaInvoiceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ProformaInvoiceDto>>
            {
                IsSuccess = true,
                Data = new List<ProformaInvoiceDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllProformaInvoiceAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllProformaInvoiceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ProformaInvoiceDto>>
            {
                IsSuccess = true,
                Data = new List<ProformaInvoiceDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllProformaInvoiceAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllProformaInvoiceQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetProformaInvoiceByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProformaInvoiceDto());

        var result = await CreateSut().GetProformaInvoiceByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetProformaInvoiceAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProformaInvoiceLookupDto>() as IReadOnlyList<ProformaInvoiceLookupDto>);

        var result = await CreateSut().GetProformaInvoiceAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetBySalesOrder ──────────────────────────────────────────

    [Fact]
    public async Task GetBySalesOrder_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetProformaInvoiceBySalesOrderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProformaInvoiceDto>());

        var result = await CreateSut().GetProformaInvoiceBySalesOrderAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateProformaInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateProformaInvoice(new CreateProformaInvoiceCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateProformaInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateProformaInvoice(new UpdateProformaInvoiceCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── UpdatePayment ────────────────────────────────────────────

    [Fact]
    public async Task UpdatePayment_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateProformaPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateProformaPayment(new UpdateProformaPaymentCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteProformaInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteProformaInvoice(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteProformaInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteProformaInvoice(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteProformaInvoiceCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
