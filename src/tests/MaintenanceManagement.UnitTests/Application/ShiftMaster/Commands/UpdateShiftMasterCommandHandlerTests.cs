using AutoMapper;
using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMaster.Commands
{
    public sealed class UpdateShiftMasterCommandHandlerTests
    {
        private readonly Mock<IShiftMasterCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IShiftMasterQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateShiftMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateShiftMasterCommand ValidCommand() => new()
        {
            Id = 1, ShiftName = "Morning", IsActive = 1
        };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockQueryRepo.Setup(r => r.IsShiftMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMaster>(It.IsAny<UpdateShiftMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMaster>()))
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
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.ShiftMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.IsShiftMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ShiftMaster>(It.IsAny<UpdateShiftMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ShiftMaster());

            var command = new UpdateShiftMasterCommand { Id = 1, ShiftName = "Morning", IsActive = 0 };
            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
