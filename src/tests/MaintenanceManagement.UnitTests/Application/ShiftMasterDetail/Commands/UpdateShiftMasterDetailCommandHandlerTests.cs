using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMasterDetail.Commands
{
    public sealed class UpdateShiftMasterDetailCommandHandlerTests
    {
        private readonly Mock<IShiftMasterDetailCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateShiftMasterDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateShiftMasterDetailCommand ValidCommand() => new()
        {
            Id = 1, ShiftMasterId = 1, StartTime = new TimeOnly(6, 0), EndTime = new TimeOnly(14, 0)
        };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>(It.IsAny<UpdateShiftMasterDetailCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()))
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
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMasterDetail>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
