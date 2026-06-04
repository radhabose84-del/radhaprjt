using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.OCREntry;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.OCREntry
{
    [Collection("DatabaseCollection")]
    public sealed class OCREntryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IDocumentSequenceLookup> _docSeqMock = new(MockBehavior.Loose);

        public OCREntryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            // IncrementDocNoAsync runs inside the repo transaction — make it a no-op.
            _docSeqMock
                .Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                .Returns(Task.CompletedTask);
        }

        private OCREntryCommandRepository CreateRepository(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object, _docSeqMock.Object);

        private static PurchaseManagement.Domain.Entities.OCREntry BuildEntity(
            int miscId, int paymentTermId, string ocrNumber = "OCR-2026-0001") =>
            new()
            {
                OcrNumber = ocrNumber,
                OcrDate = DateTimeOffset.UtcNow,
                ProcurementSourceId = miscId,
                ProcurementTypeId = miscId,
                BrokerDirectId = miscId,
                GradeId = miscId,
                StatusId = miscId,
                PaymentTermId = paymentTermId,
                SupplierId = 10,
                LocationId = 11,
                StationId = 12,
                ItemId = 13,
                CountId = 14,
                Quantity = 100m,
                Rate = 75000m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<(int miscId, int paymentTermId)> SeedPrerequisitesAsync(
            PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "MT001",
                    Description = "Test Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });

            var misc = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Code = "MSC001",
                    Description = "Test Misc",
                    MiscTypeId = miscType.Id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });

            var paymentTerm = new PurchaseManagement.Domain.Entities.PaymentTermMaster
            {
                Code = "PT001",
                Description = "30 Days",
                BaselineTypeId = misc.Id,
                CreditDays = 30,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseManagement.Domain.Entities.PaymentTermMaster>().Add(paymentTerm);
            await ctx.SaveChangesAsync();

            return (misc.Id, paymentTerm.Id);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscId, paymentTermId) = await SeedPrerequisitesAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, paymentTermId), 0, CancellationToken.None);

            created.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscId, paymentTermId) = await SeedPrerequisitesAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(
                BuildEntity(miscId, paymentTermId, "OCR-2026-0002"), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            saved!.OcrNumber.Should().Be("OCR-2026-0002");
            saved.SupplierId.Should().Be(10);
            saved.ItemId.Should().Be(13);
            saved.Quantity.Should().Be(100m);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscId, paymentTermId) = await SeedPrerequisitesAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, paymentTermId), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(created.Id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ClearDocumentPathByFileNameAsync_Should_Return_True_And_Null_Column()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscId, paymentTermId) = await SeedPrerequisitesAsync(ctx);
            var entity = BuildEntity(miscId, paymentTermId, "OCR-2026-DOC1");
            entity.DocumentPath = "OCR-2026-DOC1.png"; // file name stored in DocumentPath
            var created = await CreateRepository(ctx).CreateAsync(entity, 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var cleared = await CreateRepository(ctx).ClearDocumentPathByFileNameAsync("OCR-2026-DOC1.png", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            cleared.Should().BeTrue();
            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>().FirstAsync(x => x.Id == created.Id);
            saved.DocumentPath.Should().BeNull();
        }

        [Fact]
        public async Task ClearDocumentPathByFileNameAsync_Should_Return_False_When_NoMatch()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscId, paymentTermId) = await SeedPrerequisitesAsync(ctx);
            // Saved with a different document name; the temp name matches nothing.
            var entity = BuildEntity(miscId, paymentTermId, "OCR-2026-DOC2");
            entity.DocumentPath = "OCR-2026-DOC2.png";
            await CreateRepository(ctx).CreateAsync(entity, 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var cleared = await CreateRepository(ctx).ClearDocumentPathByFileNameAsync(
                "TEMP_905dddb8-9bc5-4667-a2a4-ac9274946078.png", CancellationToken.None);

            cleared.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateOcrApproveAsync_Should_Update_StatusId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscId, paymentTermId) = await SeedPrerequisitesAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, paymentTermId), 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var updated = await CreateRepository(ctx).UpdateOcrApproveAsync(created.Id, miscId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            updated.Should().BeTrue();
            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>().FirstAsync(x => x.Id == created.Id);
            saved.StatusId.Should().Be(miscId);
        }
    }
}
