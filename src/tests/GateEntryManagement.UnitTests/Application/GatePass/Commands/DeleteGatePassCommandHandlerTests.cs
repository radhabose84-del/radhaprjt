using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.GatePass.Commands.DeleteGatePass;
using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GatePass.Commands
{
    public sealed class DeleteGatePassCommandHandlerTests
    {
        private readonly Mock<IGatePassCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteGatePassCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMiscMasterQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id, int vmrInStatusId = 10, bool deleteResult = true)
        {
            var inStatusEntity = new GateEntryManagement.Domain.Entities.MiscMaster
            {
                Id = vmrInStatusId,
                Code = "IN",
                Description = "Inside Premises"
            };

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.VMRStatus, MiscEnumEntity.VMRStatusInsidePremises))
                .ReturnsAsync(inStatusEntity);

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, vmrInStatusId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(new DeleteGatePassCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            await sut.Handle(new DeleteGatePassCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "GATEPASS_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            SetupHappyPath(99, deleteResult: false);
            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(
                new DeleteGatePassCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("Gate Pass not found.");
        }

        [Fact]
        public async Task Handle_VMRStatusNotFound_ThrowsExceptionRules()
        {
            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.VMRStatus, MiscEnumEntity.VMRStatusInsidePremises))
                .ReturnsAsync((GateEntryManagement.Domain.Entities.MiscMaster?)null);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(
                new DeleteGatePassCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("VMR Status 'IN' not found in MiscMaster.");
        }
    }
}
