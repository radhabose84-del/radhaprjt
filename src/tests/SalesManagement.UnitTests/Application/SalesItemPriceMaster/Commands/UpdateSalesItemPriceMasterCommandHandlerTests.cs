#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.UpdateSalesItemPriceMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesItemPriceMaster.Commands
{
    public class UpdateSalesItemPriceMasterCommandHandlerTests
    {
        private readonly Mock<ISalesItemPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateSalesItemPriceMasterCommandHandler CreateSut() =>
            new UpdateSalesItemPriceMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupGetByIdReturnsDto(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(SalesItemPriceMasterBuilders.ValidDto(id: id));
        }

        private void SetupGetByIdReturnsNull(int id = 99)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterDto)null);
        }

        private void SetupUpdateAsync(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesItemPriceMaster>()))
                .ReturnsAsync(id);
        }

        private void SetupMapper(UpdateSalesItemPriceMasterCommand command)
        {
            var entity = SalesItemPriceMasterBuilders.ValidEntity(command.Id);
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesItemPriceMaster>(command))
                .Returns(entity);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
            SetupGetByIdReturnsDto(id: 1);
            SetupMapper(command);
            SetupUpdateAsync(id: 1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 7);
            SetupGetByIdReturnsDto(id: 7);
            SetupMapper(command);
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesItemPriceMaster>()))
                .ReturnsAsync(7);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
            SetupGetByIdReturnsDto(id: 1);
            SetupMapper(command);
            SetupUpdateAsync(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesItemPriceMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
            SetupGetByIdReturnsDto(id: 1);
            SetupMapper(command);
            SetupUpdateAsync(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionCode == "SALES_ITEM_PRICE_UPDATE" &&
                        e.Module == "SalesItemPriceMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsPriceCode()
        {
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(SalesItemPriceMasterBuilders.ValidDto(id: 1, priceCode: "PC001"));
            SetupMapper(command);
            SetupUpdateAsync(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "PC001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── Entity Not Found ──────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 99);
            SetupGetByIdReturnsNull(id: 99);

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallUpdateAsync()
        {
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 99);
            SetupGetByIdReturnsNull(id: 99);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesItemPriceMaster>()),
                Times.Never);
        }
    }
}
