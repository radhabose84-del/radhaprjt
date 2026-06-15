using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Updates.Purchase;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Commands.CreateQcInspection;
using QCManagement.Domain.Entities;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QcInspection.Commands
{
    public class CreateQcInspectionCommandHandlerTests
    {
        private readonly Mock<IQcInspectionCommandRepository> _cmd = new(MockBehavior.Strict);
        private readonly Mock<IQcInspectionQueryRepository> _qry = new(MockBehavior.Strict);
        private readonly Mock<IGrnLookup> _grn = new(MockBehavior.Strict);
        private readonly Mock<IArrivalLookup> _arrival = new(MockBehavior.Strict);
        private readonly Mock<IArrivalQcUpdate> _arrivalQcUpdate = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _item = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);

        private CreateQcInspectionCommandHandler CreateSut() =>
            new(_cmd.Object, _qry.Object, _grn.Object, _arrival.Object, _arrivalQcUpdate.Object, _item.Object, _mediator.Object, _ip.Object);

        private void SetupHappyPath(int newId = 88)
        {
            _qry.Setup(q => q.GetSourceTypeIdByCodeAsync("ARRIVAL"))
                .ReturnsAsync(QcInspectionBuilders.ArrivalSourceTypeId);
            _grn.Setup(g => g.GetByGrnDetailIdAsync(4321, It.IsAny<CancellationToken>()))
                .ReturnsAsync(QcInspectionBuilders.ValidGrnLookup());
            _item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { QcInspectionBuilders.ValidItemLookup() });
            _qry.Setup(q => q.GetPurchasedGoodsQcTypeIdAsync()).ReturnsAsync(8);
            _qry.Setup(q => q.ResolveActiveSpecIdAsync(7, 9, 8, It.IsAny<DateTimeOffset>())).ReturnsAsync(5);
            _qry.Setup(q => q.GetSpecSnapshotAsync(5)).ReturnsAsync(QcInspectionBuilders.ValidSnapshot());
            _qry.Setup(q => q.GetMaxInspectionSequenceAsync(It.IsAny<int>())).ReturnsAsync(0);
            _cmd.Setup(c => c.CreateAsync(It.IsAny<QcInspectionHdr>())).ReturnsAsync(newId);
            _qry.Setup(q => q.GetByIdAsync(newId)).ReturnsAsync(QcInspectionBuilders.ValidDto(newId));
            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_Valid_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(QcInspectionBuilders.ValidCreateCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Valid_ReturnsCreatedInspection()
        {
            SetupHappyPath(newId: 99);
            var result = await CreateSut().Handle(QcInspectionBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(99);
        }

        [Fact]
        public async Task Handle_Valid_SnapshotsParametersIntoDetails()
        {
            QcInspectionHdr? captured = null;
            _qry.Setup(q => q.GetSourceTypeIdByCodeAsync("ARRIVAL"))
                .ReturnsAsync(QcInspectionBuilders.ArrivalSourceTypeId);
            _grn.Setup(g => g.GetByGrnDetailIdAsync(4321, It.IsAny<CancellationToken>()))
                .ReturnsAsync(QcInspectionBuilders.ValidGrnLookup());
            _item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { QcInspectionBuilders.ValidItemLookup() });
            _qry.Setup(q => q.GetPurchasedGoodsQcTypeIdAsync()).ReturnsAsync(8);
            _qry.Setup(q => q.ResolveActiveSpecIdAsync(7, 9, 8, It.IsAny<DateTimeOffset>())).ReturnsAsync(5);
            _qry.Setup(q => q.GetSpecSnapshotAsync(5)).ReturnsAsync(QcInspectionBuilders.ValidSnapshot());
            _qry.Setup(q => q.GetMaxInspectionSequenceAsync(It.IsAny<int>())).ReturnsAsync(0);
            _cmd.Setup(c => c.CreateAsync(It.IsAny<QcInspectionHdr>()))
                .Callback<QcInspectionHdr>(e => captured = e)
                .ReturnsAsync(88);
            _qry.Setup(q => q.GetByIdAsync(88)).ReturnsAsync(QcInspectionBuilders.ValidDto(88));
            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(QcInspectionBuilders.ValidCreateCommand(), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.Details.Should().HaveCount(1);
            captured.QcInspectionNo.Should().StartWith("QCI-");
            captured.QualitySpecificationCode.Should().Be("QS-0001");
        }

        [Fact]
        public async Task Handle_Valid_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(QcInspectionBuilders.ValidCreateCommand(), CancellationToken.None);

            _mediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "QC_INSPECTION_CREATE"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ArrivalSource_UsesArrivalLookupAndSetsSourceIds()
        {
            QcInspectionHdr? captured = null;
            _qry.Setup(q => q.GetSourceTypeIdByCodeAsync("ARRIVAL"))
                .ReturnsAsync(QcInspectionBuilders.ArrivalSourceTypeId);
            _arrival.Setup(a => a.GetByArrivalDetailIdAsync(7777, It.IsAny<CancellationToken>()))
                .ReturnsAsync(QcInspectionBuilders.ValidArrivalLookup());
            _item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { QcInspectionBuilders.ValidItemLookup() });
            _qry.Setup(q => q.GetPurchasedGoodsQcTypeIdAsync()).ReturnsAsync(8);
            _qry.Setup(q => q.ResolveActiveSpecIdAsync(7, 9, 8, It.IsAny<DateTimeOffset>())).ReturnsAsync(5);
            _qry.Setup(q => q.GetSpecSnapshotAsync(5)).ReturnsAsync(QcInspectionBuilders.ValidSnapshot());
            _qry.Setup(q => q.GetMaxInspectionSequenceAsync(It.IsAny<int>())).ReturnsAsync(0);
            _cmd.Setup(c => c.CreateAsync(It.IsAny<QcInspectionHdr>()))
                .Callback<QcInspectionHdr>(e => captured = e)
                .ReturnsAsync(88);
            _qry.Setup(q => q.GetByIdAsync(88)).ReturnsAsync(QcInspectionBuilders.ValidDto(88));
            // Arrival source marks the source ArrivalHeader as QC 'PENDING' after the inspection is created.
            _qry.Setup(q => q.GetQcStatusIdByCodeAsync("PENDING")).ReturnsAsync(10);
            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = QcInspectionBuilders.ValidCreateCommand(
                sourceTypeId: QcInspectionBuilders.ArrivalSourceTypeId, sourceDetailId: 7777);
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            captured.Should().NotBeNull();
            captured!.SourceTypeId.Should().Be(QcInspectionBuilders.ArrivalSourceTypeId);
            captured.SourceHeaderId.Should().Be(200);   // ArrivalHeaderId
            captured.SourceDetailId.Should().Be(7777);  // ArrivalDetailId
            captured.ReceivedQuantity.Should().Be(500m);
            _grn.Verify(g => g.GetByGrnDetailIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
