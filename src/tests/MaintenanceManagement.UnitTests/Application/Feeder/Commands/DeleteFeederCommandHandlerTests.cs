using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Feeder.Commands
{
    public sealed class DeleteFeederCommandHandlerTests
    {
        private readonly Mock<IFeederCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteFeederCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(It.IsAny<DeleteFeederCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.Feeder { Id = id });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new DeleteFeederCommand { Id = 1 }, CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteFeederCommand { Id = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteFeederCommand { Id = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(It.IsAny<DeleteFeederCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.Feeder { Id = 99 });
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()))
                .ReturnsAsync(false);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteFeederCommand { Id = 99 }, CancellationToken.None);
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
