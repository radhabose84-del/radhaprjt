using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Arrival.Common;
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
    public sealed class ArrivalCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IDocumentSequenceLookup> _docSeqMock = new(MockBehavior.Loose);

        public ArrivalCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _docSeqMock
                .Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                .Returns(Task.CompletedTask);
        }

        private ArrivalCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx, _docSeqMock.Object);

        private static ArrivalHeader BuildHeader(
            int rmpoId, int qcStatusId, int statusId,
            string arrivalNumber = "ARV-2025-0006",
            long baleFrom = 100001, long baleTo = 100005)
        {
            var header = new ArrivalHeader
            {
                UnitId = 1,
                ArrivalNumber = arrivalNumber,
                ArrivalDate = DateTimeOffset.UtcNow,
                RawMaterialPOId = rmpoId,
                VehicleNumber = "TN-38-BC-4521",
                SupplierId = 10,
                StationId = 12,
                GodownId = 5,
                TransporterId = 7,
                FreightRate = 1200m,
                GrossWeight = 30000m,
                TareWeight = 10000m,
                NetWeight = 20000m,
                PartyWeight = 19900m,
                WeightDifference = -100m,
                MoisturePercentage = 7.5m,
                GstPercentage = 5m,
                QcStatusId = qcStatusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ArrivalDetails = new List<ArrivalDetail>
                {
                    new()
                    {
                        ItemId = 13, HsnId = 1, PackTypeId = 2, MixCodeId = 3, UomId = 4,
                        Rate = 68500m, OrderedQty = 500m, ArrivedQty = 150m, CancelledQty = 0m, BalanceQty = 350m,
                        BatchNumber = "BATCH-0012-A",
                        BaleNumberFrom = baleFrom, BaleNumberTo = baleTo,
                        TotalBaleCount = (int)(baleTo - baleFrom + 1)
                    }
                }
            };

            // StockLedgerRaw rows are built by the handler/factory; mirror that here for the repo test.
            // The factory saves payload bale entries verbatim — expand the range into explicit
            // per-bale entries (even-split weight, no barcode) to represent the payload.
            var count = baleTo - baleFrom + 1;
            var perBaleWeight = count > 0 ? header.NetWeight / count : 0m;
            var bales = new List<ArrivalStockLedgerFactory.BaleEntry>();
            for (var b = baleFrom; b <= baleTo; b++)
                bales.Add(new ArrivalStockLedgerFactory.BaleEntry(b, perBaleWeight, null));

            header.StockRows = ArrivalStockLedgerFactory.Build(
                header.ArrivalDate,
                new[] { new ArrivalStockLedgerFactory.LineInput(13, 4, bales) });

            return header;
        }

        private async Task<(int rmpoId, int qcStatusId, int statusId)> SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscTypeMaster
                { MiscTypeCode = "MT001", Description = "Test Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var qcStatus = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                { Code = "PEND", Description = "Pending", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var status = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                { Code = "DRFT", Description = "Draft", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var paymentTerm = new PurchaseManagement.Domain.Entities.PaymentTermMaster
            { Code = "PT001", Description = "30 Days", BaselineTypeId = qcStatus.Id, CreditDays = 30, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<PurchaseManagement.Domain.Entities.PaymentTermMaster>().Add(paymentTerm);
            await ctx.SaveChangesAsync();

            var ocr = new PurchaseManagement.Domain.Entities.OCREntry
            {
                OcrNumber = "OCR-2025-0004", OcrDate = DateTimeOffset.UtcNow,
                ProcurementSourceId = status.Id, ProcurementTypeId = status.Id,
                StatusId = qcStatus.Id, PaymentTermId = paymentTerm.Id,
                SupplierId = 10, LocationId = 11, StationId = 12, ItemId = 13, CountId = 14,
                Quantity = 800m, Rate = 68500m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>().Add(ocr);
            await ctx.SaveChangesAsync();

            var rmpo = new RawMaterialPOHeader
            {
                UnitId = 1, PONumber = "PO-2025-0012", PODate = DateTimeOffset.UtcNow,
                OcrId = ocr.Id, ProcurementDocumentTypeId = status.Id, StatusId = qcStatus.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<RawMaterialPOHeader>().Add(rmpo);
            await ctx.SaveChangesAsync();

            return (rmpo.Id, qcStatus.Id, status.Id);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(rmpoId, qcId, statusId), 0, CancellationToken.None);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildHeader(rmpoId, qcId, statusId, "ARV-2025-0007"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<ArrivalHeader>().Include(h => h.ArrivalDetails).FirstAsync(x => x.Id == newId);
            saved.ArrivalNumber.Should().Be("ARV-2025-0007");
            saved.NetWeight.Should().Be(20000m);
            saved.GstPercentage.Should().Be(5m);
            saved.ArrivalDetails.Should().HaveCount(1);
            saved.ArrivalDetails!.First().BalanceQty.Should().Be(350m);
        }

        [Fact]
        public async Task CreateAsync_Should_Generate_StockLedger_PerBale()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedPrerequisitesAsync(ctx);

            // Range 100001..100005 → 5 bale rows
            var newId = await CreateRepository(ctx).CreateAsync(
                BuildHeader(rmpoId, qcId, statusId, "ARV-2025-0008", 100001, 100005), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var ledger = await ctx.Set<StockLedgerRaw>().Where(s => s.LotNo == newId).OrderBy(s => s.BaleNo).ToListAsync();
            ledger.Should().HaveCount(5);
            ledger.First().BaleNo.Should().Be(100001);             // BaleNo holds the bale number
            ledger.Last().BaleNo.Should().Be(100005);
            ledger.Should().OnlyContain(x => x.BarcodeNumber == null);   // consolidated → null barcode
            ledger.Should().OnlyContain(x => x.DocType == "ARV");
            // Even split: NetWeight 20000 / 5 bales = 4000 per bale
            ledger.Should().OnlyContain(x => x.BaleWeight == 4000m);
            ledger.Should().OnlyContain(x => x.DocDate == ledger.First().DocDate);
        }

        [Fact]
        public async Task CreateAsync_Individual_Should_Persist_Payload_BaleWeights()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedPrerequisitesAsync(ctx);

            var header = BuildHeader(rmpoId, qcId, statusId, "ARV-2025-0099", 100001, 100002);
            // Override with an Individual capture (barcode scan → BarcodeNumber present, explicit weights).
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

            var newId = await CreateRepository(ctx).CreateAsync(header, 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var ledger = await ctx.Set<StockLedgerRaw>().Where(s => s.LotNo == newId).OrderBy(s => s.BaleNo).ToListAsync();
            ledger.Should().HaveCount(2);
            ledger[0].BarcodeNumber.Should().Be(900001);
            ledger[0].BaleWeight.Should().Be(221.5m);
            ledger[1].BarcodeNumber.Should().Be(900002);
            ledger[1].BaleWeight.Should().Be(223.0m);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildHeader(rmpoId, qcId, statusId, "ARV-2025-0009"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<ArrivalHeader>().FirstAsync(x => x.Id == newId);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        [Fact]
        public async Task UpdateAsync_Should_Regenerate_StockLedger()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedPrerequisitesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(
                BuildHeader(rmpoId, qcId, statusId, "ARV-2025-0010", 100001, 100005), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var update = BuildHeader(rmpoId, qcId, statusId, "ARV-2025-0010", 100001, 100003);
            update.Id = newId;
            await CreateRepository(ctx).UpdateAsync(update, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var ledger = await ctx.Set<StockLedgerRaw>()
                .Where(s => s.LotNo == newId).ToListAsync();
            ledger.Should().HaveCount(3);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (rmpoId, qcId, statusId) = await SeedPrerequisitesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(
                BuildHeader(rmpoId, qcId, statusId, "ARV-2025-0011"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(newId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Set<ArrivalHeader>().IgnoreQueryFilters().FirstAsync(x => x.Id == newId);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
