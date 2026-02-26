using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.DeleteMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Application.MarketingOfficer.Queries.GetAllMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerAutoComplete;
using SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class MarketingOfficerControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private MarketingOfficerController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMarketingOfficerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MarketingOfficerDto>>
            {
                IsSuccess = true,
                Data = new List<MarketingOfficerDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllMarketingOfficerAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMarketingOfficerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MarketingOfficerDto>>
            {
                IsSuccess = true,
                Data = new List<MarketingOfficerDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllMarketingOfficerAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllMarketingOfficerQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMarketingOfficerByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketingOfficerDto());

        var result = await CreateSut().GetMarketingOfficerByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMarketingOfficerAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MarketingOfficerLookupDto>() as IReadOnlyList<MarketingOfficerLookupDto>);

        var result = await CreateSut().GetMarketingOfficerAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateMarketingOfficerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateMarketingOfficer(new CreateMarketingOfficerCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateMarketingOfficerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateMarketingOfficer(new UpdateMarketingOfficerCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMarketingOfficerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteMarketingOfficer(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMarketingOfficerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteMarketingOfficer(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteMarketingOfficerCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
