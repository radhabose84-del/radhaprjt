using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrder.Commands.CancelSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderDocument;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderImage;
using SalesManagement.Application.SalesOrder.Commands.ForecloseSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderDocument;
using SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderImage;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderById;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAutoComplete;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesOrderControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesOrderController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesOrderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesOrderHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<SalesOrderHeaderDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllSalesOrderAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesOrderHeaderDto { Id = 1 });

        var result = await CreateSut().GetSalesOrderByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesOrderAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesOrderLookupDto>() as IReadOnlyList<SalesOrderLookupDto>);

        var result = await CreateSut().GetSalesOrderAutoCompleteAsync("test");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPending_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPendingSalesOrderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PendingSalesOrderDto>(), 0));

        var result = await CreateSut().GetPendingSalesOrderAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPendingById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPendingSalesOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PendingSalesOrderByIdDto { Id = 1 });

        var result = await CreateSut().GetPendingSalesOrderByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Success", Data = 1 });

        var command = new CreateSalesOrderCommand();
        var result = await CreateSut().CreateSalesOrder(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateSalesOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Success", Data = 1 });

        var command = new UpdateSalesOrderCommand { Id = 1 };
        var result = await CreateSut().UpdateSalesOrder(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Cancel_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelSalesOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().CancelSalesOrder(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Foreclose_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<ForecloseSalesOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().ForecloseSalesOrder(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadDocument_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UploadSalesOrderDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesOrderDocumentDto { ImageName = "test.pdf" });

        var command = new UploadSalesOrderDocumentCommand();
        var result = await CreateSut().UploadSalesOrderDocument(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteDocument_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesOrderDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesOrderDocument("test.pdf");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadImage_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UploadSalesOrderImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesOrderDocumentDto { ImageName = "test.jpg" });

        var command = new UploadSalesOrderImageCommand();
        var result = await CreateSut().UploadSalesOrderImage(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesOrderImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesOrderImage("test.jpg");
        result.Should().BeOfType<OkObjectResult>();
    }
}
