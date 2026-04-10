using Contracts.Common;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MachineWiseFrequencyUpdate;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ReschedulePreventive;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ScheduleWorkOrder;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetDetailSchedulerByDate;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetSchedulerByDate;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class PreventiveSchedulerControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private PreventiveSchedulerController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPreventiveSchedulerQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetPreventiveSchedulerDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetAll(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPreventiveSchedulerByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<PreventiveSchedulerHdrByIdDto> { IsSuccess = true });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreatePreventiveSchedulerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreatePreventiveSchedulerCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdatePreventiveSchedulerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().Update(new UpdatePreventiveSchedulerCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeletePreventiveSchedulerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Reschedule_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ReshedulePreventiveCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().Reschedule(new ReshedulePreventiveCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateActiveInActive_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ActiveInActivePreventiveCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateActiveInActive(new ActiveInActivePreventiveCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetScheduler_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSchedulerByDateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<SchedulerByDateDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetScheduler(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMachineDetail_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMachineDetailByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<PreventiveSchedulerDto> { IsSuccess = true });

            var result = await CreateSut().GetMachineDetail(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnMappedMachines_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUnMappedMachineQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UnMappedMachineDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetUnMappedMachines(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task MapMachines_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<MapMachineCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().MapMachines(new MapMachineCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task MachineFrequencyUpdate_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<MachineWiseFrequencyUpdateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().MachineFrequencyUpdate(new MachineWiseFrequencyUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
