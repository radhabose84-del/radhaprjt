using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.GlAccountMaster;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Entities = FinanceManagement.Domain.Entities;

namespace FinanceManagement.IntegrationTests.Repositories.GlAccountMaster
{
    // US-GL02-10 Multi-Company COA — propagation/inheritance + the new query/posting helpers, against
    // the real SQL test DB. Entity group = companies 100 (template), 200, 300 (EntityId 1); 999 is a
    // different entity. TemplateCompanyId = 100.
    [Collection("DatabaseCollection")]
    public sealed class GlAccountMasterMultiCompanyTests
    {
        private const int Template = 100;
        private const int Sub1 = 200;
        private const int Sub2 = 300;
        private const int OtherEntity = 999;

        private readonly DbFixture _fixture;
        public GlAccountMasterMultiCompanyTests(DbFixture fixture) => _fixture = fixture;

        // ── helpers ────────────────────────────────────────────────────────────
        private sealed class Fk
        {
            public int AccountTypeId;
            public int AccountGroupId;
            public int NormalBalanceId;
            public int CurrencyId;
            public int SubLedgerId;
        }

        private static Mock<ICompanyLookup> CompanyLookup()
        {
            var mock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto>
            {
                new() { CompanyId = Template, CompanyName = "Group", EntityId = 1 },
                new() { CompanyId = Sub1, CompanyName = "Spinning", EntityId = 1 },
                new() { CompanyId = Sub2, CompanyName = "Processing", EntityId = 1 },
                new() { CompanyId = OtherEntity, CompanyName = "OtherEntity", EntityId = 2 },
            });
            return mock;
        }

        private GlobalCoaPropagationService Propagation(ApplicationDbContext ctx) =>
            new(ctx, CompanyLookup().Object, Options.Create(new MultiCompanyCoaOptions { TemplateCompanyId = Template }));

        private async Task<Fk> SeedFkGraphAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = new Entities.MiscTypeMaster
            { MiscTypeCode = "MC_MULTICO", Description = "MC", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var nb = new Entities.MiscMaster { MiscTypeId = miscType.Id, Code = "NB", Description = "Normal", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var slt = new Entities.MiscMaster { MiscTypeId = miscType.Id, Code = "SLT", Description = "SubLedger", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.AddRange(nb, slt);

            var accountType = new Entities.AccountTypeMaster
            { CompanyId = Template, AccountTypeName = "Asset", StartCode = "1", AccountCodeLength = 4, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var group = new Entities.AccountGroup
            { CompanyId = Template, GroupCode = "MCGRP", GroupName = "MC Group", Level = 1, IsLeaf = true, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var currency = new Entities.CurrencyForexConfig
            { CompanyId = Template, CurrencyTypeCode = "INR", CurrencyTypeName = "Rupee", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.AccountTypeMaster.Add(accountType);
            ctx.AccountGroup.Add(group);
            ctx.CurrencyForexConfig.Add(currency);
            await ctx.SaveChangesAsync();

            return new Fk
            {
                AccountTypeId = accountType.Id,
                AccountGroupId = group.Id,
                NormalBalanceId = nb.Id,
                CurrencyId = currency.Id,
                SubLedgerId = slt.Id
            };
        }

        private static Entities.GlAccountMaster Account(Fk fk, int companyId, string code, bool isGlobal = false,
            bool restricted = false, int? globalAccountId = null, bool localOverride = false, string name = "Acct") => new()
        {
            CompanyId = companyId,
            AccountTypeId = fk.AccountTypeId,
            AccountGroupId = fk.AccountGroupId,
            AccountCode = code,
            AccountName = name,
            NormalBalanceId = fk.NormalBalanceId,
            CurrencyTypeId = fk.CurrencyId,
            SubLedgerTypeId = fk.SubLedgerId,
            IsGlobal = isGlobal,
            IsCompanyRestricted = restricted,
            GlobalAccountId = globalAccountId,
            IsLocalOverride = localOverride,
            IsActive = Status.Active,
            IsDeleted = IsDelete.NotDeleted
        };

        private async Task<int> AddAsync(Entities.GlAccountMaster a)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new GlAccountMasterCommandRepository(ctx).CreateAsync(a);
        }

        // ── AC1 inheritance ──────────────────────────────────────────────────────
        [Fact]
        public async Task InheritForCompany_CopiesOnlyNonRestrictedGlobals()
        {
            await _fixture.ClearAllTablesAsync();
            var fk = await SeedFkGraphAsync();
            var globalA = await AddAsync(Account(fk, Template, "1001", isGlobal: true, name: "Cash"));
            await AddAsync(Account(fk, Template, "1002", isGlobal: true, restricted: true, name: "Secret"));
            await AddAsync(Account(fk, Template, "1003", isGlobal: false, name: "Local"));

            int created;
            await using (var ctx = _fixture.CreateFreshDbContext())
                created = await Propagation(ctx).InheritForCompanyAsync(Sub1, CancellationToken.None);

            created.Should().Be(1);
            await using var verify = _fixture.CreateFreshDbContext();
            var copies = await verify.GlAccountMaster.Where(a => a.CompanyId == Sub1).ToListAsync();
            copies.Should().HaveCount(1);
            copies[0].AccountCode.Should().Be("1001");
            copies[0].GlobalAccountId.Should().Be(globalA);
            copies[0].IsGlobal.Should().BeFalse();
        }

        // ── AC3 propagation skips overrides ───────────────────────────────────────
        [Fact]
        public async Task PropagateUpdate_UpdatesTrackingCopies_SkipsLocalOverride()
        {
            await _fixture.ClearAllTablesAsync();
            var fk = await SeedFkGraphAsync();
            var globalA = await AddAsync(Account(fk, Template, "1001", isGlobal: true, name: "Cash"));
            await AddAsync(Account(fk, Sub1, "1001", globalAccountId: globalA, name: "Cash"));
            await AddAsync(Account(fk, Sub2, "1001", globalAccountId: globalA, localOverride: true, name: "Renamed Locally"));

            // edit the template
            await using (var edit = _fixture.CreateFreshDbContext())
            {
                var t = await edit.GlAccountMaster.SingleAsync(a => a.Id == globalA);
                t.AccountName = "Cash & Bank";
                await edit.SaveChangesAsync();
            }

            int updated;
            await using (var ctx = _fixture.CreateFreshDbContext())
                updated = await Propagation(ctx).PropagateUpdateAsync(globalA, CancellationToken.None);

            updated.Should().Be(1);
            await using var verify = _fixture.CreateFreshDbContext();
            (await verify.GlAccountMaster.SingleAsync(a => a.CompanyId == Sub1)).AccountName.Should().Be("Cash & Bank");
            (await verify.GlAccountMaster.SingleAsync(a => a.CompanyId == Sub2)).AccountName.Should().Be("Renamed Locally");
        }

        // ── AC3 command-repo: editing a copy marks it an override + persists restriction ──
        [Fact]
        public async Task UpdateCopy_SetsLocalOverride_AndPersistsRestricted()
        {
            await _fixture.ClearAllTablesAsync();
            var fk = await SeedFkGraphAsync();
            var globalA = await AddAsync(Account(fk, Template, "1001", isGlobal: true, name: "Cash"));
            var copyId = await AddAsync(Account(fk, Sub1, "1001", globalAccountId: globalA, name: "Cash"));

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = Account(fk, Sub1, "1001", globalAccountId: globalA, name: "Cash (local)");
                entity.Id = copyId;
                entity.IsCompanyRestricted = true;
                await new GlAccountMasterCommandRepository(ctx).UpdateAsync(entity);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var saved = await verify.GlAccountMaster.SingleAsync(a => a.Id == copyId);
            saved.IsLocalOverride.Should().BeTrue("editing an inherited copy creates an override");
            saved.IsCompanyRestricted.Should().BeTrue();
            saved.AccountName.Should().Be("Cash (local)");
        }

        // ── AC4 consistency report ────────────────────────────────────────────────
        [Fact]
        public async Task GetSingleEntityAccounts_ReturnsCodesPresentInOneCompany()
        {
            await _fixture.ClearAllTablesAsync();
            var fk = await SeedFkGraphAsync();
            // "1001" in both Sub1 and Sub2 → shared (excluded). "1009" only in Sub2 → flagged.
            await AddAsync(Account(fk, Sub1, "1001", name: "Cash"));
            await AddAsync(Account(fk, Sub2, "1001", name: "Cash"));
            await AddAsync(Account(fk, Sub2, "1009", name: "Dye Cost"));

            var repo = new GlAccountMasterQueryRepository(new SqlConnection(_fixture.ConnectionString), CompanyLookup().Object);
            var rows = await repo.GetSingleEntityAccountsAsync(new[] { Sub1, Sub2 });

            rows.Should().ContainSingle(r => r.AccountCode == "1009").Which.CompanyId.Should().Be(Sub2);
            rows.Should().NotContain(r => r.AccountCode == "1001");
        }

        // ── AC2 posting guard ─────────────────────────────────────────────────────
        [Fact]
        public async Task GetForeignRestrictedAccountIds_ReturnsRestrictedAccountsOfOtherCompanies()
        {
            await _fixture.ClearAllTablesAsync();
            var fk = await SeedFkGraphAsync();
            var restrictedInTemplate = await AddAsync(Account(fk, Template, "1001", restricted: true, name: "Restricted"));
            var ownAccount = await AddAsync(Account(fk, Sub1, "2001", name: "Own"));

            var journalRepo = new JournalQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                CompanyLookup().Object,
                new Mock<IFinancialYearLookup>(MockBehavior.Loose).Object);

            // Posting from Sub1: the template's restricted account is foreign-restricted; Sub1's own is fine.
            var foreign = await journalRepo.GetForeignRestrictedAccountIdsAsync(
                new[] { restrictedInTemplate, ownAccount }, Sub1);

            foreign.Should().ContainSingle().Which.Should().Be(restrictedInTemplate);
        }
    }
}
