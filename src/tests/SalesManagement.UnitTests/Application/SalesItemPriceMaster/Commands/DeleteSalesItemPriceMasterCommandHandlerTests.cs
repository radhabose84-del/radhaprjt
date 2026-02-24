#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.DeleteSalesItemPriceMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesItemPriceMaster.Commands
{
    /// <summary>
    /// Delete handler throws ExceptionRules when entity not found or soft-delete fails.
    /// </summary>
    public class DeleteSalesItemPriceMasterCommandHandlerTests
    {
        private readonly Mock<ISalesItemPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteSalesItemPriceMasterCommandHandler CreateSut() =>
            new DeleteSalesItemPriceMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupGetByIdReturnsDto(int id = 1, string priceCode = "PC001")
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(SalesItemPriceMasterBuilders.ValidDto(id: id, priceCode: priceCode));
        }

        private void SetupGetByIdReturnsNull(int id = 99)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterDto)null);
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
            var command = new DeleteSalesItemPriceMasterCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsSoftDeleteAsync_Once()
        {
            var command = new DeleteSalesItemPriceMasterCommand(1);
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
            var command = new DeleteSalesItemPriceMasterCommand(1);
            SetupGetByIdReturnsDto(id: 1, priceCode: "PC001");
            SetupSoftDeleteSuccess(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PC001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── Entity Not Found ──────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsExceptionRules()
        {
            var command = new DeleteSalesItemPriceMasterCommand(99);
            SetupGetByIdReturnsNull(id: 99);

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallSoftDelete()
        {
            var command = new DeleteSalesItemPriceMasterCommand(99);
            SetupGetByIdReturnsNull(id: 99);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ── Soft Delete Fails ─────────────────────────────────────────────────

        [Fact]
        public async Task Handle_SoftDeleteFails_ThrowsExceptionRules()
        {
            var command = new DeleteSalesItemPriceMasterCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteFails(id: 1);

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Failed to delete*");
        }

        [Fact]
        public async Task Handle_SoftDeleteFails_DoesNotPublishAuditEvent()
        {
            var command = new DeleteSalesItemPriceMasterCommand(1);
            SetupGetByIdReturnsDto(id: 1);
            SetupSoftDeleteFails(id: 1);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
