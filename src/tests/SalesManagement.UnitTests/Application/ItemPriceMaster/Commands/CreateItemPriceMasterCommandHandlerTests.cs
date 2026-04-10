using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Commands
{
    public class CreateItemPriceMasterCommandHandlerTests
    {
        private readonly Mock<IItemPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpAddress = new();
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateItemPriceMasterCommandHandler CreateSut() =>
            new(
                _mockCommandRepo.Object,
                _mockMiscMasterQueryRepo.Object,
                _mockDocSeqLookup.Object,
                _mockItemLookup.Object,
                _mockIpAddress.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        private void SetupHappyPath(CreateItemPriceMasterCommand cmd, int newId = 1)
        {
            // Pending status
            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 100 });

            // UnitId from IP
            _mockIpAddress.Setup(s => s.GetUnitId()).Returns(1);

            // Transaction type
            _mockDocSeqLookup
                .Setup(r => r.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            // Document number
            _mockDocSeqLookup
                .Setup(r => r.GenerateDocumentNumber(10))
                .ReturnsAsync(new List<string> { "PM-001" } as IReadOnlyList<string>);

            // Mapper
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(cmd))
                .Returns(new SalesManagement.Domain.Entities.ItemPriceMaster
                {
                    ItemId = cmd.ItemId,
                    VariantId = cmd.VariantId,
                    SalesSegmentId = cmd.SalesSegmentId,
                    BaseRate = cmd.BaseRate,
                    CurrencyId = cmd.CurrencyId
                });

            // Create
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>(), It.IsAny<int>()))
                .ReturnsAsync(newId);

            // Audit
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static CreateItemPriceMasterCommand ValidCommand() => new()
        {
            ItemId = 1,
            VariantId = 5,
            SalesSegmentId = 2,
            BaseRate = 100m,
            CurrencyId = 1,
            ValidFrom = DateOnly.FromDateTime(DateTime.Today),
            ValidTo = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
        };

        [Fact]
        public async Task Handle_SingleVariant_ReturnsSuccess()
        {
            var command = ValidCommand();
            SetupHappyPath(command, 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SingleVariant_ReturnsNewId()
        {
            var command = ValidCommand();
            SetupHappyPath(command, 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_SingleVariant_PublishesAuditEvent()
        {
            var command = ValidCommand();
            SetupHappyPath(command, 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "ITEM_PRICE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoVariantId_NoVariantsExist_CreatesSingleRecord()
        {
            var command = ValidCommand();
            command.VariantId = null;
            SetupHappyPath(command, 1);

            // No variants exist for this parent item
            _mockItemLookup
                .Setup(r => r.GetVariantsByParentIdAsync(command.ItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>() as IReadOnlyList<ItemLookupDto>);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>(), It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoVariantId_VariantsExist_CreatesBulk()
        {
            var command = ValidCommand();
            command.VariantId = null;
            SetupHappyPath(command, 1);

            var variants = new List<ItemLookupDto>
            {
                new() { Id = 10, ItemCode = "V1", ItemName = "Variant1" },
                new() { Id = 11, ItemCode = "V2", ItemName = "Variant2" }
            };
            _mockItemLookup
                .Setup(r => r.GetVariantsByParentIdAsync(command.ItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variants as IReadOnlyList<ItemLookupDto>);

            _mockCommandRepo
                .Setup(r => r.CreateBulkAsync(It.IsAny<List<SalesManagement.Domain.Entities.ItemPriceMaster>>(), It.IsAny<int>()))
                .ReturnsAsync(new List<int> { 1, 2 });

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("2 variants");
        }

        [Fact]
        public async Task Handle_NoTransactionType_ThrowsExceptionRules()
        {
            var command = ValidCommand();
            SetupHappyPath(command, 1);

            // Override: no transaction type
            _mockDocSeqLookup
                .Setup(r => r.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Contracts.Common.ExceptionRules>()
                .WithMessage("*Transaction Type*");
        }
    }
}
