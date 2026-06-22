using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.TripSheet.Commands.CreateTripSheet;
using SalesManagement.Application.TripSheet.Commands.DeleteTripSheet;
using SalesManagement.Application.TripSheet.Commands.UpdateTripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Application.TripSheet.Queries.GetAllTripSheet;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetAutoComplete;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class TripSheetControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private TripSheetController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTripSheetQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<TripSheetHeaderDto>>
            {
                IsSuccess = true, Data = new List<TripSheetHeaderDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
            });

        var result = await CreateSut().GetAllTripSheetAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTripSheetQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<TripSheetHeaderDto>> { IsSuccess = true, Data = new List<TripSheetHeaderDto>() });

        await CreateSut().GetAllTripSheetAsync(1, 10);

        _mockMediator.Verify(m => m.Send(It.IsAny<GetAllTripSheetQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTripSheetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TripSheetHeaderDto());

        var result = await CreateSut().GetTripSheetByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTripSheetAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TripSheetLookupDto>() as IReadOnlyList<TripSheetLookupDto>);

        var result = await CreateSut().GetTripSheetAutoCompleteAsync("test");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateTripSheetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateTripSheet(new CreateTripSheetCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTripSheetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateTripSheet(new UpdateTripSheetCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTripSheetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteTripSheet(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTripSheetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteTripSheet(1);

        _mockMediator.Verify(m => m.Send(It.IsAny<DeleteTripSheetCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
