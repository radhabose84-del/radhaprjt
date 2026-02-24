#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.BusinessUnit.Commands.DeleteBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.BusinessUnit.Commands
{
    /// <summary>
    /// ⚠️ BusinessUnit Delete differs from SalesChannel/SalesOrganisation:
    /// - Not found → throws EntityNotFoundException (not ExceptionRules)
    /// - SoftDeleteAsync returns false → returns false (no exception)
    /// </summary>
    public class DeleteBusinessUnitCommandHandlerTests
    {
        private readonly Mock<IBusinessUnitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IBusinessUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteBusinessUnitCommandHandler CreateSut() =>
            new DeleteBusinessUnitCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupGetByIdReturnsDto(int id = 1, string code = "BU001")
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(BusinessUnitBuilders.ValidDto(id: id, code: code));
        }

        private void SetupGetByIdReturnsNull(int id = 99)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((SalesManagement.Application.BusinessUnit.Dto.BusinessUnitDto)null);
        }

        private void SetupSoftDeleteSuccess(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        private void SetupSoftDeleteFails(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_SoftDeleteSucceeds_ReturnsTrue()
        {
            var command = new DeleteBusinessUnitCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsSoftDeleteAsync_Once()
        {
            var command = new DeleteBusinessUnitCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            var command = new DeleteBusinessUnitCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "BUSINESSUNIT_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsBusinessUnitCode()
        {
            var command = new DeleteBusinessUnitCommand(1);
            SetupGetByIdReturnsDto(id: 1, code: "BU001");
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "BU001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── Not Found Path ────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            var command = new DeleteBusinessUnitCommand(99);
            SetupGetByIdReturnsNull(id: 99);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*Business Unit*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallSoftDelete()
        {
            var command = new DeleteBusinessUnitCommand(99);
            SetupGetByIdReturnsNull(id: 99);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ── Soft Delete Fails (returns false — no exception) ──────────────────

        [Fact]
        public async Task Handle_SoftDeleteFails_ReturnsFalse()
        {
            var command = new DeleteBusinessUnitCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteFails(id: 1);
            // No audit publish expected when deleted = false

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_SoftDeleteFails_DoesNotPublishAuditEvent()
        {
            var command = new DeleteBusinessUnitCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteFails(id: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
