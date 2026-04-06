using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkCenter.Commands
{
    public sealed class DeleteWorkCenterCommandHandlerTests
    {
        private readonly Mock<IWorkCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IWorkCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteWorkCenterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static DeleteWorkCenterCommand ValidCommand() => new() { Id = 1 };

        private void SetupHappyPath(int deleteResult = 1)
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(It.IsAny<DeleteWorkCenterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkCenter());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.WorkCenter>()))
                .ReturnsAsync(deleteResult);
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
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.WorkCenter>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsNegativeOne_ReturnsFailure()
        {
            SetupHappyPath(deleteResult: -1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
