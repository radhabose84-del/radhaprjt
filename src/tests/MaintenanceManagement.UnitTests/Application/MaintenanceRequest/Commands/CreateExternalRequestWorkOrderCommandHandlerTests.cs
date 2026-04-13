using AutoMapper;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateExternalRequestWorkOrder;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Commands.BatchD
{
    public sealed class CreateExternalRequestWorkOrderCommandHandlerTests
    {
        private readonly Mock<IMaintenanceRequestCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockWorkOrderCommand = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockWorkOrderQuery = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Loose);

        private CreateExternalRequestWorkOrderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQueryRepo.Object, _mockWorkOrderCommand.Object, _mockWorkOrderQuery.Object,
                _mockIpService.Object, _mockTimeZoneService.Object);

        [Fact]
        public async Task Handle_NoExternalRequests_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetExternalRequestByIdAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetExternalRequestByIdDto>());

            var result = await CreateSut().Handle(
                new CreateExternalRequestWorkOrderCommand { Ids = new List<int> { 1 } },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NoOpenStatus_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetExternalRequestByIdAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetExternalRequestByIdDto> { new() { Id = 1, MaintenanceTypeId = 1 } });
            _mockQueryRepo.Setup(r => r.GetMaintenanceOpenstatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());

            var result = await CreateSut().Handle(
                new CreateExternalRequestWorkOrderCommand { Ids = new List<int> { 1 } },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_HappyPath_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetExternalRequestByIdAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetExternalRequestByIdDto> { new() { Id = 1, MaintenanceTypeId = 1 } });
            _mockQueryRepo.Setup(r => r.GetMaintenanceOpenstatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() { Id = 1 } });
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(It.IsAny<object>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder { MiscStatus = null! });
            _mockWorkOrderCommand.Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder { MiscStatus = null!, Id = 1 });

            var result = await CreateSut().Handle(
                new CreateExternalRequestWorkOrderCommand { Ids = new List<int> { 1 } },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }
    }
}
