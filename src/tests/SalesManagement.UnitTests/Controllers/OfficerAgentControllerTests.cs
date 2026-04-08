using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Application.OfficerAgent.Queries.GetAllOfficerAgent;
using SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentAutoComplete;
using SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class OfficerAgentControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private OfficerAgentController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllOfficerAgentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<OfficerAgentGroupedDto>>
            {
                IsSuccess = true,
                Data = new List<OfficerAgentGroupedDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllOfficerAgentAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllOfficerAgentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<OfficerAgentGroupedDto>>
            {
                IsSuccess = true,
                Data = new List<OfficerAgentGroupedDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllOfficerAgentAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllOfficerAgentQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOfficerAgentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<OfficerAgentGroupedDto>
            {
                IsSuccess = true,
                Data = new OfficerAgentGroupedDto()
            });

        var result = await CreateSut().GetOfficerAgentByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOfficerAgentAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OfficerAgentGroupedDto>() as IReadOnlyList<OfficerAgentGroupedDto>);

        var result = await CreateSut().GetOfficerAgentAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOfficerAgentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateOfficerAgent(new CreateOfficerAgentCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOfficerAgentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateOfficerAgent(new UpdateOfficerAgentCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteOfficerAgentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteOfficerAgent(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteOfficerAgentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteOfficerAgent(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteOfficerAgentCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
