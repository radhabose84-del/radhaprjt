using Contracts.Common;
using MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryAutoComplete;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryById;
using MaintenanceManagement.Presentation.Controllers;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class MaintenanceCategoryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private MaintenanceCategoryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceCategoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MaintenanceCategoryDto>>
                {
                    IsSuccess = true,
                    Data = new List<MaintenanceCategoryDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMaintenanceCategoryAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceCategoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MaintenanceCategoryDto>>
                {
                    IsSuccess = true,
                    Data = new List<MaintenanceCategoryDto>()
                });

            await CreateSut().GetAllMaintenanceCategoryAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetMaintenanceCategoryQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MaintenanceCategoryBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceCategoryAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MaintenanceCategoryAutoCompleteDto>
                {
                    MaintenanceCategoryBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetMaintenanceCategory("Elec");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMaintenanceCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(MaintenanceCategoryBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMaintenanceCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(MaintenanceCategoryBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMaintenanceCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteMaintenanceTypeAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMaintenanceCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteMaintenanceTypeAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteMaintenanceCategoryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
