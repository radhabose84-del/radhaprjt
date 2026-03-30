using Contracts.Common;
using MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeAutoComplete;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeById;
using MaintenanceManagement.Presentation.Controllers;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class MaintenanceTypeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private MaintenanceTypeController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MaintenanceTypeDto>>
                {
                    IsSuccess = true,
                    Data = new List<MaintenanceTypeDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMaintenanceTypeAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MaintenanceTypeDto>>
                {
                    IsSuccess = true,
                    Data = new List<MaintenanceTypeDto>()
                });

            await CreateSut().GetAllMaintenanceTypeAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetMaintenanceTypeQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MaintenanceTypeBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MaintenanceTypeAutoCompleteDto>
                {
                    MaintenanceTypeBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetMaintenanceType("Prev");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMaintenanceTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(MaintenanceTypeBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMaintenanceTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(MaintenanceTypeBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMaintenanceTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteMaintenanceTypeAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMaintenanceTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteMaintenanceTypeAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteMaintenanceTypeCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
