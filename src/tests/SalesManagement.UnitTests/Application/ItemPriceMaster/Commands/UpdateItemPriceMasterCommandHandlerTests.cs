using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Commands
{
    public class UpdateItemPriceMasterCommandHandlerTests
    {
        private readonly Mock<IItemPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateItemPriceMasterCommandHandler CreateSut() =>
            new UpdateItemPriceMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupUpdateAsync(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()))
                .ReturnsAsync(id);
        }

        private void SetupMapper(UpdateItemPriceMasterCommand command)
        {
            var entity = ItemPriceMasterBuilders.ValidEntity(command.Id);
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
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
            var command = ItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
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
            var command = ItemPriceMasterBuilders.ValidUpdateCommand(id: 7);
            SetupMapper(command);
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()))
                .ReturnsAsync(7);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = ItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
            SetupMapper(command);
            SetupUpdateAsync(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
            SetupMapper(command);
            SetupUpdateAsync(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionCode == "ITEM_PRICE_UPDATE" &&
                        e.Module == "ItemPriceMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsPriceCode()
        {
            var command = ItemPriceMasterBuilders.ValidUpdateCommand(id: 1);
            SetupMapper(command);
            SetupUpdateAsync(id: 1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
