using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.StoTypeMaster.Commands.CreateStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.DeleteStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.UpdateStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;
using SalesManagement.Application.StoTypeMaster.Queries.GetAllStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterAutoComplete;
using SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class StoTypeMasterControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private StoTypeMasterController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllStoTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<StoTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<StoTypeMasterDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllStoTypeMasterAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllStoTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<StoTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<StoTypeMasterDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllStoTypeMasterAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllStoTypeMasterQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStoTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoTypeMasterDto());

        var result = await CreateSut().GetStoTypeMasterByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStoTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoTypeMasterLookupDto>() as IReadOnlyList<StoTypeMasterLookupDto>);

        var result = await CreateSut().GetStoTypeMasterAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateStoTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateStoTypeMaster(new CreateStoTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateStoTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateStoTypeMaster(new UpdateStoTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteStoTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteStoTypeMaster(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteStoTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteStoTypeMaster(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteStoTypeMasterCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
