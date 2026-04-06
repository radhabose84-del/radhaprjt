using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.CostCenter.Commands
{
    public sealed class UpdateCostCenterCommandHandlerTests
    {
        private readonly Mock<ICostCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICostCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateCostCenterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateCostCenterCommand ValidCommand() => new()
        {
            Id = 1, CostCenterName = "Updated CC", IsActive = 1
        };

        private void SetupHappyPath(int result = 1)
        {
            _mockQueryRepo.Setup(r => r.IsCostCenterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.CostCenter>(It.IsAny<UpdateCostCenterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.CostCenter());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.CostCenter>()))
                .ReturnsAsync(result);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            SetupHappyPath(result: 1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.CostCenter>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsExceptionRules()
        {
            SetupHappyPath(result: 0);
            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
