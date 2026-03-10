using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Commands
{
    public class CreateItemPriceMasterCommandHandlerTests
    {
        private readonly Mock<IItemPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceQueryRepository> _mockDocSeqQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateItemPriceMasterCommandHandler CreateSut() =>
            new CreateItemPriceMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockMiscMasterQueryRepo.Object,
                _mockDocSeqQueryRepo.Object,
                _mockIpAddressService.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupHappyPath(CreateItemPriceMasterCommand command, int newId = 1)
        {
            var entity = new SalesManagement.Domain.Entities.ItemPriceMaster
            {
                ItemId = command.ItemId,
                SalesSegmentId = command.SalesSegmentId,
                ExMillRate = command.ExMillRate
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
                .Returns(entity);

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

            _mockIpAddressService
                .Setup(s => s.GetUnitId())
                .Returns(1);

            _mockDocSeqQueryRepo
                .Setup(r => r.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(5);

            _mockDocSeqQueryRepo
                .Setup(r => r.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "UNIT-PM-2526-0001" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>(), It.IsAny<int>()))
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
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>(), It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
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
        public async Task Handle_ValidCommand_SetsPriceCodeFromDocumentSequence()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.ItemPriceMaster? capturedEntity = null;

            var entity = new SalesManagement.Domain.Entities.ItemPriceMaster
            {
                ItemId = command.ItemId,
                SalesSegmentId = command.SalesSegmentId,
                ExMillRate = command.ExMillRate
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
                .Returns(entity);

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

            _mockIpAddressService
                .Setup(s => s.GetUnitId())
                .Returns(1);

            _mockDocSeqQueryRepo
                .Setup(r => r.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(5);

            _mockDocSeqQueryRepo
                .Setup(r => r.GenerateDocumentNumber(5))
                .ReturnsAsync(new List<string> { "UNIT-PM-2526-0001" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>(), It.IsAny<int>()))
                .Callback<SalesManagement.Domain.Entities.ItemPriceMaster, int>((e, _) => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.PriceCode.Should().Be("UNIT-PM-2526-0001");
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
        public async Task Handle_ValidCommand_GetsUnitIdFromToken()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockIpAddressService.Verify(s => s.GetUnitId(), Times.Once);
        }

        [Fact]
        public async Task Handle_TransactionTypeNotFound_ThrowsExceptionRules()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();

            var entity = new SalesManagement.Domain.Entities.ItemPriceMaster
            {
                ItemId = command.ItemId,
                SalesSegmentId = command.SalesSegmentId,
                ExMillRate = command.ExMillRate
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
                .Returns(entity);

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

            _mockIpAddressService
                .Setup(s => s.GetUnitId())
                .Returns(1);

            _mockDocSeqQueryRepo
                .Setup(r => r.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*PriceMaster*");
        }

        [Fact]
        public async Task Handle_NoDocumentSequence_ThrowsExceptionRules()
        {
            var command = ItemPriceMasterBuilders.ValidCreateCommand();

            var entity = new SalesManagement.Domain.Entities.ItemPriceMaster
            {
                ItemId = command.ItemId,
                SalesSegmentId = command.SalesSegmentId,
                ExMillRate = command.ExMillRate
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(command))
                .Returns(entity);

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

            _mockIpAddressService
                .Setup(s => s.GetUnitId())
                .Returns(1);

            _mockDocSeqQueryRepo
                .Setup(r => r.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(5);

            _mockDocSeqQueryRepo
                .Setup(r => r.GenerateDocumentNumber(5))
                .ReturnsAsync(new List<string>());

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*document sequence*");
        }
    }
}
