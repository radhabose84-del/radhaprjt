using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Dto;
using SalesManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using SalesManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class MiscTypeMasterControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private MiscTypeMasterController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MiscTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<MiscTypeMasterDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllMiscTypeMasterAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MiscTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<MiscTypeMasterDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllMiscTypeMasterAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MiscTypeMasterDto());

        var result = await CreateSut().GetMiscTypeMasterByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MiscTypeMasterLookupDto>() as IReadOnlyList<MiscTypeMasterLookupDto>);

        var result = await CreateSut().GetMiscTypeMasterAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateMiscTypeMaster(new CreateMiscTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateMiscTypeMaster(new UpdateMiscTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteMiscTypeMaster(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteMiscTypeMaster(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
