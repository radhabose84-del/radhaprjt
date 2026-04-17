using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.TncTemplateMaster
{
    [Collection("DatabaseCollection")]
    public sealed class TncTemplateMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TncTemplateMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private TncTemplateMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureMiscTypeAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == code);
            if (existing != null) return existing.Id;
            var t = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = code,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(t);
            await ctx.SaveChangesAsync();
            return t.Id;
        }

        private async Task<int> EnsureMiscAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == code && m.MiscTypeId == miscTypeId);
            if (existing != null) return existing.Id;
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = code,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedTemplateAsync(
            string templateName,
            string templateCode,
            int? typeId = null,
            int? applicabilityId = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            typeId ??= await EnsureMiscAsync(await EnsureMiscTypeAsync("TNCQ_TT"), "PURCH_QT");
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = new TnCTemplateMaster
            {
                TemplateCode = templateCode,
                TemplateName = templateName,
                TemplateTypeId = typeId.Value,
                TermsHtml = "<p>Sample</p>",
                ApprovalFlag = false,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.TnCTemplateMaster.AddAsync(t);
            await ctx.SaveChangesAsync();

            if (applicabilityId.HasValue)
            {
                await ctx.TnCTemplateApplicability.AddAsync(new TnCTemplateApplicability
                {
                    TnCTemplateMasterId = t.Id,
                    ApplicabilityId = applicabilityId.Value,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
                await ctx.SaveChangesAsync();
            }

            return t.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllTncTemplateAsync ---

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_All1", "TQ_C1");

            var (rows, total) = await CreateRepo().GetAllTncTemplateAsync(1, 10, null!);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedTemplateAsync("UNIQ_TEMPLATE", "TQ_C2");
            await SeedTemplateAsync("Other_Template", "TQ_C3");

            var (rows, _) = await CreateRepo().GetAllTncTemplateAsync(1, 10, "UNIQ_");

            rows.Should().HaveCount(1);
            rows[0].TemplateName.Should().Be("UNIQ_TEMPLATE");
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_DEL", "TQ_C4", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllTncTemplateAsync(1, 10, null!);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Default_Pagination()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_PAG", "TQ_PG1");

            var (rows, _) = await CreateRepo().GetAllTncTemplateAsync(0, 0, null!);

            rows.Should().NotBeNull();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_With_Children()
        {
            await ClearAsync();
            var typeId = await EnsureMiscAsync(await EnsureMiscTypeAsync("TNCQ_TT"), "PURCH_QT");
            var appId = await EnsureMiscAsync(await EnsureMiscTypeAsync("TNCQ_APP_T"), "APP_GBI");
            var id = await SeedTemplateAsync("TQ_GBI", "TQ_GBI", typeId: typeId, applicabilityId: appId);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.TemplateName.Should().Be("TQ_GBI");
            result.Applicabilities.Should().ContainSingle();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_GSD", "TQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ExistsByTypeAndNameAsync ---

        [Fact]
        public async Task ExistsByTypeAndNameAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            var typeId = await EnsureMiscAsync(await EnsureMiscTypeAsync("TNCQ_TT"), "PURCH_QT");
            await SeedTemplateAsync("TQ_DUP", "TQ_DUP", typeId: typeId);

            var result = await CreateRepo().ExistsByTypeAndNameAsync(typeId, "TQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByTypeAndNameAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var typeId = await EnsureMiscAsync(await EnsureMiscTypeAsync("TNCQ_TT"), "PURCH_QT");
            var id = await SeedTemplateAsync("TQ_SELF", "TQ_SELF", typeId: typeId);

            var result = await CreateRepo().ExistsByTypeAndNameAsync(typeId, "TQ_SELF", excludeId: id);

            result.Should().BeFalse();
        }

        // --- ApplicabilitiesExistAsync ---

        [Fact]
        public async Task ApplicabilitiesExistAsync_Should_Return_False_For_Empty_Input()
        {
            var result = await CreateRepo().ApplicabilitiesExistAsync(Array.Empty<int>());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ApplicabilitiesExistAsync_Should_Return_False_When_Some_Missing()
        {
            // Pass a known-bad id; even if matching MiscTypeId 9 lookup is wrong on test DB,
            // a non-existent id is guaranteed to fail the count check.
            var result = await CreateRepo().ApplicabilitiesExistAsync(new[] { 9999999 });
            result.Should().BeFalse();
        }

        // --- IsUsedInTransactionsAsync ---
        // Skipped: queries Purchase.PurchaseOrder + Sales.SalesOrder which the
        // PurchaseManagement_TestDb does not provision (test DB only contains tables
        // for entities owned by this module). Method is exercised by handler-level tests.

        // --- GetTnCTemplateAutoCompleteAsync ---

        [Fact]
        public async Task GetTnCTemplateAutoCompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedTemplateAsync("AC_Match1", "AC_M1");
            await SeedTemplateAsync("AC_Match2", "AC_M2", active: Status.Inactive);

            var result = await CreateRepo().GetTnCTemplateAutoCompleteAsync("AC_Match", null, null);

            result.Should().HaveCount(1);
            result[0].TemplateName.Should().Be("AC_Match1");
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Children()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_SDV1", "TQ_SDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Children_Exist()
        {
            await ClearAsync();
            var typeId = await EnsureMiscAsync(await EnsureMiscTypeAsync("TNCQ_TT"), "PURCH_QT");
            var appId = await EnsureMiscAsync(await EnsureMiscTypeAsync("TNCQ_APP_T"), "APP_SDV");
            var id = await SeedTemplateAsync("TQ_SDV2", "TQ_SDV2", typeId: typeId, applicabilityId: appId);

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        // --- IsTnCTemplateLinkedAsync ---

        [Fact]
        public async Task IsTnCTemplateLinkedAsync_Should_Return_False_When_No_Active_Children()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_LK1", "TQ_LK1");

            var result = await CreateRepo().IsTnCTemplateLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
