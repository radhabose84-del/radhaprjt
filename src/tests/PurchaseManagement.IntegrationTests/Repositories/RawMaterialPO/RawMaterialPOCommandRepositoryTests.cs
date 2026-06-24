using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.RawMaterialPO;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.RawMaterialPO
{
    [Collection("DatabaseCollection")]
    public sealed class RawMaterialPOCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IDocumentSequenceLookup> _docSeqMock = new(MockBehavior.Loose);

        public RawMaterialPOCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _docSeqMock
                .Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                .Returns(Task.CompletedTask);
        }

        private RawMaterialPOCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx, _docSeqMock.Object);

        private static RawMaterialPOHeader BuildHeader(int ocrId, int miscId, string poNumber = "RMPO-2026-0001") =>
            new()
            {
                UnitId = 1,
                PONumber = poNumber,
                PODate = DateTimeOffset.UtcNow,
                OcrId = ocrId,
                ProcurementDocumentTypeId = miscId,
                StatusId = miscId,
                TaxableTotal = 34_250_000m,
                TotalGstAmount = 1_712_500m,
                NetTotal = 35_962_500m,
                CropYear = "2024-2025",
                ArrivalType = "Spot",
                PassingDate = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                CreditDays = 30,
                CottonApprovedBy = "QA Lead",
                CottonApprovedOn = new DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero),
                DocumentPath = "RMPO-2026-0001.png",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                RawMaterialPODetails = new List<RawMaterialPODetail>
                {
                    new()
                    {
                        ItemId = 13, HsnId = 1, Quantity = 500m, Weight = 85000m, Rate = 68500m,
                        ItemValue = 34_250_000m, CGSTValue = 856_250m, SGSTValue = 856_250m, IGSTValue = 0m,
                        TotalGST = 1_712_500m, NetValue = 35_962_500m,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    }
                }
            };

        private async Task<(int ocrId, int miscId)> SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "MT001", Description = "Test Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                });

            var misc = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Code = "MSC001", Description = "Purchase Order", MiscTypeId = miscType.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                });

            var paymentTerm = new PurchaseManagement.Domain.Entities.PaymentTermMaster
            {
                Code = "PT001", Description = "30 Days", BaselineTypeId = misc.Id, CreditDays = 30,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseManagement.Domain.Entities.PaymentTermMaster>().Add(paymentTerm);
            await ctx.SaveChangesAsync();

            var ocr = new PurchaseManagement.Domain.Entities.OCREntry
            {
                OcrNumber = "OCR-2025-0004", OcrDate = DateTimeOffset.UtcNow,
                ProcurementSourceId = misc.Id, ProcurementTypeId = misc.Id,
                StatusId = misc.Id, PaymentTermId = paymentTerm.Id,
                SupplierId = 10, LocationId = 11, StationId = 12, ItemId = 13, CountId = 14,
                Quantity = 800m, Rate = 68500m,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>().Add(ocr);
            await ctx.SaveChangesAsync();

            return (ocr.Id, misc.Id);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(ocrId, miscId), 0, CancellationToken.None);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(ocrId, miscId, "RMPO-2026-0002"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<RawMaterialPOHeader>()
                .Include(h => h.RawMaterialPODetails)
                .FirstAsync(x => x.Id == newId);

            saved.PONumber.Should().Be("RMPO-2026-0002");
            saved.OcrId.Should().Be(ocrId);
            saved.RawMaterialPODetails.Should().HaveCount(1);
            saved.RawMaterialPODetails!.First().ItemValue.Should().Be(34_250_000m);
            saved.RawMaterialPODetails!.First().NetValue.Should().Be(35_962_500m);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_CottonFields_And_DocumentPath()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(ocrId, miscId, "RMPO-2026-0021"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<RawMaterialPOHeader>().FirstAsync(x => x.Id == newId);
            saved.CropYear.Should().Be("2024-2025");
            saved.ArrivalType.Should().Be("Spot");
            saved.CreditDays.Should().Be(30);
            saved.CottonApprovedBy.Should().Be("QA Lead");
            saved.PassingDate.Should().Be(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
            saved.CottonApprovedOn.Should().Be(new DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero));
            saved.DocumentPath.Should().Be("RMPO-2026-0001.png");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_CottonFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(ocrId, miscId, "RMPO-2026-0022"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var update = BuildHeader(ocrId, miscId, "RMPO-2026-0022");
            update.Id = newId;
            update.CropYear = "2025-2026";
            update.CreditDays = 45;
            update.DocumentPath = "RMPO-2026-0022.png";
            await CreateRepository(ctx).UpdateAsync(update, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<RawMaterialPOHeader>().FirstAsync(x => x.Id == newId);
            saved.CropYear.Should().Be("2025-2026");
            saved.CreditDays.Should().Be(45);
            saved.DocumentPath.Should().Be("RMPO-2026-0022.png");
        }

        [Fact]
        public async Task ClearDocumentPathByFileNameAsync_Should_Null_Reference()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);
            var header = BuildHeader(ocrId, miscId, "RMPO-2026-0023");
            header.DocumentPath = "RMPO-2026-0023.png";
            var newId = await CreateRepository(ctx).CreateAsync(header, 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var cleared = await CreateRepository(ctx).ClearDocumentPathByFileNameAsync("RMPO-2026-0023.png", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            cleared.Should().BeTrue();
            var saved = await ctx.Set<RawMaterialPOHeader>().FirstAsync(x => x.Id == newId);
            saved.DocumentPath.Should().BeNull();
        }

        [Fact]
        public async Task ClearDocumentPathByFileNameAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepository(ctx).ClearDocumentPathByFileNameAsync("nope.png", CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(ocrId, miscId, "RMPO-2026-0003"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<RawMaterialPOHeader>().FirstAsync(x => x.Id == newId);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(ocrId, miscId, "RMPO-2026-0004"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var update = BuildHeader(ocrId, miscId, "RMPO-2026-0004");
            update.Id = newId;
            update.RawMaterialPODetails = new List<RawMaterialPODetail>
            {
                new() { ItemId = 13, HsnId = 1, Quantity = 200m, Rate = 68500m, ItemValue = 13_700_000m, TotalGST = 0m, NetValue = 13_700_000m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            await CreateRepository(ctx).UpdateAsync(update, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<RawMaterialPOHeader>().Include(h => h.RawMaterialPODetails).FirstAsync(x => x.Id == newId);
            saved.RawMaterialPODetails.Should().HaveCount(1);
            saved.RawMaterialPODetails!.First().Quantity.Should().Be(200m);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, miscId) = await SeedPrerequisitesAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader(ocrId, miscId, "RMPO-2026-0005"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(newId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Set<RawMaterialPOHeader>().IgnoreQueryFilters().FirstAsync(x => x.Id == newId);
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
