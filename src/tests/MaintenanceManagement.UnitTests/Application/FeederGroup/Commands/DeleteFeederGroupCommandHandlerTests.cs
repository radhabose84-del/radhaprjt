using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.FeederGroup.Commands
{
    public sealed class DeleteFeederGroupCommandHandlerTests
    {
        private readonly Mock<IFeederGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFeederGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteFeederGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(It.IsAny<DeleteFeederGroupCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.FeederGroup { Id = id });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<MaintenanceManagement.Domain.Entities.Power.FeederGroup>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new DeleteFeederGroupCommand { Id = 1 }, CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteFeederGroupCommand { Id = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.Power.FeederGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteFeederGroupCommand { Id = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(It.IsAny<DeleteFeederGroupCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.FeederGroup { Id = 99 });
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<MaintenanceManagement.Domain.Entities.Power.FeederGroup>()))
                .ReturnsAsync(false);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteFeederGroupCommand { Id = 99 }, CancellationToken.None);
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
