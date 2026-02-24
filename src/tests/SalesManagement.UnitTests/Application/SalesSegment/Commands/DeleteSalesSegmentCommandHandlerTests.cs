#nullable disable
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.DeleteSalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesSegment.Commands
{
    /// <summary>
    /// ⚠️ SalesSegment Delete returns false (not throw) when entity not found.
    /// SoftDelete failure also returns false with no audit event.
    /// </summary>
    public class DeleteSalesSegmentCommandHandlerTests
    {
        private readonly Mock<ISalesSegmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteSalesSegmentCommandHandler CreateSut() =>
            new DeleteSalesSegmentCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupGetByIdReturnsDto(int id = 1, string segmentName = "Test Segment")
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(SalesSegmentBuilders.ValidDto(id: id, segmentName: segmentName));
        }

        private void SetupGetByIdReturnsNull(int id = 99)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((SalesSegmentDto)null);
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
            var command = new DeleteSalesSegmentCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsSoftDeleteAsync_Once()
        {
            var command = new DeleteSalesSegmentCommand(1);
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
            var command = new DeleteSalesSegmentCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_SEGMENT_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsSegmentName()
        {
            var command = new DeleteSalesSegmentCommand(1);
            SetupGetByIdReturnsDto(id: 1, segmentName: "Finance Segment");
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "Finance Segment"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── Entity Not Found (returns false — no exception) ───────────────────

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsFalse()
        {
            var command = new DeleteSalesSegmentCommand(99);
            SetupGetByIdReturnsNull(id: 99);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallSoftDelete()
        {
            var command = new DeleteSalesSegmentCommand(99);
            SetupGetByIdReturnsNull(id: 99);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ── Soft Delete Fails ─────────────────────────────────────────────────

        [Fact]
        public async Task Handle_SoftDeleteFails_ReturnsFalse()
        {
            var command = new DeleteSalesSegmentCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteFails(id: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_SoftDeleteFails_DoesNotPublishAuditEvent()
        {
            var command = new DeleteSalesSegmentCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteFails(id: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
