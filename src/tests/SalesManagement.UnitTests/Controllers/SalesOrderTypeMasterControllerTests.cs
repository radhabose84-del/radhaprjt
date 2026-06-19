using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.CreateSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.DeleteSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetAllSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterAutoComplete;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesOrderTypeMasterControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesOrderTypeMasterController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesOrderTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesOrderTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<SalesOrderTypeMasterDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllSalesOrderTypeMasterAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesOrderTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesOrderTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<SalesOrderTypeMasterDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllSalesOrderTypeMasterAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllSalesOrderTypeMasterQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesOrderTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesOrderTypeMasterDto());

        var result = await CreateSut().GetSalesOrderTypeMasterByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesOrderTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesOrderTypeMasterLookupDto>() as IReadOnlyList<SalesOrderTypeMasterLookupDto>);

        var result = await CreateSut().GetSalesOrderTypeMasterAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesOrderTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateSalesOrderTypeMaster(new CreateSalesOrderTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateSalesOrderTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateSalesOrderTypeMaster(new UpdateSalesOrderTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesOrderTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesOrderTypeMaster(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesOrderTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesOrderTypeMaster(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteSalesOrderTypeMasterCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
