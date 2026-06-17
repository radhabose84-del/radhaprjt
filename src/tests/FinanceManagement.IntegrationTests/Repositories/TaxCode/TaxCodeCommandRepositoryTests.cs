using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.TaxCode;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.TaxCode
{
    [Collection("DatabaseCollection")]
    public sealed class TaxCodeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TaxCodeCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static TaxCodeCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        // Seeds a MiscMaster value under the TAX_TYPE misc-type and returns its id (FK target).
        private async Task<int> SeedTaxTypeAsync(string code = "GST_OUT")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "TAX_TYPE");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster { MiscTypeCode = "TAX_TYPE", Description = "Tax Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var val = new Domain.Entities.MiscMaster { MiscTypeId = type.Id, Code = code, Description = code, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            await ctx.MiscMaster.AddAsync(val);
            await ctx.SaveChangesAsync();
            return val.Id;
        }

        private static Domain.Entities.TaxCodeMaster BuildEntity(
            int taxTypeId,
            string code = "GST-OUT-5",
            string name = "GST Output 5%") =>
            new()
            {
                CompanyId = 1,
                TaxCode = code,
                TaxName = name,
                TaxTypeId = taxTypeId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateTaxCodeAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();
            var ttId = await SeedTaxTypeAsync();

            var newId = await CreateRepository(ctx).CreateTaxCodeAsync(BuildEntity(ttId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateTaxCodeAsync_Should_Persist_Fields_And_Audit()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();
            var ttId = await SeedTaxTypeAsync();

            var newId = await CreateRepository(ctx).CreateTaxCodeAsync(BuildEntity(ttId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TaxCodeMaster.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.TaxCode.Should().Be("GST-OUT-5");
            saved.TaxTypeId.Should().Be(ttId);
            saved.IsActive.Should().Be(Status.Active);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE (code immutable) ---

        [Fact]
        public async Task UpdateTaxCodeAsync_Should_Persist_MutableFields_And_Keep_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();
            var ttId = await SeedTaxTypeAsync();
            var id = await CreateRepository(ctx).CreateTaxCodeAsync(BuildEntity(ttId, name: "Original"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.TaxCodeMaster.FirstAsync(x => x.Id == id);
            entity.TaxName = "Updated Name";
            entity.TaxCode = "TAMPERED";   // must be ignored by the repo (immutable)
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateTaxCodeAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.TaxCodeMaster.FirstAsync(x => x.Id == id);
            updated.TaxName.Should().Be("Updated Name");
            updated.TaxCode.Should().Be("GST-OUT-5");
        }

        // --- DEACTIVATE (no delete; "remove" = Update IsActive = Inactive) ---

        [Fact]
        public async Task UpdateTaxCodeAsync_Should_Deactivate_Via_IsActive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();
            var ttId = await SeedTaxTypeAsync();
            var id = await CreateRepository(ctx).CreateTaxCodeAsync(BuildEntity(ttId));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.TaxCodeMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await CreateRepository(ctx).UpdateTaxCodeAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.TaxCodeMaster.FirstAsync(x => x.Id == id);
            updated.IsActive.Should().Be(Status.Inactive);
        }

        // --- RATE VERSIONS (AC3-A) ---

        [Fact]
        public async Task CreateRateVersionAsync_FirstVersion_Should_Be_VersionOne_AndOpen()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();
            var ttId = await SeedTaxTypeAsync();
            var taxCodeId = await CreateRepository(ctx).CreateTaxCodeAsync(BuildEntity(ttId));
            ctx.ChangeTracker.Clear();

            var verId = await CreateRepository(ctx).CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
            {
                TaxCodeId = taxCodeId,
                RatePercent = 5.0m,
                EffectiveFrom = new DateOnly(2017, 7, 1)
            });
            ctx.ChangeTracker.Clear();

            var version = await ctx.TaxCodeRateVersion.FirstAsync(x => x.Id == verId);
            version.VersionNo.Should().Be(1);
            version.EffectiveTo.Should().BeNull();
        }

        [Fact]
        public async Task CreateRateVersionAsync_SecondVersion_Should_Close_PriorOpenVersion()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();
            var ttId = await SeedTaxTypeAsync();
            var repo = CreateRepository(ctx);
            var taxCodeId = await repo.CreateTaxCodeAsync(BuildEntity(ttId));
            ctx.ChangeTracker.Clear();

            await repo.CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
            {
                TaxCodeId = taxCodeId,
                RatePercent = 5.0m,
                EffectiveFrom = new DateOnly(2017, 7, 1)
            });
            ctx.ChangeTracker.Clear();

            var newFrom = new DateOnly(2026, 7, 1);
            await CreateRepository(ctx).CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
            {
                TaxCodeId = taxCodeId,
                RatePercent = 12.0m,
                EffectiveFrom = newFrom
            });
            ctx.ChangeTracker.Clear();

            var versions = await ctx.TaxCodeRateVersion
                .Where(v => v.TaxCodeId == taxCodeId)
                .OrderBy(v => v.VersionNo)
                .ToListAsync();

            versions.Should().HaveCount(2);
            versions[0].VersionNo.Should().Be(1);
            versions[0].EffectiveTo.Should().Be(newFrom.AddDays(-1));   // prior version closed
            versions[1].VersionNo.Should().Be(2);
            versions[1].RatePercent.Should().Be(12.0m);
            versions[1].EffectiveTo.Should().BeNull();                  // new version open
        }
    }
}
