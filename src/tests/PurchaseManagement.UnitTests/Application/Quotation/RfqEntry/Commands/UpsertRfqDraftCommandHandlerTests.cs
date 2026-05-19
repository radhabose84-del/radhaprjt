using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UpsertDraft;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Application.Quotation.RfqEntry.Commands
{
    public sealed class UpsertRfqDraftCommandHandlerTests
    {
        private readonly Mock<IRfqCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSequence = new(MockBehavior.Loose);

        private UpsertRfqDraftCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockDocSequence.Object);

        private void SetupDocSequence()
        {
            _mockDocSequence
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSequence
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "RFQ001" });
        }

        [Fact]
        public async Task Handle_CreateDraft_ReturnsNewId()
        {
            var command = new UpsertRfqDraftCommand { Id = null, InitiationTypeId = 1 };
            SetupDocSequence();
            _mockRepo.Setup(r => r.GetStatusIdByCodeAsync("DRAFT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Id.Should().Be(10);
            result.RfqCode.Should().Be("RFQ001");
        }

        [Fact]
        public async Task Handle_CreateDraft_CallsCreateOnce()
        {
            var command = new UpsertRfqDraftCommand { Id = null, InitiationTypeId = 1 };
            SetupDocSequence();
            _mockRepo.Setup(r => r.GetStatusIdByCodeAsync("DRAFT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockRepo.Verify(r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateDraft_CallsUpdateDraftPartialAsync()
        {
            var command = new UpsertRfqDraftCommand { Id = 5, InitiationTypeId = 2 };
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockRepo.Setup(r => r.GetAggregateTrackingAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RfqMaster { Id = 5, RfqCode = "RFQ005" });

            var result = await CreateSut().Handle(command, CancellationToken.None);

            _mockRepo.Verify(r => r.UpdateDraftPartialAsync(5, It.IsAny<RfqMaster>(), It.IsAny<List<RfqItem>?>(), It.IsAny<List<RfqSupplier>?>(), It.IsAny<CancellationToken>()), Times.Once);
            result.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_CreateDraft_WithItems_FiltersInvalidItems()
        {
            var command = new UpsertRfqDraftCommand
            {
                Id = null,
                InitiationTypeId = 1,
                Items = new List<DraftItemDto>
                {
                    new DraftItemDto { ItemId = 1, UomId = 1, Qty = 10 },
                    new DraftItemDto { ItemId = 0, UomId = 0, Qty = 0 } // invalid - should be filtered
                }
            };
            SetupDocSequence();
            _mockRepo.Setup(r => r.GetStatusIdByCodeAsync("DRAFT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            RfqMaster? captured = null;
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<RfqMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<RfqMaster, int, CancellationToken>((r, _, _) => captured = r)
                .ReturnsAsync(11);

            await CreateSut().Handle(command, CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.Items.Should().HaveCount(1);
        }
    }
}
