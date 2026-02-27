using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Commands
{
    public class CreateItemPriceMasterCommandHandlerTests
    {
        private readonly Mock<IItemPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateItemPriceMasterCommandHandler CreateSut() =>
            new CreateItemPriceMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupHappyPath(CreateItemPriceMasterCommand command, int newId = 1)
        {
            var entity = ItemPriceMasterBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            const int expectedId = 42;
            SetupHappyPath(command, newId: expectedId);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(expectedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_MessageContainsCreated()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand(priceCode: "PC001");
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "ITEM_PRICE_CREATE" &&
                        e.Module == "ItemPriceMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsPriceCode()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand(priceCode: "PC999");
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "PC999"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            var capturedEntity = default(SalesManagement.Domain.Entities.ItemPriceMaster);

            var entity = new SalesManagement.Domain.Entities.ItemPriceMaster
            {
                PriceCode = command.PriceCode,
                ItemId = command.ItemId,
                SalesSegmentId = command.SalesSegmentId,
                ExMillPrice = command.ExMillPrice
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()))
                .Callback<SalesManagement.Domain.Entities.ItemPriceMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsIsDeletedNotDeleted()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            var capturedEntity = default(SalesManagement.Domain.Entities.ItemPriceMaster);

            var entity = new SalesManagement.Domain.Entities.ItemPriceMaster
            {
                PriceCode = command.PriceCode,
                ItemId = command.ItemId,
                SalesSegmentId = command.SalesSegmentId,
                ExMillPrice = command.ExMillPrice
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()))
                .Callback<SalesManagement.Domain.Entities.ItemPriceMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }
    }
}
