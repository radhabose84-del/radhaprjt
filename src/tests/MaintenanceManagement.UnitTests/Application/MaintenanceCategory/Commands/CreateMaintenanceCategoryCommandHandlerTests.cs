using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceCategory.Commands
{
    public sealed class CreateMaintenanceCategoryCommandHandlerTests
    {
        private readonly Mock<IMaintenanceCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMaintenanceCategoryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(It.IsAny<object>()))
                .Returns(MaintenanceCategoryBuilders.ValidEntity(newId));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceCategory>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceCategory>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(It.IsAny<object>()))
                .Returns(MaintenanceCategoryBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MaintenanceCategory>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(
                MaintenanceCategoryBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
