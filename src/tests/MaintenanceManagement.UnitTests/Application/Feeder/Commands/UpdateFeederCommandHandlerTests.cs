using AutoMapper;
using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Feeder.Commands
{
    public sealed class UpdateFeederCommandHandlerTests
    {
        private readonly Mock<IFeederCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateFeederCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1, byte isActive = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(It.IsAny<UpdateFeederCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.Feeder { Id = id });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(id, It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateFeederCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateFeederCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateFeederCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateLinkedFeeder_ThrowsValidationException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(It.IsAny<UpdateFeederCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.Feeder { Id = 1 });
            _mockQueryRepo
                .Setup(r => r.IsFeederLinkedAsync(1))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(
                new UpdateFeederCommand { Id = 1, IsActive = 0 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsException()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(It.IsAny<UpdateFeederCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.Power.Feeder { Id = 1 });
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, It.IsAny<MaintenanceManagement.Domain.Entities.Power.Feeder>()))
                .ReturnsAsync(false);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(
                new UpdateFeederCommand { Id = 1, IsActive = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
