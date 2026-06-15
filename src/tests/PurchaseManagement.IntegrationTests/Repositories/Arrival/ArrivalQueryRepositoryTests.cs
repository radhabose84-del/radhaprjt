using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using PurchaseManagement.Application.Arrival.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Gate;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Dtos.Lookups.QC;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.QC;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Domain.Entities.Arrival;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Arrival;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Arrival
{
    [Collection("DatabaseCollection")]
    public sealed class ArrivalQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IDocumentSequenceLookup> _docSeqMock = new(MockBehavior.Loose);
        private readonly Mock<ISupplierLookup> _supplierMock = new(MockBehavior.Loose);
        private readonly Mock<IStationLookup> _stationMock = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _warehouseMock = new(MockBehavior.Loose);
        private readonly Mock<ITransporterLookup> _transporterMock = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _itemMock = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _hsnMock = new(MockBehavior.Loose);
        private readonly Mock<IPackTypeLookup> _packTypeMock = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _uomMock = new(MockBehavior.Loose);
        private readonly Mock<IQcMiscMasterLookup> _qcMiscMock = new(MockBehavior.Loose);
        private readonly Mock<IQualitySpecificationLookup> _qualitySpecMock = new(MockBehavior.Loose);
        private readonly Mock<IVehicleMovementRecordLookup> _vmrMock = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _ipMock = new(MockBehavior.Loose);

        public ArrivalQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _ipMock.Setup(i => i.GetUnitId()).Returns(1);
            _docSeqMock
                .Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                .Returns(Task.CompletedTask);
            _supplierMock.Setup(s => s.GetActiveSupplierByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SupplierLookupDto { Id = 10, VendorName = "Sree Lakshmi Cotton" });
            _transporterMock.Setup(t => t.GetActiveTransporterByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransporterLookupDto { Id = 7, TransporterName = "TCI Freight" });
            _stationMock.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StationLookupDto> { new() { Id = 12, StationName = "Coimbatore" } });
            _warehouseMock.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto> { new() { Id = 5, WarehouseName = "Godown-1" } });
            _itemMock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 13, ItemName = "Cotton MCU-5" } });
            _hsnMock.Setup(h => h.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HSNLookupDto> { new() { Id = 1, HSNCode = "5201" } });
            _packTypeMock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PackTypeLookupDto> { new() { Id = 2, PackTypeName = "Bale" } });
            _uomMock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new() { Id = 4, UOMName = "Bale" } });
            // QcStatusId → QC.MiscMaster cross-module lookup — echo the requested ids so QcStatusName resolves.
            _qcMiscMock.Setup(q => q.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new QcMiscMasterLookupDto { Id = id, Code = "QCS", Description = "QC Status" }).ToList());
            // Item 13 has a QC quality specification → pending arrivals for it are QC-applicable.
            _qualitySpecMock.Setup(q => q.GetMatchingAsync(
                    It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QualitySpecificationMatchDto { MatchedItemIds = new HashSet<int> { 13 } });
        }

        private ArrivalQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ArrivalQueryRepository(conn, _supplierMock.Object, _stationMock.Object, _warehouseMock.Object,
                _transporterMock.Object, _itemMock.Object, _hsnMock.Object, _packTypeMock.Object, _uomMock.Object,
                _qcMiscMock.Object, _qualitySpecMock.Object, _vmrMock.Object, _ipMock.Object);
        }

        private ArrivalCommandRepository CreateCommandRepo(ApplicationDbContext ctx) =>
            new(ctx, _docSeqMock.Object);

        private static ArrivalHeader BuildHeader(int rmpoId, int qcId, int statusId, string arrivalNumber) =>
            new()
            {
                UnitId = 1, ArrivalNumber = arrivalNumber, ArrivalDate = DateTimeOffset.UtcNow,
                RawMaterialPOId = rmpoId, VehicleNumber = "TN-38-BC-4521",
                SupplierId = 10, StationId = 12, GodownId = 5, TransporterId = 7,
                GrossWeight = 30000m, TareWeight = 10000m, NetWeight = 20000m, PartyWeight = 19900m, WeightDifference = -100m,
                QcStatusId = qcId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                ArrivalDetails = new List<ArrivalDetail>
                {
                    new()
                    {
                        ItemId = 13, HsnId = 1, PackTypeId = 2, MixCodeId = 3, UomId = 4,
                        Rate = 68500m, OrderedQty = 500m, ArrivedQty = 150m, CancelledQty = 0m, BalanceQty = 350m,
                        BatchNumber = "BATCH-0012-A", BaleNumberFrom = 100001, BaleNumberTo = 100003, TotalBaleCount = 3
                    }
                }
            };

        private async Task<(int rmpoId, int qcId, int statusId)> SeedAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscTypeMaster
                { MiscTypeCode = "MT001", Description = "Test Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var qc = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                { Code = "PEND", Description = "Pending", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var status = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                { Code = "DRFT", Description = "Draft", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var paymentTerm = new PurchaseManagement.Domain.Entities.PaymentTermMaster
            { Code = "PT001", Description = "30 Days", BaselineTypeId = qc.Id, CreditDays = 30, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<PurchaseManagement.Domain.Entities.PaymentTermMaster>().Add(paymentTerm);
            await ctx.SaveChangesAsync();

            var ocr = new PurchaseManagement.Domain.Entities.OCREntry
            {
                OcrNumber = "OCR-2025-0004", OcrDate = DateTimeOffset.UtcNow,
                ProcurementSourceId = status.Id, ProcurementTypeId = status.Id, BrokerDirectId = status.Id,
                StatusId = qc.Id, PaymentTermId = paymentTerm.Id,
                SupplierId = 10, LocationId = 11, StationId = 12, ItemId = 13, CountId = 14,
                Quantity = 800m, Rate = 68500m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>().Add(ocr);
            await ctx.SaveChangesAsync();

            var rmpo = new RawMaterialPOHeader
            {
                UnitId = 1, PONumber = "PO-2025-0012", PODate = DateTimeOffset.UtcNow,
                OcrId = ocr.Id, ProcurementDocumentTypeId = status.Id, StatusId = qc.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<RawMaterialPOHeader>().Add(rmpo);
            await ctx.SaveChangesAsync();

            return (rmpo.Id, qc.Id, status.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Details_And_Names()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedAsync(ctx);
            var id = await CreateCommandRepo(ctx).CreateAsync(BuildHeader(rmpoId, qcId, statusId, "ARV-Q-0001"), 0, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.ArrivalNumber.Should().Be("ARV-Q-0001");
            dto.PONumber.Should().Be("PO-2025-0012");
            dto.SupplierName.Should().Be("Sree Lakshmi Cotton");
            dto.TransporterName.Should().Be("TCI Freight");
            dto.StationName.Should().Be("Coimbatore");
            dto.GodownName.Should().Be("Godown-1");
            dto.Details.Should().HaveCount(1);
            dto.Details[0].ItemName.Should().Be("Cotton MCU-5");
            dto.Details[0].HsnCode.Should().Be("5201");
            dto.Details[0].PackTypeName.Should().Be("Bale");
            dto.Details[0].UomName.Should().Be("Bale");
        }

        [Fact]
        public async Task GetByIdAsync_Individual_Should_Return_Bales_Array()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedAsync(ctx);

            var header = BuildHeader(rmpoId, qcId, statusId, "ARV-Q-IND-1");
            header.StockRows = ArrivalStockLedgerFactory.Build(
                header.ArrivalDate,
                new[]
                {
                    new ArrivalStockLedgerFactory.LineInput(13, 4, new[]
                    {
                        new ArrivalStockLedgerFactory.BaleEntry(100001, 221.5m, 900001),
                        new ArrivalStockLedgerFactory.BaleEntry(100002, 223.0m, 900002)
                    })
                });
            var id = await CreateCommandRepo(ctx).CreateAsync(header, 0, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            var line = dto!.Details.Single(d => d.ItemId == 13);
            line.Bales.Should().HaveCount(2);
            line.Bales[0].BaleNo.Should().Be(100001);
            line.Bales[0].BarcodeNumber.Should().Be(900001);
            line.Bales[0].BaleWeight.Should().Be(221.5m);
        }

        [Fact]
        public async Task GetByIdAsync_Consolidated_Should_Return_Empty_Bales()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedAsync(ctx);

            var header = BuildHeader(rmpoId, qcId, statusId, "ARV-Q-CON-1");
            // No bale entries in the payload → no StockLedgerRaw rows → empty Bales on read.
            header.StockRows = ArrivalStockLedgerFactory.Build(
                header.ArrivalDate,
                new[] { new ArrivalStockLedgerFactory.LineInput(13, 4, null) });
            var id = await CreateCommandRepo(ctx).CreateAsync(header, 0, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            var line = dto!.Details.Single(d => d.ItemId == 13);
            line.Bales.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedAsync(ctx);
            await CreateCommandRepo(ctx).CreateAsync(BuildHeader(rmpoId, qcId, statusId, "ARV-Q-0002"), 0, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_PendingStatus_Filters_By_QcStatus()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedAsync(ctx);

            // QC signed off — QcStatusId set (not pending)
            await CreateCommandRepo(ctx).CreateAsync(BuildHeader(rmpoId, qcId, statusId, "ARV-Q-DONE"), 0, CancellationToken.None);

            // Pending QC — QcStatusId null, distinct bale range
            var pending = BuildHeader(rmpoId, qcId, statusId, "ARV-Q-PEND");
            pending.QcStatusId = null;
            pending.ArrivalDetails!.First().BaleNumberFrom = 200001;
            pending.ArrivalDetails!.First().BaleNumberTo = 200003;
            await CreateCommandRepo(ctx).CreateAsync(pending, 0, CancellationToken.None);

            var (_, allTotal) = await CreateQueryRepo().GetAllAsync(1, 10, null, null);
            var (pendingItems, pendingTotal) = await CreateQueryRepo().GetAllAsync(1, 10, null, true);
            var (doneItems, doneTotal) = await CreateQueryRepo().GetAllAsync(1, 10, null, false);

            allTotal.Should().Be(2);
            pendingTotal.Should().Be(1);
            pendingItems.Should().ContainSingle(x => x.ArrivalNumber == "ARV-Q-PEND");
            doneTotal.Should().Be(1);
            doneItems.Should().ContainSingle(x => x.ArrivalNumber == "ARV-Q-DONE");
        }

        [Fact]
        public async Task GetAllAsync_Pending_Excludes_Arrivals_With_No_QcSpec()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedAsync(ctx);

            var pending = BuildHeader(rmpoId, qcId, statusId, "ARV-Q-NOQC");
            pending.QcStatusId = null;
            await CreateCommandRepo(ctx).CreateAsync(pending, 0, CancellationToken.None);

            // No quality specification matches any item/category → QC inspection does not apply →
            // the pending arrival must not appear in the pending list.
            _qualitySpecMock.Setup(q => q.GetMatchingAsync(
                    It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QualitySpecificationMatchDto());

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, true);

            total.Should().Be(0);
            items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedAsync(ctx);
            var id = await CreateCommandRepo(ctx).CreateAsync(BuildHeader(rmpoId, qcId, statusId, "ARV-Q-0003"), 0, CancellationToken.None);
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task RawMaterialPOExistsAsync_Should_Return_True_For_Seeded_PO()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, _, _) = await SeedAsync(ctx);

            var exists = await CreateQueryRepo().RawMaterialPOExistsAsync(rmpoId);

            exists.Should().BeTrue();
        }
    }
}
