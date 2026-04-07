using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.DeleteMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;
using SalesManagement.Application.MovementTypeConfig.Queries.GetAllMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigAutoComplete;
using SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class MovementTypeConfigControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private MovementTypeConfigController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMovementTypeConfigQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MovementTypeConfigDto>>
            {
                IsSuccess = true,
                Data = new List<MovementTypeConfigDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllMovementTypeConfigAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMovementTypeConfigQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MovementTypeConfigDto>>
            {
                IsSuccess = true,
                Data = new List<MovementTypeConfigDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllMovementTypeConfigAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllMovementTypeConfigQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMovementTypeConfigByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MovementTypeConfigDto());

        var result = await CreateSut().GetMovementTypeConfigByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMovementTypeConfigAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MovementTypeConfigLookupDto>() as IReadOnlyList<MovementTypeConfigLookupDto>);

        var result = await CreateSut().GetMovementTypeConfigAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateMovementTypeConfigCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateMovementTypeConfig(new CreateMovementTypeConfigCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateMovementTypeConfigCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateMovementTypeConfig(new UpdateMovementTypeConfigCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMovementTypeConfigCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteMovementTypeConfig(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMovementTypeConfigCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteMovementTypeConfig(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteMovementTypeConfigCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
