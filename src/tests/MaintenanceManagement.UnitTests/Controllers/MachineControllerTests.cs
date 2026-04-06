using Contracts.Common;
using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterById;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class MachineControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private MachineController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMachineMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MachineMasterDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetAllMachineMasterAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMachineMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MachineMasterDto>> { IsSuccess = true, Data = new() });

            await CreateSut().GetAllMachineMasterAsync();
            _mockMediator.Verify(m => m.Send(It.IsAny<GetMachineMasterQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMachineMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MachineMasterDto());

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMachineMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MachineMasterAutoCompleteDto>());

            var result = await CreateSut().GetMachineMasterAsync(null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateMachineMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateMachineMasterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateMachineMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateMachineMasterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteMachineMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteMachineMasterAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
