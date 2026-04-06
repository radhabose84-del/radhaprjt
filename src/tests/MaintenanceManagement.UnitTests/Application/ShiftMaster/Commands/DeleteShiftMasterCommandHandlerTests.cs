using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMaster.Commands
{
    public sealed class DeleteShiftMasterCommandHandlerTests
    {
        private readonly Mock<IShiftMasterCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IShiftMasterQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteShiftMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static DeleteShiftMasterCommand ValidCommand() => new() { Id = 1 };

        private void SetupHappyPath(bool deleteResult = true)
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMaster>(It.IsAny<DeleteShiftMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMaster());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMaster>()))
                .ReturnsAsync(deleteResult);
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
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
