using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.DeleteSalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.UpdateSalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotation;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAutoComplete;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesQuotationControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesQuotationController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesQuotationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesQuotationHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<SalesQuotationHeaderDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllSalesQuotationAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesQuotationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesQuotationHeaderDto { Id = 1 });

        var result = await CreateSut().GetSalesQuotationByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesQuotationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesQuotationLookupDto>() as IReadOnlyList<SalesQuotationLookupDto>);

        var result = await CreateSut().GetSalesQuotationAutoCompleteAsync("test");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesQuotationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateSalesQuotationCommand { CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1 };
        var result = await CreateSut().CreateSalesQuotation(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateSalesQuotationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateSalesQuotationCommand { Id = 1, CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1, IsActive = 1 };
        var result = await CreateSut().UpdateSalesQuotation(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesQuotationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesQuotation(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesQuotationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesQuotation(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteSalesQuotationCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
