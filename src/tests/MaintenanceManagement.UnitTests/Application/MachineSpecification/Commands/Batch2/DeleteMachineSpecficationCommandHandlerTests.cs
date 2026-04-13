using AutoMapper;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Commands.Batch2
{
    public sealed class DeleteMachineSpecficationCommandHandlerTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteMachineSpecficationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<DeleteMachineSpecficationCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification { Id = 1 });
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteMachineSpecficationCommand { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<DeleteMachineSpecficationCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification { Id = 99 });
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(-1);

            var result = await CreateSut().Handle(new DeleteMachineSpecficationCommand { Id = 99 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<DeleteMachineSpecficationCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification { Id = 1 });
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteMachineSpecficationCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()),
                Times.Once);
        }
    }
}
