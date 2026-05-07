using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Commands.BatchD
{
    public sealed class CreateMaintenanceRequestCommandHandlerTests
    {
        private readonly Mock<IMaintenanceRequestCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockWorkOrderCommand = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateMaintenanceRequestCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private CreateMaintenanceRequestCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object,
                _mockWorkOrderCommand.Object, _mockIpService.Object, _mockLogger.Object, _mockOutbox.Object,
                _mockMiscRepo.Object, _mockDeptLookup.Object);

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action act = () => { _ = CreateSut(); };
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Handle_NoOpenStatus_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetMaintenanceOpenstatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());

            var result = await CreateSut().Handle(new CreateMaintenanceRequestCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CreateFails_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetMaintenanceOpenstatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() { Id = 1 } });
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceRequest>(It.IsAny<CreateMaintenanceRequestCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MaintenanceRequest());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceRequest>()))
                .ReturnsAsync(0);

            var result = await CreateSut().Handle(new CreateMaintenanceRequestCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        // Sets up the happy path far enough to reach the wfCreate / breakDownCode lookups.
        private void SetupHappyPathThroughWorkOrder(int requestTypeId = 5)
        {
            _mockQueryRepo.Setup(r => r.GetMaintenanceOpenstatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() { Id = 1 } });

            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceRequest>(It.IsAny<CreateMaintenanceRequestCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MaintenanceRequest { RequestTypeId = requestTypeId });

            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceRequest>()))
                .ReturnsAsync(42);

            _mockQueryRepo.Setup(r => r.GetMaintenanceRequestTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() { Id = requestTypeId } });

            _mockQueryRepo.Setup(r => r.GetMachineInfoAsync(It.IsAny<int>()))
                .ReturnsAsync(("MachineX", 7, 1));

            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceRequest>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder
                {
                    MiscStatus = new MaintenanceManagement.Domain.Entities.MiscMaster()
                });
        }

        [Fact]
        public async Task Handle_NullWfCreate_SkipsNotification_StillReturnsSuccess()
        {
            SetupHappyPathThroughWorkOrder();

            // wfCreate is null — null-guard branch must run
            _mockMiscRepo.Setup(r => r.GetByWFMiscMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MiscMaster?)null);
            _mockMiscRepo.Setup(r => r.GetByMiscMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Id = 99 });

            var result = await CreateSut().Handle(new CreateMaintenanceRequestCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _mockOutbox.Verify(
                o => o.ScheduleWithoutSaveAsync(It.IsAny<Contracts.Events.Notifications.NotificationCreatedEvent>(),
                    It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyMaintenanceRequestType_LogsError_StillReturnsSuccess()
        {
            // Status configured, but MaintenanceRequestType MiscMaster is empty (misconfig).
            _mockQueryRepo.Setup(r => r.GetMaintenanceOpenstatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() { Id = 1 } });
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceRequest>(It.IsAny<CreateMaintenanceRequestCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MaintenanceRequest());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceRequest>()))
                .ReturnsAsync(42);
            _mockQueryRepo.Setup(r => r.GetMaintenanceRequestTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>()); // ← empty
            _mockQueryRepo.Setup(r => r.GetMachineInfoAsync(It.IsAny<int>()))
                .ReturnsAsync(("MachineX", 7, 1));

            var result = await CreateSut().Handle(new CreateMaintenanceRequestCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            // Misconfiguration must be logged at Error level — observable to ops.
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Misconfiguration")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
            // No WorkOrder / notification path runs.
            _mockOutbox.Verify(
                o => o.ScheduleWithoutSaveAsync(It.IsAny<Contracts.Events.Notifications.NotificationCreatedEvent>(),
                    It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_NullBreakDownCode_SkipsNotification_StillReturnsSuccess()
        {
            SetupHappyPathThroughWorkOrder();

            _mockMiscRepo.Setup(r => r.GetByWFMiscMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Id = 11 });
            // breakDownCode is null — null-guard branch must run
            _mockMiscRepo.Setup(r => r.GetByMiscMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MiscMaster?)null);

            var result = await CreateSut().Handle(new CreateMaintenanceRequestCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _mockOutbox.Verify(
                o => o.ScheduleWithoutSaveAsync(It.IsAny<Contracts.Events.Notifications.NotificationCreatedEvent>(),
                    It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
