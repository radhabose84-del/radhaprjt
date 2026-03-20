using Contracts.Common;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Application.State.Commands.UpdateState;
using UserManagement.Application.State.Commands.DeleteState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.State.Queries.GetStateById;
using UserManagement.Application.State.Queries.GetStateAutoComplete;
using UserManagement.Presentation.Controllers;
using UserManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.UnitTests.Presentation.State
{
    public sealed class StateControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private StateController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllStates_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StateDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<StateDto> { StateBuilders.ValidDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllStatesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllStates_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StateDto>>
                {
                    IsSuccess = true,
                    Data = new List<StateDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllStatesAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetStateQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(StateBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            var command = StateBuilders.ValidCreateCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(StateBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            var command = StateBuilders.ValidUpdateCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateStateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteStateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(StateBuilders.ValidDto());

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStateAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateAutoCompleteDTO> { StateBuilders.ValidAutoCompleteDto() });

            var result = await CreateSut().GetState("Maha");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
