using AutoMapper;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Commands.Batch2
{
    public sealed class UpdateMachineSpecficationCommandHandlerTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);

        private UpdateMachineSpecficationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockIp.Object, _mockTz.Object);

        private static UpdateMachineSpecficationCommand ValidCommand() => new()
        {
            Specifications = new List<MachineSpecificationUpdateDto>
            {
                new() { SpecificationId = 1, MachineId = 1, SpecificationValue = "V1" }
            }
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceManagement.Domain.Entities.MachineSpecification>
                {
                    new() { Id = 1, MachineId = 1, SpecificationId = 1 }
                });
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ReturnsFailure()
        {
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceManagement.Domain.Entities.MachineSpecification>());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Success_PublishesAuditEventPerSpec()
        {
            _mockMapper
                .Setup(m => m.Map<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>(It.IsAny<object>()))
                .Returns(new List<MaintenanceManagement.Domain.Entities.MachineSpecification>
                {
                    new() { Id = 1, MachineId = 1, SpecificationId = 1 },
                    new() { Id = 2, MachineId = 1, SpecificationId = 2 }
                });
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineSpecification"),
                               It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}
