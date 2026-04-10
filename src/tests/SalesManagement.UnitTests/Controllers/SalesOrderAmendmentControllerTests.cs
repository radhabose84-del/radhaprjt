using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendmentById;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAmendmentById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesOrderAmendmentControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesOrderAmendmentController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesOrderAmendmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<SalesOrderAmendmentHeaderDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetBySalesOrderHeaderId_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesOrderAmendmentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<SalesOrderAmendmentHeaderDto>()
            });

        var result = await CreateSut().GetBySalesOrderHeaderIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPending_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPendingSalesOrderAmendmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PendingSalesOrderAmendmentDto>(), 0));

        var result = await CreateSut().GetPendingAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPendingById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPendingSalesOrderAmendmentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesOrderAmendmentHeaderDto { Id = 1 });

        var result = await CreateSut().GetPendingByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesOrderAmendmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Success", Data = 1 });

        var command = new CreateSalesOrderAmendmentCommand { SalesOrderHeaderId = 1, Reason = "Test" };
        var result = await CreateSut().CreateAsync(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesOrderAmendmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var command = new CreateSalesOrderAmendmentCommand { SalesOrderHeaderId = 1 };
        await CreateSut().CreateAsync(command);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<CreateSalesOrderAmendmentCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
