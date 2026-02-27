using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.DeleteItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Application.ItemPriceMaster.Queries.GetAllItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterAutoComplete;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class ItemPriceMasterControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private ItemPriceMasterController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllItemPriceMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ItemPriceMasterDto>>
            {
                IsSuccess = true,
                Data = new List<ItemPriceMasterDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllItemPriceMasterAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllItemPriceMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ItemPriceMasterDto>>
            {
                IsSuccess = true,
                Data = new List<ItemPriceMasterDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllItemPriceMasterAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllItemPriceMasterQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetItemPriceMasterByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemPriceMasterDto());

        var result = await CreateSut().GetItemPriceMasterByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetItemPriceMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ItemPriceMasterLookupDto>() as IReadOnlyList<ItemPriceMasterLookupDto>);

        var result = await CreateSut().GetItemPriceMasterAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateItemPriceMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateItemPriceMaster(new CreateItemPriceMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateItemPriceMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateItemPriceMaster(new UpdateItemPriceMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteItemPriceMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteItemPriceMaster(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteItemPriceMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteItemPriceMaster(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteItemPriceMasterCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
