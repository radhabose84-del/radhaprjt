using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.ScheduleIII;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.ScheduleIII
{
    [Collection("DatabaseCollection")]
    public sealed class ScheduleIIIImportRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ScheduleIIIImportRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static async Task<(int StmtId, int NatureId)> SeedMiscAsync(ApplicationDbContext ctx)
        {
            var stmtType = new MiscTypeMaster { MiscTypeCode = "S3_STMT_TYPE", Description = "Statement type" };
            var natureType = new MiscTypeMaster { MiscTypeCode = "S3_NATURE", Description = "Nature" };
            ctx.MiscTypeMaster.AddRange(stmtType, natureType);
            await ctx.SaveChangesAsync();

            var pl = new MiscMaster { MiscTypeId = stmtType.Id, Code = "PL", Description = "Statement of P&L", SortOrder = 1 };
            var income = new MiscMaster { MiscTypeId = natureType.Id, Code = "Income", Description = "Income", SortOrder = 1 };
            ctx.MiscMaster.AddRange(pl, income);
            await ctx.SaveChangesAsync();
            return (pl.Id, income.Id);
        }

        [Fact]
        public async Task CommitAsync_Creates_Sections_With_Line_Items_Atomically()
        {
            await _fixture.ClearAllTablesAsync();
            int stmtId, natureId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                (stmtId, natureId) = await SeedMiscAsync(ctx);

            var section = new ScheduleIIISection { SectionName = "Revenue", StatementTypeId = stmtId, NatureId = natureId };
            var items = new List<ScheduleIIISectionItem>
            {
                new() { LineCode = "REV01", LineName = "Sale of products" },
                new() { LineCode = "REV02", LineName = "Other income" }
            };

            (int sectionsCreated, int itemsCreated, List<int> ids) result;
            await using (var ctx = _fixture.CreateFreshDbContext())
                result = await new ScheduleIIIImportCommandRepository(ctx)
                    .CommitAsync(new[] { (section, items) }, CancellationToken.None);

            result.sectionsCreated.Should().Be(1);
            result.itemsCreated.Should().Be(2);
            result.ids.Should().HaveCount(1);

            await using var verify = _fixture.CreateFreshDbContext();
            var savedSectionId = result.ids[0];
            var savedItems = await verify.ScheduleIIISectionItem.Where(i => i.SectionId == savedSectionId).ToListAsync();
            savedItems.Should().HaveCount(2);
            savedItems.Select(i => i.LineCode).Should().BeEquivalentTo(new[] { "REV01", "REV02" });
        }

        [Fact]
        public async Task QueryRepo_Resolves_Options_And_Existing_Section_Names()
        {
            await _fixture.ClearAllTablesAsync();
            int stmtId, natureId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                (stmtId, natureId) = await SeedMiscAsync(ctx);
                ctx.ScheduleIIISection.Add(new ScheduleIIISection { SectionName = "Existing Sec", StatementTypeId = stmtId, NatureId = natureId });
                await ctx.SaveChangesAsync();
            }

            var repo = new ScheduleIIIImportQueryRepository(new SqlConnection(_fixture.ConnectionString));

            (await repo.GetStatementTypeOptionsAsync()).Should().Contain(o => o.Code == "PL");
            (await repo.GetNatureOptionsAsync()).Should().Contain(o => o.Code == "Income");

            var existing = await repo.GetExistingSectionNamesAsync(new[] { "Existing Sec", "New Sec" });
            existing.Should().ContainSingle().Which.Should().Be("Existing Sec");
        }
    }
}
