using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingAutoComplete;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByCustomerId;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingById;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAllAgentCustomerMapping;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class AgentCustomerMappingControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private AgentCustomerMappingController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllAgentCustomerMappingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCustomerMappingDto>>
            {
                IsSuccess = true,
                Data = new List<AgentCustomerMappingDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllAgentCustomerMappingAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllAgentCustomerMappingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCustomerMappingDto>>
            {
                IsSuccess = true,
                Data = new List<AgentCustomerMappingDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllAgentCustomerMappingAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllAgentCustomerMappingQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentCustomerMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentCustomerMappingDto());

        var result = await CreateSut().GetAgentCustomerMappingByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentCustomerMappingAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentCustomerMappingLookupDto>() as IReadOnlyList<AgentCustomerMappingLookupDto>);

        var result = await CreateSut().GetAgentCustomerMappingAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetByCustomerId ──────────────────────────────────────────

    [Fact]
    public async Task GetByCustomerId_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentCustomerMappingByCustomerIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCustomerMappingDto>>
            {
                IsSuccess = true,
                Data = new List<AgentCustomerMappingDto>(),
                TotalCount = 0
            });

        var result = await CreateSut().GetAgentCustomerMappingByCustomerIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByCustomerId_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentCustomerMappingByCustomerIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCustomerMappingDto>>
            {
                IsSuccess = true,
                Data = new List<AgentCustomerMappingDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAgentCustomerMappingByCustomerIdAsync(5);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentCustomerMappingByCustomerIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateAgentCustomerMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateAgentCustomerMapping(new CreateAgentCustomerMappingCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateAgentCustomerMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateAgentCustomerMapping(new UpdateAgentCustomerMappingCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteAgentCustomerMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteAgentCustomerMapping(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteAgentCustomerMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteAgentCustomerMapping(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteAgentCustomerMappingCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
