using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.VehicleMovementRecord.Commands
{
    public sealed class DeleteVehicleMovementRecordCommandHandlerTests
    {
        private readonly Mock<IVehicleMovementRecordCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteVehicleMovementRecordCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id, bool result = true)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(new DeleteVehicleMovementRecordCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(new DeleteVehicleMovementRecordCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "VMR_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new DeleteVehicleMovementRecordCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("Vehicle Movement Record not found.");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            try { await sut.Handle(new DeleteVehicleMovementRecordCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
