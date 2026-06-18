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

        // Ensures a header for (company/division) and adds one detail line. Returns the detail id.
        private static async Task<int> SeedDetailRowAsync(ApplicationDbContext ctx, int statusId, int sectionId, int sectionItemId, int displayOrder = 1, int companyId = 1, int divisionId = 7)
        {
            var header = await ctx.ScheduleIIIHeader.FirstOrDefaultAsync(h => h.CompanyId == companyId && h.DivisionId == divisionId);
            if (header == null)
            {
                header = new ScheduleIIIHeader { CompanyId = companyId, DivisionId = divisionId, StatusId = statusId, TextileSplitEnabled = false };
                ctx.ScheduleIIIHeader.Add(header);
                await ctx.SaveChangesAsync();
            }

            var d = new ScheduleIIIDetail
            { ScheduleIIIHeaderId = header.Id, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = sectionItemId, DisplayOrder = displayOrder };
            ctx.ScheduleIIIDetail.Add(d);
            await ctx.SaveChangesAsync();
            return d.Id;
        }

        // ---- HEADER + DETAIL -------------------------------------------------

        [Fact]
        public async Task EnsureHeader_Defaults_To_Draft_And_CreateDetail_Persists()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");

            var repo = CreateRepository(ctx);
            var headerId = await repo.EnsureHeaderAsync(1, 7);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIHeader.FirstAsync(x => x.Id == headerId)).StatusId
                .Should().Be(misc["DRAFT"], "a new header always starts as DRAFT");

            var detailId = await CreateRepository(ctx).CreateDetailAsync(new ScheduleIIIDetail
            { ScheduleIIIHeaderId = headerId, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = lineId, DisplayOrder = 1 });

            detailId.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIDetail.FirstAsync(x => x.Id == detailId)).ScheduleIIISectionItemId.Should().Be(lineId);
        }

        [Fact]
        public async Task EnsureHeader_Is_Idempotent_Per_CompanyDivision()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedMiscAsync(ctx);

            var repo = CreateRepository(ctx);
            var first = await repo.EnsureHeaderAsync(1, 7);
            var second = await CreateRepository(ctx).EnsureHeaderAsync(1, 7);

            second.Should().Be(first);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIHeader.CountAsync(h => h.CompanyId == 1 && h.DivisionId == 7)).Should().Be(1);
        }

        [Fact]
        public async Task UpdateHeader_Should_Set_Status_And_TextileSplit()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            await CreateRepository(ctx).EnsureHeaderAsync(1, 7);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateHeaderAsync(1, 7, misc["LOCKED"], true);
            ctx.ChangeTracker.Clear();

            var header = await ctx.ScheduleIIIHeader.FirstAsync(x => x.CompanyId == 1 && x.DivisionId == 7);
            header.StatusId.Should().Be(misc["LOCKED"]);
            header.TextileSplitEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task LockStructure_Should_Lock_The_Header()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var l1 = await SeedLineAsync(ctx, sectionId, "A", "Alpha");
            var l2 = await SeedLineAsync(ctx, sectionId, "B", "Beta");
            await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, l1, 1);
            await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, l2, 2);
            var headerId = (await ctx.ScheduleIIIHeader.FirstAsync(h => h.CompanyId == 1 && h.DivisionId == 7)).Id;
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).LockStructureAsync(headerId)).Should().BeTrue();
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIIHeader.FirstAsync(x => x.Id == headerId)).StatusId
                .Should().Be(misc["LOCKED"]);
        }

        [Fact]
        public async Task SoftDeleteDetail_Should_Remove_One_Line()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            var d = await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, lineId, 1);
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).SoftDeleteDetailAsync(d, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIDetail.IgnoreQueryFilters().FirstAsync(x => x.Id == d)).IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task ReorderDetail_Should_Swap_DisplayOrder()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var l1 = await SeedLineAsync(ctx, sectionId, "A", "Alpha");
            var l2 = await SeedLineAsync(ctx, sectionId, "B", "Beta");
            var d1 = await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, l1, 1);
            var d2 = await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, l2, 2);
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).ReorderDetailAsync(d2, 1, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIIDetail.FirstAsync(x => x.Id == d2)).DisplayOrder.Should().Be(1);
            (await ctx.ScheduleIIIDetail.FirstAsync(x => x.Id == d1)).DisplayOrder.Should().Be(2);
        }

        [Fact]
        public async Task CreateDetailRange_Inserts_All_Lines()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var l1 = await SeedLineAsync(ctx, sectionId, "A", "Alpha");
            var l2 = await SeedLineAsync(ctx, sectionId, "B", "Beta");
            var l3 = await SeedLineAsync(ctx, sectionId, "C", "Gamma");
            var headerId = await CreateRepository(ctx).EnsureHeaderAsync(1, 7);
            ctx.ChangeTracker.Clear();

            var n = await CreateRepository(ctx).CreateDetailRangeAsync(new List<ScheduleIIIDetail>
            {
                new() { ScheduleIIIHeaderId = headerId, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = l1, DisplayOrder = 1 },
                new() { ScheduleIIIHeaderId = headerId, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = l2, DisplayOrder = 2 },
                new() { ScheduleIIIHeaderId = headerId, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = l3, DisplayOrder = 3 },
            });

            n.Should().Be(3);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIDetail.CountAsync(x => x.ScheduleIIIHeaderId == headerId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(3);
        }

        [Fact]
        public async Task UpdateDetailRange_Swaps_Orders_Without_Unique_Violation()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"]);
            var l1 = await SeedLineAsync(ctx, sectionId, "A", "Alpha");
            var l2 = await SeedLineAsync(ctx, sectionId, "B", "Beta");
            var d1 = await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, l1, 1);
            var d2 = await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, l2, 2);
            ctx.ChangeTracker.Clear();

            // Swap display orders (1↔2) and deactivate d1 — the UNIQUE(HeaderId, DisplayOrder) index must not trip.
            var n = await CreateRepository(ctx).UpdateDetailRangeAsync(new List<ScheduleIIIDetail>
            {
                new() { Id = d1, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = l1, DisplayOrder = 2, IsActive = Status.Inactive },
                new() { Id = d2, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = l2, DisplayOrder = 1, IsActive = Status.Active },
            });

            n.Should().Be(2);
            ctx.ChangeTracker.Clear();
            (await ctx.ScheduleIIIDetail.FirstAsync(x => x.Id == d1)).DisplayOrder.Should().Be(2);
            (await ctx.ScheduleIIIDetail.FirstAsync(x => x.Id == d2)).DisplayOrder.Should().Be(1);
            (await ctx.ScheduleIIIDetail.FirstAsync(x => x.Id == d1)).IsActive.Should().Be(Status.Inactive);
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

        // ---- SUB-TOTAL HEADER (CRUD) -----------------------------------------

        [Fact]
        public async Task CreateSubTotal_Should_Persist_Header_Only()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "Gross Profit", IncludeOtherIncome = false, DisplayOrder = 1 });

            id.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == id);
            saved.FormulaName.Should().Be("Gross Profit");
            saved.FormulaExpression.Should().BeEmpty();   // operands not saved yet
        }

        [Fact]
        public async Task UpdateSubTotal_Should_Update_Header_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "Gross Profit", DisplayOrder = 1 });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateSubTotalAsync(new ScheduleIIISubTotal
            {
                Id = id,
                FormulaName = "EBITDA",
                IncludeOtherIncome = true,
                DisplayOrder = 2,
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == id);
            updated.FormulaName.Should().Be("EBITDA");
            updated.IncludeOtherIncome.Should().BeTrue();
            updated.DisplayOrder.Should().Be(2);
        }

        [Fact]
        public async Task SoftDeleteSubTotal_Should_Set_IsDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "Gross Profit", DisplayOrder = 1 });
            ctx.ChangeTracker.Clear();

            (await CreateRepository(ctx).SoftDeleteSubTotalAsync(id, CancellationToken.None)).Should().BeTrue();
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ScheduleIIISubTotal.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        // ---- SUB-TOTAL FORMULA (operands) ------------------------------------

        [Fact]
        public async Task SaveSubTotalFormula_Should_Insert_Operands_And_Build_Expression()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, sectionId, "REV", "Revenue");
            var cogs = await SeedLineAsync(ctx, sectionId, "COGS", "Cost of Goods Sold");

            var gpId = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "Gross Profit", DisplayOrder = 1 });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SaveSubTotalFormulaAsync(gpId, new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"],  DisplayOrder = 1 },
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = cogs,    OperatorId = misc["MINUS"], DisplayOrder = 2 },
            });
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == gpId)).FormulaExpression.Should().Be("Revenue - Cost of Goods Sold");
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == gpId && f.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
        }

        [Fact]
        public async Task SaveSubTotalFormula_Should_Support_SubTotal_Operand()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var other = await SeedLineAsync(ctx, sectionId, "OI", "Other Income");

            var gpId = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "Gross Profit", DisplayOrder = 1 });
            var ebitdaId = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "EBITDA", DisplayOrder = 2 });
            ctx.ChangeTracker.Clear();

            // EBITDA = + Gross Profit (a sub-total operand) + Other Income (a line operand)
            await CreateRepository(ctx).SaveSubTotalFormulaAsync(ebitdaId, new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["SUBTOTAL"], OperandSubTotalId = gpId,  OperatorId = misc["PLUS"], DisplayOrder = 1 },
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = other,     OperatorId = misc["PLUS"], DisplayOrder = 2 },
            });
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == ebitdaId)).FormulaExpression.Should().Be("Gross Profit + Other Income");
            var subOperand = await ctx.ScheduleIIISubTotalFormula.FirstAsync(f => f.SubTotalId == ebitdaId && f.OperandSubTotalId != null);
            subOperand.OperandSubTotalId.Should().Be(gpId);
            subOperand.SectionItemId.Should().BeNull();
        }

        [Fact]
        public async Task SaveSubTotalFormula_Should_Replace_Operands_Physically()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, sectionId, "REV", "Revenue");
            var other = await SeedLineAsync(ctx, sectionId, "OI", "Other Income");

            var id = await CreateRepository(ctx).CreateSubTotalAsync(
                new ScheduleIIISubTotal { FormulaName = "Gross Profit", DisplayOrder = 1 });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SaveSubTotalFormulaAsync(id, new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 }
            });
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SaveSubTotalFormulaAsync(id, new List<ScheduleIIISubTotalFormula>
            {
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 },
                new() { OperandTypeId = misc["LINEITEM"], SectionItemId = other,   OperatorId = misc["PLUS"], DisplayOrder = 2 },
            });
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIISubTotal.FirstAsync(x => x.Id == id)).FormulaExpression.Should().Be("Revenue + Other Income");
            // Old operands physically deleted, only the new set remains.
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => f.SubTotalId == id)).Should().Be(2);
        }
    }
}
