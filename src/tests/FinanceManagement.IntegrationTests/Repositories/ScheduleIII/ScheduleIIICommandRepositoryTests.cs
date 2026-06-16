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

        private static async Task<int> SeedStructureAsync(ApplicationDbContext ctx, int statusId, int companyId = 1, int divisionId = 7)
        {
            var s = new ScheduleIIIStructure
            {
                CompanyId = companyId,
                DivisionId = divisionId,
                StructureStatusId = statusId,
                TextileSplitEnabled = false,
                VersionNo = 1
            };
            ctx.ScheduleIIIStructure.Add(s);
            await ctx.SaveChangesAsync();
            return s.Id;
        }

        private static async Task<int> SeedSectionAsync(ApplicationDbContext ctx, int structureId, int stmtTypeId, int natureId, string name = "Current Assets")
        {
            var sec = new ScheduleIIISection
            {
                StructureId = structureId,
                SectionName = name,
                StatementTypeId = stmtTypeId,
                NatureId = natureId,
                DisplayOrder = 1
            };
            ctx.ScheduleIIISection.Add(sec);
            await ctx.SaveChangesAsync();
            return sec.Id;
        }

        private static async Task<int> SeedLineAsync(ApplicationDbContext ctx, int structureId, int sectionId, string code, string name, int order)
        {
            var line = new ScheduleIIILineItem
            {
                StructureId = structureId,
                SectionId = sectionId,
                LineCode = code,
                LineName = name,
                DisplayOrder = order
            };
            ctx.ScheduleIIILineItem.Add(line);
            await ctx.SaveChangesAsync();
            return line.Id;
        }

        // ---- CREATE / UPDATE / DELETE line item ------------------------------

        [Fact]
        public async Task CreateLineItem_Should_Return_NewId_And_Persist()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, structureId, misc["BS"], misc["ASSET"]);

            var newId = await CreateRepository(ctx).CreateLineItemAsync(new ScheduleIIILineItem
            {
                StructureId = structureId,
                SectionId = sectionId,
                LineCode = "INV",
                LineName = "Inventories",
                NoteReference = "Note 8",
                DisplayOrder = 1
            });

            newId.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ScheduleIIILineItem.FirstAsync(x => x.Id == newId);
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
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, structureId, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, structureId, sectionId, "INV", "Inventories", 1);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateLineItemAsync(new ScheduleIIILineItem
            {
                Id = lineId,
                LineName = "Inventories (revised)",
                NoteReference = "Note 8A",
                DisplayOrder = 2,
                IsActive = Status.Active
            });

            ctx.ChangeTracker.Clear();
            var updated = await ctx.ScheduleIIILineItem.FirstAsync(x => x.Id == lineId);
            updated.LineName.Should().Be("Inventories (revised)");
            updated.LineCode.Should().Be("INV"); // immutable
        }

        [Fact]
        public async Task SoftDeleteLineItem_Should_SetFlag_And_ReturnFalse_WhenMissing()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, structureId, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, structureId, sectionId, "INV", "Inventories", 1);

            var deleted = await CreateRepository(ctx).SoftDeleteLineItemAsync(lineId, CancellationToken.None);
            var missing = await CreateRepository(ctx).SoftDeleteLineItemAsync(999999, CancellationToken.None);

            deleted.Should().BeTrue();
            missing.Should().BeFalse();
            ctx.ChangeTracker.Clear();
            var row = await ctx.ScheduleIIILineItem.IgnoreQueryFilters().FirstAsync(x => x.Id == lineId);
            row.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task ReorderLineItem_Should_Swap_DisplayOrder_With_Neighbour()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var sectionId = await SeedSectionAsync(ctx, structureId, misc["BS"], misc["ASSET"]);
            var first = await SeedLineAsync(ctx, structureId, sectionId, "A", "First", 1);
            var second = await SeedLineAsync(ctx, structureId, sectionId, "B", "Second", 2);
            ctx.ChangeTracker.Clear();

            var ok = await CreateRepository(ctx).ReorderLineItemAsync(first, 2, CancellationToken.None); // move down

            ok.Should().BeTrue();
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIILineItem.FirstAsync(x => x.Id == first)).DisplayOrder.Should().Be(2);
            (await ctx.ScheduleIIILineItem.FirstAsync(x => x.Id == second)).DisplayOrder.Should().Be(1);
        }

        // ---- SUB-TOTAL (+ formula expression) --------------------------------

        [Fact]
        public async Task CreateSubTotal_Should_Persist_Formulas_And_Build_Expression()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var plSection = await SeedSectionAsync(ctx, structureId, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, structureId, plSection, "REV", "Revenue", 1);
            var cogs = await SeedLineAsync(ctx, structureId, plSection, "COGS", "Cost of Goods Sold", 2);
            ctx.ChangeTracker.Clear();

            var subTotal = new ScheduleIIISubTotal
            {
                StructureId = structureId,
                SubTotalName = "Gross Profit",
                FormulaExpression = string.Empty,
                IncludeOtherIncome = false,
                IsSystemDefined = false,
                DisplayOrder = 1
            };
            var formulas = new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["LINEITEM"], OperandRefId = revenue, OperatorId = misc["PLUS"],  DisplayOrder = 1 },
                new() { OperandTypeId = misc["LINEITEM"], OperandRefId = cogs,    OperatorId = misc["MINUS"], DisplayOrder = 2 },
            };

            var newId = await CreateRepository(ctx).CreateSubTotalAsync(subTotal, formulas);

            newId.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == newId);
            saved.FormulaExpression.Should().Be("Revenue - Cost of Goods Sold");
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == newId && f.IsDeleted == IsDelete.NotDeleted))
                .Should().Be(2);
        }

        [Fact]
        public async Task UpdateSubTotal_Should_Replace_Formulas_And_Rebuild_Expression()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var plSection = await SeedSectionAsync(ctx, structureId, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, structureId, plSection, "REV", "Revenue", 1);
            var other = await SeedLineAsync(ctx, structureId, plSection, "OI", "Other Income", 2);

            var seedId = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { StructureId = structureId, SubTotalName = "X", FormulaExpression = string.Empty, DisplayOrder = 1 },
                new List<ScheduleIIISubTotalFormula>
                {
                    new() { OperandTypeId = misc["LINEITEM"], OperandRefId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 }
                });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateSubTotalAsync(seedId, "Revised", true, new List<ScheduleIIISubTotalFormula>
            {
                new() { SubTotalId = seedId, OperandTypeId = misc["LINEITEM"], OperandRefId = revenue, OperatorId = misc["PLUS"],  DisplayOrder = 1 },
                new() { SubTotalId = seedId, OperandTypeId = misc["LINEITEM"], OperandRefId = other,   OperatorId = misc["PLUS"],  DisplayOrder = 2 },
            });

            ctx.ChangeTracker.Clear();
            var updated = await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == seedId);
            updated.SubTotalName.Should().Be("Revised");
            updated.IncludeOtherIncome.Should().BeTrue();
            updated.FormulaExpression.Should().Be("Revenue + Other Income");
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == seedId && f.IsDeleted == IsDelete.NotDeleted))
                .Should().Be(2);
        }

        // ---- LOCK ------------------------------------------------------------

        [Fact]
        public async Task LockStructure_Should_Set_Status_To_Locked()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            ctx.ChangeTracker.Clear();

            var ok = await CreateRepository(ctx).LockStructureAsync(structureId);

            ok.Should().BeTrue();
            ctx.ChangeTracker.Clear();
            var structure = await ctx.ScheduleIIIStructure.FirstAsync(x => x.Id == structureId);
            structure.StructureStatusId.Should().Be(misc["LOCKED"]);
        }
    }
}
