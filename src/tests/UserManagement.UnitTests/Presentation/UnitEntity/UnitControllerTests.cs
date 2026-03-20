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
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Presentation.UnitEntity
{
    public sealed class UnitControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UnitController>> _mockLogger = new(MockBehavior.Loose);

        private UnitController CreateSut() => new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllUnits_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUnitsDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetUnitsDTO> { UnitEntityBuilders.ValidGetUnitsDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUnitsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllUnits_NoData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUnitsDTO>>
                {
                    IsSuccess = false,
                    Message = "No Unit found",
                    Data = new List<GetUnitsDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUnitsAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAllUnits_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetUnitsDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetUnitsDTO> { UnitEntityBuilders.ValidGetUnitsDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllUnitsAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetUnitQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(UnitEntityBuilders.ValidGetUnitsByIdDto());

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
        public async Task Create_ValidCommand_ReturnsOkResult()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateUnitAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateUnitAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUnitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteUnitAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(UnitEntityBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetUnit("Test", 1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnitByUserId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitByUserIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(UnitEntityBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetUnitByUserId(1, 1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
