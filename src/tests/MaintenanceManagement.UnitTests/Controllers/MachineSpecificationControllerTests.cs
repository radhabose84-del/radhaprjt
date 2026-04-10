using Contracts.Common;
using MaintenanceManagement.Application.MachineSpecification.Command;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.Queries.GetMachineSpecificationById;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class MachineSpecificationControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<MachineSpecificationController>> _mockLogger = new(MockBehavior.Loose);

        private MachineSpecificationController CreateSut() => new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMachineSpecificationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MachineSpecificationDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateMachineSpecficationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<int>> { IsSuccess = true, Data = new() });

            var command = new CreateMachineSpecficationCommand
            {
                Specifications = new List<MachineSpecificationCreateDto>
                {
                    new MachineSpecificationCreateDto { SpecificationId = 1, MachineId = 1, SpecificationValue = "Test" }
                }
            };
            var result = await CreateSut().CreateAsync(command);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateMachineSpecficationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var command = new UpdateMachineSpecficationCommand
            {
                Specifications = new List<MachineSpecificationUpdateDto>
                {
                    new MachineSpecificationUpdateDto { SpecificationId = 1, MachineId = 1, SpecificationValue = "Test" }
                }
            };
            var result = await CreateSut().UpdateAsync(command);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
