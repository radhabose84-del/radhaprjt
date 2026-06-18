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

        // ---- seeding helpers (global catalog + junction) ---------------------

        private static async Task<Dictionary<string, int>> SeedMiscAsync(ApplicationDbContext ctx)
        {
            var groups = new (string Type, (string Code, string Desc)[] Values)[]
            {
                ("S3_STMT_TYPE",    new[] { ("BS", "Balance Sheet"), ("PL", "Statement of P&L") }),
                ("S3_NATURE",       new[] { ("ASSET", "Asset"), ("INCOME", "Income"), ("EXPENSE", "Expense") }),
                ("S3_STATUS",       new[] { ("DRAFT", "Draft"), ("LOCKED", "Locked") }),
                ("S3_OPERATOR",     new[] { ("PLUS", "Plus"), ("MINUS", "Minus") }),
                ("S3_OPERAND_TYPE", new[] { ("LINEITEM", "Line Item"), ("SUBTOTAL", "Sub Total") }),
                ("S3_SUBTOTAL_TYPE",new[] { ("GROSSPROFIT", "Gross Profit"), ("EBITDA", "EBITDA") }),
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

        private static async Task<int> SeedMasterAsync(ApplicationDbContext ctx, int statusId, int companyId = 1, int divisionId = 7)
        {
            var s = new ScheduleIIIMaster { CompanyId = companyId, DivisionId = divisionId, StatusId = statusId, VersionNo = 1 };
            ctx.ScheduleIIIMaster.Add(s);
            await ctx.SaveChangesAsync();
            return s.Id;
        }

        private static async Task<int> SeedSectionAsync(ApplicationDbContext ctx, int stmtTypeId, int natureId, string name = "Current Assets")
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

        private static async Task<int> SeedMasterLineAsync(ApplicationDbContext ctx, int masterId, int lineId, int order)
        {
            var ml = new ScheduleIIIMasterLine { ScheduleIIIMasterId = masterId, ScheduleIIISectionItemId = lineId, DisplayOrder = order };
            ctx.ScheduleIIIMasterLine.Add(ml);
            await ctx.SaveChangesAsync();
            return ml.Id;
        }

        // ---- MASTER ----------------------------------------------------------

        [Fact]
        public async Task CreateMaster_Should_Persist()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);

            var id = await CreateRepository(ctx).CreateMasterAsync(new ScheduleIIIMaster
            { CompanyId = 1, DivisionId = 7, StatusId = misc["DRAFT"], VersionNo = 1 });

            id.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIMaster.FirstAsync(x => x.Id == id)).CompanyId.Should().Be(1);
        }

        [Fact]
        public async Task LockStructure_Should_Set_Locked_Status()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);

            (await CreateRepository(ctx).LockStructureAsync(masterId)).Should().BeTrue();
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIMaster.FirstAsync(x => x.Id == masterId)).StatusId.Should().Be(misc["LOCKED"]);
        }

        // ---- SECTION (global) ------------------------------------------------

        [Fact]
        public async Task CreateSection_Should_Persist()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);

            var id = await CreateRepository(ctx).CreateSectionAsync(new ScheduleIIISection
            { SectionName = "Current Assets", StatementTypeId = misc["BS"], NatureId = misc["ASSET"] });

            id.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIISection.FirstAsync(x => x.Id == id)).SectionName.Should().Be("Current Assets");
        }

        // ---- LINE ITEM (global) ----------------------------------------------

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
            var saved = await ctx.ScheduleIIISectionItem.FirstAsync(x => x.Id == newId);
            saved.LineCode.Should().Be("INV");
            saved.NoteReference.Should().Be("Note 8");
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task UpdateLineItem_Should_Change_Name_But_Not_Code()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var id = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateLineItemAsync(new ScheduleIIISectionItem
            { Id = id, SectionId = sectionId, LineName = "Inventories (revised)", IsActive = Status.Active });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ScheduleIIISectionItem.FirstAsync(x => x.Id == id);
            saved.LineName.Should().Be("Inventories (revised)");
            saved.LineCode.Should().Be("INV");
        }

        [Fact]
        public async Task SoftDeleteLineItem_Should_Set_Flag()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var id = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).SoftDeleteLineItemAsync(id, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIISectionItem.IgnoreQueryFilters().FirstAsync(x => x.Id == id)).IsDeleted.Should().Be(IsDelete.Deleted);
        }

        // ---- MASTER LINE (junction) ------------------------------------------

        [Fact]
        public async Task CreateMasterLine_Should_Link_Line_To_Master()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");

            var id = await CreateRepository(ctx).CreateMasterLineAsync(new ScheduleIIIMasterLine
            { ScheduleIIIMasterId = masterId, ScheduleIIISectionItemId = lineId, DisplayOrder = 1 });

            id.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ScheduleIIIMasterLine.FirstAsync(x => x.Id == id);
            saved.ScheduleIIIMasterId.Should().Be(masterId);
            saved.ScheduleIIISectionItemId.Should().Be(lineId);
        }

        [Fact]
        public async Task SoftDeleteMasterLine_Should_Detach()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            var mlId = await SeedMasterLineAsync(ctx, masterId, lineId, 1);
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).SoftDeleteMasterLineAsync(mlId, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIMasterLine.IgnoreQueryFilters().FirstAsync(x => x.Id == mlId)).IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task ReorderMasterLine_Should_Swap_DisplayOrder()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var l1 = await SeedLineAsync(ctx, sectionId, "A", "Alpha");
            var l2 = await SeedLineAsync(ctx, sectionId, "B", "Beta");
            var ml1 = await SeedMasterLineAsync(ctx, masterId, l1, 1);
            var ml2 = await SeedMasterLineAsync(ctx, masterId, l2, 2);
            ctx.ChangeTracker.Clear();

            // Move ml2 up (direction 1) — swaps with ml1.
            (await CreateRepository(ctx).ReorderMasterLineAsync(ml2, 1, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIIMasterLine.FirstAsync(x => x.Id == ml2)).DisplayOrder.Should().Be(1);
            (await ctx.ScheduleIIIMasterLine.FirstAsync(x => x.Id == ml1)).DisplayOrder.Should().Be(2);
        }

        // ---- SUB-TOTAL -------------------------------------------------------

        [Fact]
        public async Task CreateSubTotal_Should_Persist_Formulas_And_Build_Expression()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, sectionId, "REV", "Revenue");
            var cogs = await SeedLineAsync(ctx, sectionId, "COGS", "Cost of Goods Sold");
            ctx.ChangeTracker.Clear();

            var subTotal = new ScheduleIIISubTotal
            { ScheduleIIIMasterId = masterId, SubTotalTypeId = misc["GROSSPROFIT"], FormulaExpression = string.Empty, IsSystemDefined = false, DisplayOrder = 1 };
            var formulas = new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["LINEITEM"], OperandRefId = revenue, OperatorId = misc["PLUS"],  DisplayOrder = 1 },
                new() { OperandTypeId = misc["LINEITEM"], OperandRefId = cogs,    OperatorId = misc["MINUS"], DisplayOrder = 2 },
            };

            var newId = await CreateRepository(ctx).CreateSubTotalAsync(subTotal, formulas);

            newId.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == newId)).FormulaExpression.Should().Be("Revenue - Cost of Goods Sold");
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == newId && f.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
        }

        [Fact]
        public async Task UpdateSubTotal_Should_Replace_Formulas_And_Rebuild_Expression()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, sectionId, "REV", "Revenue");
            var other = await SeedLineAsync(ctx, sectionId, "OI", "Other Income");

            var seedId = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { ScheduleIIIMasterId = masterId, SubTotalTypeId = misc["GROSSPROFIT"], FormulaExpression = string.Empty, DisplayOrder = 1 },
                new List<ScheduleIIISubTotalFormula>
                {
                    new() { OperandTypeId = misc["LINEITEM"], OperandRefId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 }
                });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateSubTotalAsync(seedId, misc["EBITDA"], true, new List<ScheduleIIISubTotalFormula>
            {
                new() { SubTotalId = seedId, OperandTypeId = misc["LINEITEM"], OperandRefId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 },
                new() { SubTotalId = seedId, OperandTypeId = misc["LINEITEM"], OperandRefId = other,   OperatorId = misc["PLUS"], DisplayOrder = 2 },
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == seedId);
            updated.SubTotalTypeId.Should().Be(misc["EBITDA"]);
            updated.IncludeOtherIncome.Should().BeTrue();
            updated.FormulaExpression.Should().Be("Revenue + Other Income");
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == seedId && f.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
        }
    }
}
