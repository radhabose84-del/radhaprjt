using AutoMapper;
using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkCenter.Commands
{
    public sealed class UpdateWorkCenterCommandHandlerTests
    {
        private readonly Mock<IWorkCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IWorkCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateWorkCenterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateWorkCenterCommand ValidCommand() => new()
        {
            Id = 1, WorkCenterName = "WC-A", IsActive = 1
        };

        private void SetupHappyPath(int updateResult = 1)
        {
            _mockQueryRepo.Setup(r => r.IsWorkCenterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(It.IsAny<UpdateWorkCenterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkCenter());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.WorkCenter>()))
                .ReturnsAsync(updateResult);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.WorkCenter>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ReturnsFailure()
        {
            SetupHappyPath(updateResult: 0);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.IsWorkCenterLinkedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(It.IsAny<UpdateWorkCenterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkCenter());

            var command = new UpdateWorkCenterCommand { Id = 1, WorkCenterName = "WC-A", IsActive = 0 };
            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
