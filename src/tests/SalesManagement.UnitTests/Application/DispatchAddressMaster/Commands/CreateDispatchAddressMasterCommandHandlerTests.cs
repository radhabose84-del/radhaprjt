using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.CreateDispatchAddressMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.DispatchAddressMaster.Commands
{
    public sealed class CreateDispatchAddressMasterCommandHandlerTests
    {
        private readonly Mock<IDispatchAddressMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateDispatchAddressMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateDispatchAddressMasterCommand command, int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command))
                .Returns(DispatchAddressMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessMessage()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().ContainEquivalentOf("success");
        }

        // ── Repository ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMaster>()),
                Times.Once);
        }

        // ── Entity State ──────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityActiveAndNotDeleted()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.DispatchAddressMaster? capturedEntity = null;

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command))
                .Returns(DispatchAddressMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMaster>()))
                .Callback<SalesManagement.Domain.Entities.DispatchAddressMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity!.IsActive.Should().Be(BaseEntity.Status.Active);
            capturedEntity!.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        // ── Audit Event ───────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEventWithCorrectCodes()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "DISPATCH_ADDRESS_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEventOnce()
        {
            var command = DispatchAddressMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
