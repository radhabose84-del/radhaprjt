using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Commands
{
    public sealed class DeleteMachineSpecificationCommandHandlerTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteMachineSpecficationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static DeleteMachineSpecficationCommand ValidCommand() => new() { Id = 1 };

        private void SetupHappyPath(int deleteResult = 1)
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<DeleteMachineSpecficationCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(deleteResult);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsNegativeOne_ReturnsFailure()
        {
            SetupHappyPath(-1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
