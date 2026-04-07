using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn;
using SalesManagement.Application.SalesReturn.Commands.DeleteSalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Application.SalesReturn.Queries.GetAllSalesReturn;
using SalesManagement.Application.SalesReturn.Queries.GetComplaintReturnData;
using SalesManagement.Application.SalesReturn.Queries.GetSalesReturnByComplaint;
using SalesManagement.Application.SalesReturn.Queries.GetSalesReturnById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesReturnControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesReturnController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesReturnQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesReturnListDto>>
            {
                IsSuccess = true,
                Data = new List<SalesReturnListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesReturnByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<SalesReturnHeaderDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new SalesReturnHeaderDto { Id = 1 }
            });

        var result = await CreateSut().GetByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByComplaint_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesReturnByComplaintQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<SalesReturnHeaderDto>
            {
                IsSuccess = true,
                Data = new SalesReturnHeaderDto { Id = 1 }
            });

        var result = await CreateSut().GetByComplaintAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetComplaintReturnData_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetComplaintReturnDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<ComplaintReturnDataDto>
            {
                IsSuccess = true,
                Data = new ComplaintReturnDataDto { ComplaintHeaderId = 1 }
            });

        var result = await CreateSut().GetComplaintReturnDataAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesReturnCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Success", Data = 1 });

        var command = new CreateSalesReturnCommand
        {
            ComplaintHeaderId = 1,
            CustomerId = 1,
            WarehouseId = 1,
            BinId = 1
        };
        var result = await CreateSut().CreateSalesReturn(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesReturnCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesReturn(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesReturnCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesReturn(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteSalesReturnCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
