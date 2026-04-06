using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMasterDetail.Commands
{
    public sealed class DeleteShiftMasterDetailCommandHandlerTests
    {
        private readonly Mock<IShiftMasterDetailCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteShiftMasterDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static DeleteShiftMasterDetailCommand ValidCommand() => new() { Id = 1 };

        private void SetupHappyPath(bool deleteResult = true)
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>(It.IsAny<DeleteShiftMasterDetailCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()))
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
            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()), Times.Once);
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
