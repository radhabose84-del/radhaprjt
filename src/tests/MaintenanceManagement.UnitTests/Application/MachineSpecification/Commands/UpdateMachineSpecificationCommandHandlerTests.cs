using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Commands
{
    public sealed class UpdateMachineSpecificationCommandHandlerTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);

        private UpdateMachineSpecficationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object, _mockTimeZone.Object);

        private static UpdateMachineSpecficationCommand ValidCommand() => new()
        {
            Specifications = new List<MachineSpecificationUpdateDto>
            {
                new() { SpecificationId = 1, MachineId = 1, SpecificationValue = "Value1" },
                new() { SpecificationId = 2, MachineId = 1, SpecificationValue = "Value2" }
            }
        };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockMapper.Setup(m => m.Map<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceManagement.Domain.Entities.MachineSpecification>
                {
                    new() { MachineId = 1, SpecificationId = 1 },
                    new() { MachineId = 1, SpecificationId = 2 }
                });
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>()))
                .ReturnsAsync(updateResult);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_UpdateSuccess_PublishesAuditEventPerSpec()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
