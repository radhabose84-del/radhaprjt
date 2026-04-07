using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord;
using GateEntryManagement.Domain.Events;
using GateEntryManagement.UnitTests.TestData;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.VehicleMovementRecord.Commands
{
    public sealed class UpdateVehicleMovementRecordCommandHandlerTests
    {
        private readonly Mock<IVehicleMovementRecordCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateVehicleMovementRecordCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper
                .Setup(m => m.Map<GateEntryManagement.Domain.Entities.VehicleMovementRecord>(It.IsAny<UpdateVehicleMovementRecordCommand>()))
                .Returns(VehicleMovementRecordBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<GateEntryManagement.Domain.Entities.VehicleMovementRecord>()))
                .ReturnsAsync(result);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = VehicleMovementRecordBuilders.ValidUpdateCommand();
            SetupHappyPath();
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Vehicle Movement Record updated successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = VehicleMovementRecordBuilders.ValidUpdateCommand();
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<GateEntryManagement.Domain.Entities.VehicleMovementRecord>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = VehicleMovementRecordBuilders.ValidUpdateCommand();
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "VMR_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
