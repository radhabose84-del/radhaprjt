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
            var s = new ScheduleIIIMaster { CompanyId = companyId, DivisionId = divisionId, StatusId = statusId, VersionNo = 3 };
            ctx.ScheduleIIIMaster.Add(s);
            await ctx.SaveChangesAsync();
            return s.Id;
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

        private static async Task SeedMasterLineAsync(ApplicationDbContext ctx, int masterId, int lineId, int order)
        {
            ctx.ScheduleIIIMasterLine.Add(new ScheduleIIIMasterLine { ScheduleIIIMasterId = masterId, ScheduleIIISectionItemId = lineId, DisplayOrder = order });
            await ctx.SaveChangesAsync();
        }

        // ---- EXISTENCE / LOCK ------------------------------------------------

        [Fact]
        public async Task StructureExistsByCompanyDivision_TrueThenFalse()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            await SeedMasterAsync(ctx, misc["DRAFT"], companyId: 1, divisionId: 7);

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
            var draft = await SeedMasterAsync(ctx, misc["DRAFT"], divisionId: 7);
            var locked = await SeedMasterAsync(ctx, misc["LOCKED"], divisionId: 8);

            var repo = CreateQueryRepo();
            (await repo.IsStructureLockedAsync(draft)).Should().BeFalse();
            (await repo.IsStructureLockedAsync(locked)).Should().BeTrue();
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

        [Fact]
        public async Task SubTotalTypeExists_TrueThenFalse()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);

            var repo = CreateQueryRepo();
            (await repo.SubTotalTypeExistsAsync(misc["GROSSPROFIT"])).Should().BeTrue();
            (await repo.SubTotalTypeExistsAsync(misc["BS"])).Should().BeFalse();   // wrong misc type
        }

        // ---- GET STRUCTURE (via junction) ------------------------------------

        [Fact]
        public async Task GetStructure_Returns_Sections_With_LinkedLines()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"], companyId: 1, divisionId: 7);
            var section = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Non-Current Assets");
            var ppe = await SeedLineAsync(ctx, section, "PPE", "Property, Plant and Equipment");
            var inv = await SeedLineAsync(ctx, section, "INV", "Inventories");
            await SeedMasterLineAsync(ctx, masterId, ppe, 1);
            await SeedMasterLineAsync(ctx, masterId, inv, 2);

            var dto = await CreateQueryRepo().GetStructureAsync(1, 7);

            dto.Should().NotBeNull();
            dto!.CompanyName.Should().Be("Acme Mills");
            dto.DivisionName.Should().Be("Spinning Division");
            dto.StructureStatusName.Should().Be("Draft");
            dto.Sections.Should().HaveCount(1);
            dto.Sections[0].LineItems.Should().HaveCount(2);
            dto.Sections[0].LineItems.Select(l => l.LineCode).Should().Contain(new[] { "PPE", "INV" });
        }

        // ---- GET MASTER LINES ------------------------------------------------

        [Fact]
        public async Task GetMasterLines_Returns_Linked_Lines_With_Names()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);
            var section = await SeedSectionAsync(ctx, misc["BS"], misc["ASSET"], "Current Assets");
            var inv = await SeedLineAsync(ctx, section, "INV", "Inventories");
            await SeedMasterLineAsync(ctx, masterId, inv, 1);

            var result = await CreateQueryRepo().GetMasterLinesAsync(masterId);

            result.Should().HaveCount(1);
            result[0].LineCode.Should().Be("INV");
            result[0].LineName.Should().Be("Inventories");
            result[0].SectionName.Should().Be("Current Assets");
        }

        // ---- GET SUB-TOTALS --------------------------------------------------

        [Fact]
        public async Task GetSubTotals_Resolves_Name_From_MiscMaster_And_Operands()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var masterId = await SeedMasterAsync(ctx, misc["DRAFT"]);
            var section = await SeedSectionAsync(ctx, misc["PL"], misc["INCOME"], "Income");
            var revenue = await SeedLineAsync(ctx, section, "REV", "Revenue");

            var subTotal = new ScheduleIIISubTotal
            { ScheduleIIIMasterId = masterId, SubTotalTypeId = misc["GROSSPROFIT"], FormulaExpression = "Revenue", DisplayOrder = 1 };
            ctx.ScheduleIIISubTotal.Add(subTotal);
            await ctx.SaveChangesAsync();
            ctx.ScheduleIIISubTotalFormula.Add(new ScheduleIIISubTotalFormula
            { SubTotalId = subTotal.Id, OperandTypeId = misc["LINEITEM"], OperandRefId = revenue, OperatorId = misc["PLUS"], DisplayOrder = 1 });
            await ctx.SaveChangesAsync();

            var result = await CreateQueryRepo().GetSubTotalsAsync(masterId);

            result.Should().HaveCount(1);
            result[0].SubTotalName.Should().Be("Gross Profit");
            result[0].Formulas.Should().HaveCount(1);
            result[0].Formulas[0].OperandName.Should().Be("Revenue");
            result[0].Formulas[0].OperatorName.Should().Be("Plus");
        }
    }
}
