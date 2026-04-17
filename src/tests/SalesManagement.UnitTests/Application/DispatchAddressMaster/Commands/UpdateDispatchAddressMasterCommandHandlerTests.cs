using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.DispatchAddressMaster.Commands
{
    public sealed class UpdateDispatchAddressMasterCommandHandlerTests
    {
        private readonly Mock<IDispatchAddressMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDispatchAddressMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateDispatchAddressMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(UpdateDispatchAddressMasterCommand command, int updatedId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command))
                .Returns(DispatchAddressMasterBuilders.ValidEntity(updatedId));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMaster>()))
                .ReturnsAsync(updatedId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand(id: 7);
            SetupHappyPath(command, updatedId: 7);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessMessage()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().ContainEquivalentOf("success");
        }

        // ── Repository ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMaster>()),
                Times.Once);
        }

        // ── Audit Event ───────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEventWithCorrectCodes()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "DISPATCH_ADDRESS_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEventOnce()
        {
            var command = DispatchAddressMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
