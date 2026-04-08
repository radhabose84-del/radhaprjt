using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.DeleteDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetAllDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingAutoComplete;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class DispatchAddressMappingControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DispatchAddressMappingController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllDispatchAddressMappingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<DispatchAddressMappingDto>>
            {
                IsSuccess = true,
                Data = new List<DispatchAddressMappingDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllDispatchAddressMappingAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllDispatchAddressMappingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<DispatchAddressMappingDto>>
            {
                IsSuccess = true,
                Data = new List<DispatchAddressMappingDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllDispatchAddressMappingAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllDispatchAddressMappingQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDispatchAddressMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DispatchAddressMappingDto());

        var result = await CreateSut().GetDispatchAddressMappingByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetDispatchAddressMappingAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DispatchAddressMappingLookupDto>() as IReadOnlyList<DispatchAddressMappingLookupDto>);

        var result = await CreateSut().GetDispatchAddressMappingAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateDispatchAddressMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateDispatchAddressMapping(new CreateDispatchAddressMappingCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateDispatchAddressMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateDispatchAddressMapping(new UpdateDispatchAddressMappingCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteDispatchAddressMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteDispatchAddressMapping(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteDispatchAddressMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteDispatchAddressMapping(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteDispatchAddressMappingCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
