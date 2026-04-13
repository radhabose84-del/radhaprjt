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
    }
}
