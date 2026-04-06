using AutoMapper;
using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class UpdateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateMiscMasterCommand ValidCommand() => new()
        {
            Id = 1, Description = "Updated Misc", IsActive = 1
        };

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.IsMiscMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscMaster>(It.IsAny<UpdateMiscMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MiscMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MiscMaster>()))
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
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MiscMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            _mockQueryRepo.Setup(r => r.IsMiscMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscMaster>(It.IsAny<UpdateMiscMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MiscMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.IsMiscMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscMaster>(It.IsAny<UpdateMiscMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MiscMaster());

            var command = new UpdateMiscMasterCommand { Id = 1, Description = "Updated Misc", IsActive = 0 };
            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
