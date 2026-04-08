using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Units.Commands.DeleteUnit;
using UserManagement.Application.Units.Commands.UpdateUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Units.Queries.GetUnitById;
using UserManagement.Application.Units.Queries.GetUnitAutoComplete;
using UserManagement.Application.Units.Queries.GetUnitByUserId;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class UnitControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UnitController>> _mockLogger = new();

        private UnitController CreateSut() => new(_mockSender.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllUnitsAsync_WithData_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUnitsDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetUnitsDTO> { new GetUnitsDTO() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUnitsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllUnitsAsync_EmptyData_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUnitsDTO>>
                {
                    IsSuccess = true,
                    Message = "No records",
                    Data = new List<GetUnitsDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUnitsAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAllUnitsAsync_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUnitsDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetUnitsDTO> { new GetUnitsDTO() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllUnitsAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetUnitsByIdDto());

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
        public async Task CreateUnitAsync_ReturnsOkResult()
        {
            var command = new CreateUnitCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateUnitAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateUnitAsync_ReturnsOkResult()
        {
            var command = new UpdateUnitCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateUnitAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteUnitAsync_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteUnitAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnit_AutoComplete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitAutoCompleteDTO>());

            var result = await CreateSut().GetUnit("test", null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnitByUserId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitByUserIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitAutoCompleteDTO>());

            var result = await CreateSut().GetUnitByUserId(null, 1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnitByUserId_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitByUserIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitAutoCompleteDTO>());

            await CreateSut().GetUnitByUserId(null, 1);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetUnitByUserIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
