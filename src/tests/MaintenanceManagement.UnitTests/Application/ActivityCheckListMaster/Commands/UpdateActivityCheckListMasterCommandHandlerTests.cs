using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityCheckListMaster.Commands
{
    public sealed class UpdateActivityCheckListMasterCommandHandlerTests
    {
        private readonly Mock<IActivityCheckListMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IActivityCheckListMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateActivityCheckListMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateActivityCheckListMasterCommand ValidCommand() => new()
        {
            Id = 1,
            ActivityChecklist = "Updated Name"
        };

        private void SetupHappyPath()
        {
            _mockQueryRepo
                .Setup(r => r.IsActivityCheckListMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>(It.IsAny<UpdateActivityCheckListMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
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
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenLinked_ThrowsExceptionRules()
        {
            _mockQueryRepo.Setup(r => r.IsActivityCheckListMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>(It.IsAny<UpdateActivityCheckListMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster());

            var command = ValidCommand();
            command.IsActive = 0;

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            _mockQueryRepo.Setup(r => r.IsActivityCheckListMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>(It.IsAny<UpdateActivityCheckListMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
