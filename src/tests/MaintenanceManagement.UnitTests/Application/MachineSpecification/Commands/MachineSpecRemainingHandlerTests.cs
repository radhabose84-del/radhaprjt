using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Commands
{
    public sealed class CreateMachineSpecficationCommandHandlerTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMachineSpecficationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<MachineSpecificationCreateDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>())).ReturnsAsync(1);

            var command = new CreateMachineSpecficationCommand
            {
                Specifications = new() { new MachineSpecificationCreateDto { SpecificationId = 1, MachineId = 1 } }
            };
            var result = await CreateSut().Handle(command, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Contain(1);
        }
    }

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
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<DeleteMachineSpecficationCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());
            _mockCommandRepo.Setup(r => r.DeleteAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>())).ReturnsAsync(1);

            var result = await CreateSut().Handle(new DeleteMachineSpecficationCommand { Id = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<DeleteMachineSpecficationCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());
            _mockCommandRepo.Setup(r => r.DeleteAsync(99, It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>())).ReturnsAsync(-1);

            var result = await CreateSut().Handle(new DeleteMachineSpecficationCommand { Id = 99 }, CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
