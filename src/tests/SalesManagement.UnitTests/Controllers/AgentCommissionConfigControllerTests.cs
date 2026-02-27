using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigAutoComplete;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigById;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAllAgentCommissionConfig;
using SalesManagement.Presentation.Controllers;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Controllers
{
    public class AgentCommissionConfigControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private AgentCommissionConfigController CreateSut() => new(_mockMediator.Object);

        // ── GetAll ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllAgentCommissionConfigQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AgentCommissionConfigDto>>
                {
                    IsSuccess = true,
                    Data = new List<AgentCommissionConfigDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAgentCommissionConfigAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllAgentCommissionConfigQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AgentCommissionConfigDto>>
                {
                    IsSuccess = true,
                    Data = new List<AgentCommissionConfigDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAgentCommissionConfigAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllAgentCommissionConfigQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── GetById ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAgentCommissionConfigByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AgentCommissionConfigBuilders.ValidDto());

            var result = await CreateSut().GetAgentCommissionConfigByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAgentCommissionConfigByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AgentCommissionConfigBuilders.ValidDto());

            await CreateSut().GetAgentCommissionConfigByIdAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAgentCommissionConfigByIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── AutoComplete ──────────────────────────────────────────────────────

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAgentCommissionConfigAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AgentCommissionConfigBuilders.ValidLookupList());

            var result = await CreateSut().GetAgentCommissionConfigAutoCompleteAsync("agent");

            result.Should().BeOfType<OkObjectResult>();
        }

        // ── Create ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAgentCommissionConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created.", Data = 1 });

            var result = await CreateSut().CreateAgentCommissionConfig(
                AgentCommissionConfigBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAgentCommissionConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created.", Data = 1 });

            await CreateSut().CreateAgentCommissionConfig(
                AgentCommissionConfigBuilders.ValidCreateCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateAgentCommissionConfigCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── Update ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAgentCommissionConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated.", Data = 1 });

            var result = await CreateSut().UpdateAgentCommissionConfig(
                AgentCommissionConfigBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAgentCommissionConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAgentCommissionConfig(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAgentCommissionConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAgentCommissionConfig(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAgentCommissionConfigCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
