using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.DeleteSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using SalesManagement.Application.SalesGroup.Dto;
using SalesManagement.Application.SalesGroup.Queries.GetAllSalesGroup;
using SalesManagement.Application.SalesGroup.Queries.GetSalesGroupAutoComplete;
using SalesManagement.Application.SalesGroup.Queries.GetSalesGroupById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesGroupControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesGroupController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesGroupQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesGroupDto>>
            {
                IsSuccess = true,
                Data = new List<SalesGroupDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllSalesGroupAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesGroupQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesGroupDto>>
            {
                IsSuccess = true,
                Data = new List<SalesGroupDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllSalesGroupAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllSalesGroupQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesGroupByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesGroupDto());

        var result = await CreateSut().GetSalesGroupByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesGroupLookupDto>() as IReadOnlyList<SalesGroupLookupDto>);

        var result = await CreateSut().GetSalesGroupAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesGroupCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateSalesGroup(new CreateSalesGroupCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateSalesGroupCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateSalesGroup(new UpdateSalesGroupCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesGroupCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesGroup(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesGroupCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesGroup(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteSalesGroupCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
