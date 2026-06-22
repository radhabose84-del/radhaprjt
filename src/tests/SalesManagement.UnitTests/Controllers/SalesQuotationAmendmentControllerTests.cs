using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendmentById;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAmendmentById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesQuotationAmendmentControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesQuotationAmendmentController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesQuotationAmendmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>
            {
                IsSuccess = true, Data = new List<SalesQuotationAmendmentHeaderDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
            });

        var result = await CreateSut().GetAllAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetBySalesQuotationHeaderId_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesQuotationAmendmentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>
            {
                IsSuccess = true, Data = new List<SalesQuotationAmendmentHeaderDto>()
            });

        var result = await CreateSut().GetBySalesQuotationHeaderIdAsync(7);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPending_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPendingSalesQuotationAmendmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PendingSalesQuotationAmendmentDto>(), 0));

        var result = await CreateSut().GetPendingAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPendingById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPendingSalesQuotationAmendmentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesQuotationAmendmentHeaderDto());

        var result = await CreateSut().GetPendingByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesQuotationAmendmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateAsync(new CreateSalesQuotationAmendmentCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesQuotationAmendmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        await CreateSut().CreateAsync(new CreateSalesQuotationAmendmentCommand());

        _mockMediator.Verify(m => m.Send(It.IsAny<CreateSalesQuotationAmendmentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
