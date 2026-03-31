using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceCategory.Commands
{
    public sealed class UpdateMaintenanceCategoryCommandHandlerTests
    {
        private readonly Mock<IMaintenanceCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateMaintenanceCategoryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(It.IsAny<object>()))
                .Returns(MaintenanceCategoryBuilders.ValidEntity(id));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(id, It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceCategory>()))
                .ReturnsAsync(id);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidUpdateCommand(id: 1), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidUpdateCommand(id: 1), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceCategory>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidUpdateCommand(id: 1), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(It.IsAny<object>()))
                .Returns(MaintenanceCategoryBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceCategory>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidUpdateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
