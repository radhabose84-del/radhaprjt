using AutoMapper;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Commands.Batch2
{
    public sealed class CreateMachineSpecficationCommandHandlerTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMachineSpecficationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateMachineSpecficationCommand ValidCommand() => new()
        {
            Specifications = new List<MachineSpecificationCreateDto>
            {
                new() { SpecificationId = 1, MachineId = 1, SpecificationValue = "V1" }
            }
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<MachineSpecificationCreateDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(5);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Contain(5);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ReturnsFailure()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<MachineSpecificationCreateDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(0);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<MachineSpecificationCreateDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineSpecification"),
                               It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
    }
}
