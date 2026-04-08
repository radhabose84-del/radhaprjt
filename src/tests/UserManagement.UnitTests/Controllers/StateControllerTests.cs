using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Application.State.Commands.DeleteState;
using UserManagement.Application.State.Commands.UpdateState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.State.Queries.GetStateAutoComplete;
using UserManagement.Application.State.Queries.GetStateById;
using UserManagement.Application.State.Queries.GetStateByCountryId;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class StateControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private StateController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAllStatesAsync_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StateDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<StateDto> { new StateDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllStatesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllStatesAsync_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StateDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<StateDto> { new StateDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllStatesAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetStateQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StateDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_NullResult_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StateDto?)null!);

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateStateCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateStateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StateDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ValidCountryId_ReturnsOkResult()
        {
            var command = new UpdateStateCommand { CountryId = 1 };

            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateStateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_InvalidCountryId_ReturnsBadRequest()
        {
            var command = new UpdateStateCommand { CountryId = 0 };

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteStateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StateDto());

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetState_AutoComplete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStateAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateAutoCompleteDTO>());

            var result = await CreateSut().GetState("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStateByCountryId_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStateByCountryIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateDto> { new StateDto() });

            var result = await CreateSut().GetStateByCountryId(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStateByCountryId_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetStateByCountryId(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetStateByCountryId_NullResult_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStateByCountryIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<StateDto>?)null!);

            var result = await CreateSut().GetStateByCountryId(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
