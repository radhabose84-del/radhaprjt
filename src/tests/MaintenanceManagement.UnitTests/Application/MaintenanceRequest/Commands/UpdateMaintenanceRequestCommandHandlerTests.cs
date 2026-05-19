using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Party;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Commands.BatchD
{
    public sealed class UpdateMaintenanceRequestCommandHandlerTests
    {
        private readonly Mock<IMaintenanceRequestCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ISupplierLookup> _mockSupplierLookup = new(MockBehavior.Loose);

        private UpdateMaintenanceRequestCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQueryRepo.Object, _mockSupplierLookup.Object);

        private static UpdateMaintenanceRequestCommand ValidCommand() => new()
        {
            Id = 1,
            RequestTypeId = 1,
            MaintenanceTypeId = 1,
            MachineId = 10,
            ProductionDepartmentId = 2,
            MaintenanceDepartmentId = 3,
            RequestStatusId = 1
        };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceRequest>(It.IsAny<UpdateMaintenanceRequestCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MaintenanceRequest());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceRequest>()))
                .ReturnsAsync(updateResult);
            // No External request type configured → request treated as Internal (no vendor required).
            _mockQueryRepo.Setup(r => r.GetMaintenanceExternalRequestTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateFails_ReturnsFailure()
        {
            SetupHappyPath(updateResult: false);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceRequest>()),
                Times.Once);
        }
    }
}
