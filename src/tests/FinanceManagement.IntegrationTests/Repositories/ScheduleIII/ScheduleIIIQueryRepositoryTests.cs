using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.ScheduleIII;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.ScheduleIII
{
    [Collection("DatabaseCollection")]
    public sealed class ScheduleIIIQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ScheduleIIIQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ScheduleIIIQueryRepository CreateQueryRepo()
        {
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Acme Mills" } });
            var division = new Mock<IDivisionLookup>(MockBehavior.Loose);
            division.Setup(d => d.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<DivisionLookupDto> { new() { Id = 7, Name = "Spinning Division" } });
            return new ScheduleIIIQueryRepository(new SqlConnection(_fixture.ConnectionString), company.Object, division.Object);
        }

        private static async Task<Dictionary<string, int>> SeedMiscAsync(ApplicationDbContext ctx)
        {
            var groups = new (string Type, (string Code, string Desc)[] Values)[]
            {
                ("S3_STMT_TYPE",    new[] { ("BS", "Balance Sheet"), ("PL", "Statement of P&L") }),
                ("S3_NATURE",       new[] { ("ASSET", "Asset"), ("INCOME", "Income") }),
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

        private static async Task<int> SeedSectionAsync(ApplicationDbContext ctx, int stmtTypeId, int natureId, string name)
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

        // Ensures a header for (company/division) and adds one detail line.
        private static async Task SeedDetailRowAsync(ApplicationDbContext ctx, int statusId, int sectionId, int sectionItemId, int displayOrder, int companyId = 1, int divisionId = 7)
        {
            var header = await ctx.ScheduleIIIHeader.FirstOrDefaultAsync(h => h.CompanyId == companyId && h.DivisionId == divisionId);
            if (header == null)
            {
                header = new ScheduleIIIHeader { CompanyId = companyId, DivisionId = divisionId, StatusId = statusId, TextileSplitEnabled = false };
                ctx.ScheduleIIIHeader.Add(header);
                await ctx.SaveChangesAsync();
            }

            ctx.ScheduleIIIDetail.Add(new ScheduleIIIDetail
            { ScheduleIIIHeaderId = header.Id, ScheduleIIISectionId = sectionId, ScheduleIIISectionItemId = sectionItemId, DisplayOrder = displayOrder });
            await ctx.SaveChangesAsync();
        }

        // ---- EXISTENCE / LOCK ------------------------------------------------

        [Fact]
        public async Task StructureExistsByCompanyDivision_TrueThenFalse()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Assets");
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, lineId, 1, companyId: 1, divisionId: 7);

            var repo = CreateQueryRepo();
            (await repo.StructureExistsByCompanyDivisionAsync(1, 7)).Should().BeTrue();
            (await repo.StructureExistsByCompanyDivisionAsync(1, 99)).Should().BeFalse();
        }

        [Fact]
        public async Task IsStructureLocked_ReflectsStatus()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Assets");
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, lineId, 1, divisionId: 7);
            await SeedDetailRowAsync(ctx, misc["LOCKED"], sectionId, lineId, 1, divisionId: 8);

            var repo = CreateQueryRepo();
            (await repo.IsStructureLockedAsync(1, 7)).Should().BeFalse();
            (await repo.IsStructureLockedAsync(1, 8)).Should().BeTrue();
        }

        [Fact]
        public async Task DetailLineExists_And_DisplayOrderExists_PerStructure()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Assets");
            var lineId = await SeedLineAsync(ctx, sectionId, "INV", "Inventories");
            await SeedDetailRowAsync(ctx, misc["DRAFT"], sectionId, lineId, 5, companyId: 1, divisionId: 7);

            var repo = CreateQueryRepo();
            (await repo.DetailLineExistsAsync(1, 7, lineId)).Should().BeTrue();        // line already included
            (await repo.DetailLineExistsAsync(1, 7, 99999)).Should().BeFalse();        // other line
            (await repo.DetailDisplayOrderExistsAsync(1, 7, 5)).Should().BeTrue();     // order taken
            (await repo.DetailDisplayOrderExistsAsync(1, 7, 6)).Should().BeFalse();    // order free
        }

        [Fact]
        public async Task SectionExists_TrueThenFalse()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Assets");

            var repo = CreateQueryRepo();
            (await repo.SectionExistsAsync(sectionId)).Should().BeTrue();
            (await repo.SectionExistsAsync(99999)).Should().BeFalse();
        }

        // ---- GET STRUCTURE (header + detail lines) ---------------------------

        [Fact]
        public async Task GetStructure_Returns_Sections_With_IncludedLines()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var section = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Non-Current Assets");
            var ppe = await SeedLineAsync(ctx, section, "PPE", "Property, Plant and Equipment");
            var inv = await SeedLineAsync(ctx, section, "INV", "Inventories");
            await SeedDetailRowAsync(ctx, misc["DRAFT"], section, ppe, 1, companyId: 1, divisionId: 7);
            await SeedDetailRowAsync(ctx, misc["DRAFT"], section, inv, 2, companyId: 1, divisionId: 7);

            var dto = await CreateQueryRepo().GetStructureAsync(1, 7);

            dto.Should().NotBeNull();
            dto!.CompanyName.Should().Be("Acme Mills");
            dto.DivisionName.Should().Be("Spinning Division");
            dto.StructureStatusName.Should().Be("Draft");
            dto.Sections.Should().HaveCount(1);
            dto.Sections[0].LineItems.Select(l => l.LineCode).Should().Contain(new[] { "PPE", "INV" });
            dto.Sections[0].LineItems.Should().OnlyContain(l => l.DetailId > 0);
        }

        [Fact]
        public async Task GetStructure_NoHeader_FallsBackToFullCatalog()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            // Catalog only — NO header / detail rows.
            var bs = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Assets");
            var pl = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            await SeedLineAsync(ctx, bs, "INV", "Inventories");
            await SeedLineAsync(ctx, bs, "TR", "Trade Receivables");
            await SeedLineAsync(ctx, pl, "REV", "Revenue");

            var dto = await CreateQueryRepo().GetStructureAsync(1, 7);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(0, "no header exists yet — this is the catalog fallback");
            dto.CompanyName.Should().Be("Acme Mills");
            dto.Sections.Should().HaveCount(2);                                  // both catalog sections
            dto.Sections.SelectMany(s => s.LineItems).Should().HaveCount(3);     // all catalog lines
            dto.Sections.Should().Contain(s => s.StatementTypeName == "Statement of P&L");
            // Each line carries its parent section name (not null).
            dto.Sections.Should().OnlyContain(s => s.LineItems.All(li => li.SectionName == s.SectionName));
        }

        [Fact]
        public async Task GetLinesAutoComplete_Returns_StructureLines_OrderedByDisplayOrder()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var section = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Assets");
            var ppe = await SeedLineAsync(ctx, section, "PPE", "Property, Plant and Equipment");
            var inv = await SeedLineAsync(ctx, section, "INV", "Inventories");
            // Insert out of display order to prove ORDER BY DisplayOrder.
            await SeedDetailRowAsync(ctx, misc["DRAFT"], section, inv, 2, companyId: 1, divisionId: 7);
            await SeedDetailRowAsync(ctx, misc["DRAFT"], section, ppe, 1, companyId: 1, divisionId: 7);

            var repo = CreateQueryRepo();

            var all = await repo.GetLinesAutoCompleteAsync(1, 7, null);
            all.Should().HaveCount(2);
            all.Select(x => x.LineCode).Should().ContainInOrder("PPE", "INV");   // by DisplayOrder 1,2
            all[0].SectionName.Should().Be("Current Assets");
            all[0].DisplayOrder.Should().Be(1);
            all[0].DetailId.Should().BeGreaterThan(0);

            var filtered = await repo.GetLinesAutoCompleteAsync(1, 7, "Inv");
            filtered.Should().ContainSingle(x => x.LineCode == "INV");
        }

        // ---- GET SUB-TOTALS (global) -----------------------------------------

        [Fact]
        public async Task GetSubTotals_Resolves_FormulaName_And_OperandNames()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var section = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, section, "REV", "Revenue");

            var subTotal = new ScheduleIIISubTotal { FormulaName = "Gross Profit", FormulaExpression = "Revenue", DisplayOrder = 1 };
            ctx.ScheduleIIISubTotal.Add(subTotal);
            await ctx.SaveChangesAsync();
            ctx.ScheduleIIISubTotalFormula.Add(new ScheduleIIISubTotalFormula
            { SubTotalId = subTotal.Id, OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 });
            await ctx.SaveChangesAsync();

            var result = await CreateQueryRepo().GetSubTotalsAsync();

            result.Should().ContainSingle();
            result[0].FormulaName.Should().Be("Gross Profit");
            result[0].Formulas.Should().HaveCount(1);
            result[0].Formulas[0].OperandName.Should().Be("Revenue");
            result[0].Formulas[0].OperatorName.Should().Be("Plus");
        }

        // ---- FORMULA OPERANDS (Edit-formula picker) --------------------------

        [Fact]
        public async Task GetSubTotalFormulaOperands_Returns_PL_Lines_With_Selection()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var plSection = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, plSection, "REV", "Revenue");
            var cogs = await SeedLineAsync(ctx, plSection, "COGS", "Cost of Goods Sold");

            var subTotal = new ScheduleIIISubTotal { FormulaName = "Gross Profit", FormulaExpression = "Revenue - Cost of Goods Sold", DisplayOrder = 1 };
            ctx.ScheduleIIISubTotal.Add(subTotal);
            await ctx.SaveChangesAsync();
            ctx.ScheduleIIISubTotalFormula.Add(new ScheduleIIISubTotalFormula
            { SubTotalId = subTotal.Id, OperandTypeId = misc["LINEITEM"], SectionItemId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 });
            await ctx.SaveChangesAsync();

            var operands = await CreateQueryRepo().GetSubTotalFormulaOperandsAsync(subTotal.Id);

            operands.Should().HaveCount(2);   // both P&L lines
            operands.Should().Contain(o => o.Id == revenue && o.IsSelected && o.OperatorCode == "PLUS");
            operands.Should().Contain(o => o.Id == cogs && !o.IsSelected);
        }

        // ---- SUB-TOTAL UNIQUENESS (FormulaName / DisplayOrder) ---------------

        [Fact]
        public async Task SubTotalNameExists_TrueForDuplicate_FalseForSelfAndMissing()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var gp = new ScheduleIIISubTotal { FormulaName = "Gross Profit", FormulaExpression = string.Empty, DisplayOrder = 1 };
            ctx.ScheduleIIISubTotal.Add(gp);
            await ctx.SaveChangesAsync();

            var repo = CreateQueryRepo();
            (await repo.SubTotalNameExistsAsync("Gross Profit")).Should().BeTrue();          // duplicate
            (await repo.SubTotalNameExistsAsync("Gross Profit", gp.Id)).Should().BeFalse();   // self excluded
            (await repo.SubTotalNameExistsAsync("EBITDA")).Should().BeFalse();                // not present
        }

        [Fact]
        public async Task SubTotalDisplayOrderExists_TrueForDuplicate_FalseForSelfAndMissing()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var gp = new ScheduleIIISubTotal { FormulaName = "Gross Profit", FormulaExpression = string.Empty, DisplayOrder = 3 };
            ctx.ScheduleIIISubTotal.Add(gp);
            await ctx.SaveChangesAsync();

            var repo = CreateQueryRepo();
            (await repo.SubTotalDisplayOrderExistsAsync(3)).Should().BeTrue();        // duplicate
            (await repo.SubTotalDisplayOrderExistsAsync(3, gp.Id)).Should().BeFalse(); // self excluded
            (await repo.SubTotalDisplayOrderExistsAsync(9)).Should().BeFalse();        // not present
        }

        // ---- SECTION UNIQUENESS (SectionName) --------------------------------

        [Fact]
        public async Task SectionNameExists_TrueForDuplicate_FalseForSelfAndMissing()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var sectionId = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Liabilities");

            var repo = CreateQueryRepo();
            (await repo.SectionNameExistsAsync("Current Liabilities")).Should().BeTrue();           // duplicate
            (await repo.SectionNameExistsAsync("Current Liabilities", sectionId)).Should().BeFalse(); // self excluded
            (await repo.SectionNameExistsAsync("Current Assets")).Should().BeFalse();               // not present
        }
    }
}
