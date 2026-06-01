using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
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
        private readonly Mock<IItemLookup> _item = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);

        private CreateQcInspectionCommandHandler CreateSut() =>
            new(_cmd.Object, _qry.Object, _grn.Object, _item.Object, _mediator.Object, _ip.Object);

        private void SetupHappyPath(int newId = 88)
        {
            _grn.Setup(g => g.GetByGrnDetailIdAsync(4321, It.IsAny<CancellationToken>()))
                .ReturnsAsync(QcInspectionBuilders.ValidGrnLookup());
            _item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { QcInspectionBuilders.ValidItemLookup() });
            _qry.Setup(q => q.GetPurchasedGoodsQcTypeIdAsync()).ReturnsAsync(8);
            _qry.Setup(q => q.ResolveActiveSpecIdAsync(7, 9, 8, It.IsAny<DateTimeOffset>())).ReturnsAsync(5);
            _qry.Setup(q => q.GetSpecSnapshotAsync(5)).ReturnsAsync(QcInspectionBuilders.ValidSnapshot());
            _qry.Setup(q => q.GetMaxInspectionSequenceAsync(It.IsAny<int>())).ReturnsAsync(0);
            _cmd.Setup(c => c.CreateAsync(It.IsAny<QcInspectionHdr>())).ReturnsAsync(newId);
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
        public async Task Handle_Valid_ReturnsNewId()
        {
            SetupHappyPath(newId: 99);
            var result = await CreateSut().Handle(QcInspectionBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Data.Should().Be(99);
        }

        [Fact]
        public async Task Handle_Valid_SnapshotsParametersIntoDetails()
        {
            QcInspectionHdr? captured = null;
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
    }
}
