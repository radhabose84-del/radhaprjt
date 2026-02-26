using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateMiscMasterCommand command, int newId = 1, int maxSortOrder = 0)
        {
            _mockCommandRepo
                .Setup(r => r.GetMaxSortOrderAsync(command.MiscTypeId))
                .ReturnsAsync(maxSortOrder);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MiscMaster>(command))
                .Returns(MiscMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessMessage()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().ContainEquivalentOf("success");
        }

        // ── SortOrder ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_CallsGetMaxSortOrderOnce()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.GetMaxSortOrderAsync(command.MiscTypeId), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsSortOrderToMaxPlusOne()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.MiscMaster? capturedEntity = null;

            _mockCommandRepo
                .Setup(r => r.GetMaxSortOrderAsync(command.MiscTypeId))
                .ReturnsAsync(4);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MiscMaster>(command))
                .Returns(MiscMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscMaster>()))
                .Callback<SalesManagement.Domain.Entities.MiscMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity!.SortOrder.Should().Be(5);
        }

        // ── Repository ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        // ── Entity State ──────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityActiveAndNotDeleted()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.MiscMaster? capturedEntity = null;

            _mockCommandRepo
                .Setup(r => r.GetMaxSortOrderAsync(command.MiscTypeId))
                .ReturnsAsync(0);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MiscMaster>(command))
                .Returns(MiscMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscMaster>()))
                .Callback<SalesManagement.Domain.Entities.MiscMaster>(e => capturedEntity = e)
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
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "MISC_MASTER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
