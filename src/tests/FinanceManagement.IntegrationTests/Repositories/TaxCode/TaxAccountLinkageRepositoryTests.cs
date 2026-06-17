using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.TaxCode;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.TaxCode
{
    [Collection("DatabaseCollection")]
    public sealed class TaxAccountLinkageRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TaxAccountLinkageRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static TaxCodeCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);
        private TaxCodeQueryRepository CreateQueryRepo() => new(new SqlConnection(_fixture.ConnectionString));

        // Seeds a GL account directly (its own FK chain — AccountType/AccountGroup/MiscMaster —
        // is bypassed with NOCHECK so the linkage FK target exists without the full hierarchy).
        private async Task<int> SeedGlAccountAsync(string code = "42001001")
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            const string sql = @"
                ALTER TABLE Finance.GlAccountMaster NOCHECK CONSTRAINT ALL;
                INSERT INTO Finance.GlAccountMaster
                    (CompanyId, AccountTypeId, AccountGroupId, AccountCode, AccountName,
                     NormalBalanceId, CurrencyTypeId, SubLedgerTypeId,
                     IsCostCentreMandatory, IsTaxRelevant, IsInterCompany, IsReconciliationRequired,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (1, 1, 1, @Code, 'Test GL Account',
                     1, 1, 1, 0, 0, 0, 0, 1, 0, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, new { Code = code });
        }

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

        private async Task<int> SeedTaxCodeAsync(string code = "GST-OUT-5")
        {
            var taxTypeId = await SeedTaxTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateTaxCodeAsync(new Domain.Entities.TaxCodeMaster
            {
                CompanyId = 1,
                TaxCode = code,
                TaxName = "GST Output 5%",
                TaxTypeId = taxTypeId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // Seeds the APPROVAL_STATUS misc-type + value, returns the MiscMaster id for the given code.
        private async Task<int> SeedApprovalStatusAsync(string code = "PENDING")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "APPROVAL_STATUS");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster { MiscTypeCode = "APPROVAL_STATUS", Description = "Approval Status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.MiscTypeId == type.Id && m.Code == code);
            if (existing != null)
                return existing.Id;

            var val = new Domain.Entities.MiscMaster { MiscTypeId = type.Id, Code = code, Description = code, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            await ctx.MiscMaster.AddAsync(val);
            await ctx.SaveChangesAsync();
            return val.Id;
        }

        private async Task<int> SeedLinkageAsync(int taxCodeId, int glAccountId, string status = "PENDING", DateOnly? from = null)
        {
            var statusId = await SeedApprovalStatusAsync(status);
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateLinkageAsync(new Domain.Entities.TaxAccountLinkage
            {
                CompanyId = 1,
                TaxCodeId = taxCodeId,
                GlAccountId = glAccountId,
                StatusId = statusId,
                EffectiveFrom = from ?? new DateOnly(2026, 5, 25),
                // PENDING change requests are inactive until /activate; APPROVED rows are active.
                IsActive = status == "APPROVED" ? Status.Active : Status.Inactive
            });
        }

        [Fact]
        public async Task CreateLinkageAsync_Should_Persist_Pending()
        {
            await _fixture.ClearAllTablesAsync();
            var taxCodeId = await SeedTaxCodeAsync();
            var glId = await SeedGlAccountAsync();

            var linkId = await SeedLinkageAsync(taxCodeId, glId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var saved = await ctx.TaxAccountLinkage.FirstOrDefaultAsync(x => x.Id == linkId);
            saved.Should().NotBeNull();
            saved!.TaxCodeId.Should().Be(taxCodeId);
            saved.GlAccountId.Should().Be(glId);
            var pendingId = await SeedApprovalStatusAsync("PENDING");
            saved.StatusId.Should().Be(pendingId);
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task ActivateLinkageAsync_Should_Set_Activated_And_Approved()
        {
            await _fixture.ClearAllTablesAsync();
            var taxCodeId = await SeedTaxCodeAsync();
            var glId = await SeedGlAccountAsync();
            var linkId = await SeedLinkageAsync(taxCodeId, glId);
            var approvedId = await SeedApprovalStatusAsync("APPROVED");

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await CreateCommandRepo(ctx).ActivateLinkageAsync(linkId, approvedId, CancellationToken.None);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var saved = await verify.TaxAccountLinkage.FirstAsync(x => x.Id == linkId);
            saved.IsActive.Should().Be(Status.Active);
            saved.StatusId.Should().Be(approvedId);
        }

        [Fact]
        public async Task GetLinkageByAccountAsync_Should_Return_Only_Active_Approved()
        {
            await _fixture.ClearAllTablesAsync();
            var taxCodeId = await SeedTaxCodeAsync();
            var glId = await SeedGlAccountAsync();
            var linkId = await SeedLinkageAsync(taxCodeId, glId);

            // Pending linkage is not the "active" one.
            var pending = await CreateQueryRepo().GetLinkageByAccountAsync(glId);
            pending.Should().BeNull();

            var approvedId = await SeedApprovalStatusAsync("APPROVED");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await CreateCommandRepo(ctx).ActivateLinkageAsync(linkId, approvedId, CancellationToken.None);
            }

            var active = await CreateQueryRepo().GetLinkageByAccountAsync(glId);
            active.Should().NotBeNull();
            active!.Id.Should().Be(linkId);
            active.AccountCode.Should().Be("42001001");
        }

        [Fact]
        public async Task RejectLinkageAsync_Should_Set_Rejected_And_StayInactive()
        {
            await _fixture.ClearAllTablesAsync();
            var taxCodeId = await SeedTaxCodeAsync();
            var glId = await SeedGlAccountAsync();
            var linkId = await SeedLinkageAsync(taxCodeId, glId, status: "PENDING");
            var rejectedId = await SeedApprovalStatusAsync("REJECTED");

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await CreateCommandRepo(ctx).RejectLinkageAsync(linkId, rejectedId, CancellationToken.None);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var saved = await verify.TaxAccountLinkage.FirstAsync(x => x.Id == linkId);
            saved.StatusId.Should().Be(rejectedId);
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task GetPendingLinkagesAsync_Should_Return_Only_Pending()
        {
            await _fixture.ClearAllTablesAsync();
            var taxCodeId = await SeedTaxCodeAsync();
            var glId = await SeedGlAccountAsync();

            await SeedLinkageAsync(taxCodeId, glId, status: "PENDING");
            await SeedLinkageAsync(taxCodeId, glId, status: "APPROVED");

            var (pending, total) = await CreateQueryRepo().GetPendingLinkagesAsync(1, 10, null, 1);

            total.Should().Be(1);
            pending.Should().HaveCount(1);
            pending[0].Status.Should().Be("PENDING");
        }

        [Fact]
        public async Task GlAccountExistsAsync_Should_Return_True_For_Seeded()
        {
            await _fixture.ClearAllTablesAsync();
            var glId = await SeedGlAccountAsync();

            var exists = await CreateQueryRepo().GlAccountExistsAsync(glId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ActivateLinkageAsync_Should_Close_Prior_By_EffectiveToOnly()
        {
            await _fixture.ClearAllTablesAsync();
            var taxCodeId = await SeedTaxCodeAsync();
            var glId = await SeedGlAccountAsync();
            var approvedId = await SeedApprovalStatusAsync("APPROVED");

            // First active linkage for the account (earlier effective date).
            var firstId = await SeedLinkageAsync(taxCodeId, glId, status: "APPROVED", from: new DateOnly(2026, 1, 1));
            // A pending change request (new inactive row) for the same account, effective 2026-07-01.
            var secondId = await SeedLinkageAsync(taxCodeId, glId, status: "PENDING", from: new DateOnly(2026, 7, 1));

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await CreateCommandRepo(ctx).ActivateLinkageAsync(secondId, approvedId, CancellationToken.None);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var prior = await verify.TaxAccountLinkage.FirstAsync(x => x.Id == firstId);
            var current = await verify.TaxAccountLinkage.FirstAsync(x => x.Id == secondId);

            // Prior row stays ACTIVE (valid until its EffectiveTo) — only the end-date is set.
            prior.IsActive.Should().Be(Status.Active);
            prior.EffectiveTo.Should().Be(new DateOnly(2026, 6, 30));   // newFrom - 1
            current.IsActive.Should().Be(Status.Active);                // new active linkage
            current.EffectiveTo.Should().BeNull();                      // open
        }
    }
}
