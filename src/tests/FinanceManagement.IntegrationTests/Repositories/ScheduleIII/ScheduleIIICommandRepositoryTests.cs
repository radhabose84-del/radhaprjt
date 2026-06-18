using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.ScheduleIII;
using FinanceManagement.IntegrationTests.Common;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.ScheduleIII
{
    [Collection("DatabaseCollection")]
    public sealed class ScheduleIIICommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ScheduleIIICommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static ScheduleIIICommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        // ---- seeding helpers -------------------------------------------------

        private static async Task<Dictionary<string, int>> SeedMiscAsync(ApplicationDbContext ctx)
        {
            var groups = new (string Type, (string Code, string Desc)[] Values)[]
            {
                ("S3_STMT_TYPE",    new[] { ("BS", "Balance Sheet"), ("PL", "Statement of P&L") }),
                ("S3_NATURE",       new[] { ("ASSET", "Asset"), ("INCOME", "Income"), ("EXPENSE", "Expense") }),
                ("S3_STATUS",       new[] { ("DRAFT", "Draft"), ("LOCKED", "Locked") }),
                ("S3_OPERATOR",     new[] { ("PLUS", "Plus"), ("MINUS", "Minus") }),
                ("S3_OPERAND_TYPE", new[] { ("LINEITEM", "Line Item"), ("SUBTOTAL", "Sub Total") }),
            };

            var map = new Dictionary<string, int>();
            foreach (var g in groups)
            {
                var type = new MiscTypeMaster { MiscTypeCode = g.Type, Description = g.Type };
                ctx.MiscTypeMaster.Add(type);
                await ctx.SaveChangesAsync();

                foreach (var v in g.Values)
                {
                    var misc = new MiscMaster { MiscTypeId = type.Id, Code = v.Code, Description = v.Desc, SortOrder = 1 };
                    ctx.MiscMaster.Add(misc);
                    await ctx.SaveChangesAsync();
                    map[v.Code] = misc.Id;
                }
            }
            return map;
        }

        private static async Task<int> SeedSectionAsync(ApplicationDbContext ctx, int stmtTypeId, int natureId, string name = "Income")
        {
            var sec = new ScheduleIIISection { SectionName = name, StatementTypeId = stmtTypeId, NatureId = natureId };
            ctx.ScheduleIIISection.Add(sec);
            await ctx.SaveChangesAsync();
            return sec.Id;
        }

        private static async Task<int> SeedLineAsync(ApplicationDbContext ctx, int sectionId, string code, string name)
        {
            var line = new ScheduleIIISectionItem { SectionId = sectionId, LineCode = code, LineName = name };
            ctx.ScheduleIIISectionItem.Add(line);
            await ctx.SaveChangesAsync();
            return line.Id;
        }

        // A master row = (company/division header) + one included line.
        private static async Task<int> SeedMasterRowAsync(ApplicationDbContext ctx, int statusId, int sectionItemId, int displayOrder = 1, int companyId = 1, int divisionId = 7)
        {
            var s = new ScheduleIIIMaster
            { CompanyId = companyId, DivisionId = divisionId, StatusId = statusId, ScheduleIIISectionItemId = sectionItemId, DisplayOrder = displayOrder };
            ctx.ScheduleIIIMaster.Add(s);
            await ctx.SaveChangesAsync();
            return s.Id;
        }

        // ---- MASTER (merged: one row per line) -------------------------------

        [Fact]
        public async Task CreateMaster_Should_Persist_And_Default_To_Draft()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");

            var id = await CreateRepository(ctx).CreateMasterAsync(new ScheduleIIIMaster
            { CompanyId = 1, DivisionId = 7, ScheduleIIISectionItemId = lineId, DisplayOrder = 1 });

            id.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ScheduleIIIMaster.FirstAsync(x => x.Id == id);
            saved.ScheduleIIISectionItemId.Should().Be(lineId);
            saved.StatusId.Should().Be(misc["DRAFT"], "a new master always starts as DRAFT");
        }

        [Fact]
        public async Task LockStructure_Should_Lock_All_Rows_Of_Company_Division()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var l1 = await SeedLineAsync(ctx, sectionId, "A", "Alpha");
            var l2 = await SeedLineAsync(ctx, sectionId, "B", "Beta");
            var m1 = await SeedMasterRowAsync(ctx, misc["DRAFT"], l1, 1);
            await SeedMasterRowAsync(ctx, misc["DRAFT"], l2, 2);
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).LockStructureAsync(m1)).Should().BeTrue();
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIIMaster.CountAsync(x => x.CompanyId == 1 && x.DivisionId == 7 && x.StatusId == misc["LOCKED"]))
                .Should().Be(2);
        }

        [Fact]
        public async Task SoftDeleteMaster_Should_Remove_One_Line()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            var m = await SeedMasterRowAsync(ctx, misc["DRAFT"], lineId, 1);
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).SoftDeleteMasterAsync(m, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIMaster.IgnoreQueryFilters().FirstAsync(x => x.Id == m)).IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task ReorderMaster_Should_Swap_DisplayOrder()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var l1 = await SeedLineAsync(ctx, sectionId, "A", "Alpha");
            var l2 = await SeedLineAsync(ctx, sectionId, "B", "Beta");
            var m1 = await SeedMasterRowAsync(ctx, misc["DRAFT"], l1, 1);
            var m2 = await SeedMasterRowAsync(ctx, misc["DRAFT"], l2, 2);
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).ReorderMasterAsync(m2, 1, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIIMaster.FirstAsync(x => x.Id == m2)).DisplayOrder.Should().Be(1);
            (await ctx.ScheduleIIIMaster.FirstAsync(x => x.Id == m1)).DisplayOrder.Should().Be(2);
        }

        // ---- SECTION / LINE (global) -----------------------------------------

        [Fact]
        public async Task CreateLineItem_Should_Persist()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);

            var newId = await CreateRepository(ctx).CreateLineItemAsync(new ScheduleIIISectionItem
            { SectionId = sectionId, LineCode = "INV", LineName = "Inventories", NoteReference = "Note 8" });

            newId.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIISectionItem.FirstAsync(x => x.Id == newId)).LineCode.Should().Be("INV");
        }

        // ---- SUB-TOTAL -------------------------------------------------------

        [Fact]
        public async Task CreateSubTotal_Should_Persist_Operands_And_Build_Expression()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, sectionId, "REV", "Revenue");
            var cogs = await SeedLineAsync(ctx, sectionId, "COGS", "Cost of Goods Sold");
            ctx.ChangeTracker.Clear();

            var subTotal = new ScheduleIIISubTotal { FormulaName = "Gross Profit", FormulaExpression = string.Empty, DisplayOrder = 1 };
            var formulas = new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"],  DisplayOrder = 1 },
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = cogs,    OperatorId = misc["MINUS"], DisplayOrder = 2 },
            };

            var newId = await CreateRepository(ctx).CreateSubTotalAsync(subTotal, formulas);

            newId.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == newId)).FormulaExpression.Should().Be("Revenue - Cost of Goods Sold");
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == newId && f.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
        }

        [Fact]
        public async Task UpdateSubTotal_Should_Replace_Operands_And_Rebuild_Expression()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, sectionId, "REV", "Revenue");
            var other = await SeedLineAsync(ctx, sectionId, "OI", "Other Income");

            var seedId = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "Gross Profit", FormulaExpression = string.Empty, DisplayOrder = 1 },
                new List<ScheduleIIISubTotalFormula>
                {
                    new() { OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 }
                });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateSubTotalAsync(seedId, "EBITDA", true, new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 },
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = other,   OperatorId = misc["PLUS"], DisplayOrder = 2 },
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == seedId);
            updated.FormulaName.Should().Be("EBITDA");
            updated.IncludeOtherIncome.Should().BeTrue();
            updated.FormulaExpression.Should().Be("Revenue + Other Income");
            // Old operands were physically deleted, new ones inserted.
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == seedId)).Should().Be(2);
        }
    }
}
