using Contracts.Common;
using Contracts.Dtos.Lookups.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.DeleteCommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.UpdateCommissionSplit;
using SalesManagement.Application.CommissionSplit.Dto;
using SalesManagement.Application.CommissionSplit.Queries.GetAllCommissionSplit;
using SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitAutoComplete;
using SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class CommissionSplitControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private CommissionSplitController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllCommissionSplitQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<CommissionSplitDto>>
            {
                IsSuccess = true,
                Data = new List<CommissionSplitDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllCommissionSplitAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllCommissionSplitQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<CommissionSplitDto>>
            {
                IsSuccess = true,
                Data = new List<CommissionSplitDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllCommissionSplitAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllCommissionSplitQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCommissionSplitByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommissionSplitDto());

        var result = await CreateSut().GetCommissionSplitByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCommissionSplitAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommissionSplitLookupDto>() as IReadOnlyList<CommissionSplitLookupDto>);

        var result = await CreateSut().GetCommissionSplitAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateCommissionSplitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateCommissionSplit(new CreateCommissionSplitCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateCommissionSplitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateCommissionSplit(new UpdateCommissionSplitCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCommissionSplitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteCommissionSplit(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCommissionSplitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteCommissionSplit(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteCommissionSplitCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
