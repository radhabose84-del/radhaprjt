using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Commands
{
    public sealed class DeleteMachineMasterCommandHandlerTests
    {
        private readonly Mock<IMachineMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteMachineMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static DeleteMachineMasterCommand ValidCommand() => new() { Id = 1 };

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(It.IsAny<DeleteMachineMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineMaster());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()))
                .ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(It.IsAny<DeleteMachineMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineMaster());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
