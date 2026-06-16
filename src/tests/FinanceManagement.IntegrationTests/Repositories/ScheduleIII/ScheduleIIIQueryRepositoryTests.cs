using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.ScheduleIII;
using FinanceManagement.IntegrationTests.Common;
using static FinanceManagement.Domain.Common.BaseEntity;

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

        private ScheduleIIIQueryRepository CreateQueryRepo(
            Mock<ICompanyLookup>? companyLookup = null,
            Mock<IDivisionLookup>? divisionLookup = null)
        {
            companyLookup ??= BuildCompanyLookup();
            divisionLookup ??= BuildDivisionLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ScheduleIIIQueryRepository(conn, companyLookup.Object, divisionLookup.Object);
        }

        private static Mock<ICompanyLookup> BuildCompanyLookup(int companyId = 1, string name = "Acme Mills")
        {
            var mock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = companyId, CompanyName = name } });
            return mock;
        }

        private static Mock<IDivisionLookup> BuildDivisionLookup(int divisionId = 7, string name = "Spinning Division")
        {
            var mock = new Mock<IDivisionLookup>(MockBehavior.Loose);
            mock.Setup(d => d.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<DivisionLookupDto> { new() { Id = divisionId, Name = name } });
            return mock;
        }

        // ---- seeding (via EF) ------------------------------------------------

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
            { CompanyId = companyId, DivisionId = divisionId, StructureStatusId = statusId, VersionNo = 3 };
            ctx.ScheduleIIIStructure.Add(s);
            await ctx.SaveChangesAsync();
            return s.Id;
        }

        private static async Task<int> SeedSectionAsync(ApplicationDbContext ctx, int structureId, int stmtTypeId, int natureId, string name, int order)
        {
            var sec = new ScheduleIIISection
            { StructureId = structureId, SectionName = name, StatementTypeId = stmtTypeId, NatureId = natureId, DisplayOrder = order };
            ctx.ScheduleIIISection.Add(sec);
            await ctx.SaveChangesAsync();
            return sec.Id;
        }

        private static async Task<int> SeedLineAsync(ApplicationDbContext ctx, int structureId, int sectionId, string code, string name, int order, int? parentId = null, Status active = Status.Active)
        {
            var line = new ScheduleIIILineItem
            {
                StructureId = structureId,
                SectionId = sectionId,
                ParentLineId = parentId,
                LineCode = code,
                LineName = name,
                DisplayOrder = order,
                IsActive = active
            };
            ctx.ScheduleIIILineItem.Add(line);
            await ctx.SaveChangesAsync();
            return line.Id;
        }

        // ---- EXISTENCE / LOCK ------------------------------------------------

        [Fact]
        public async Task StructureExistsByCompanyDivision_TrueThenFalse()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            await SeedStructureAsync(ctx, misc["DRAFT"], companyId: 1, divisionId: 7);

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
            var draft = await SeedStructureAsync(ctx, misc["DRAFT"], companyId: 1, divisionId: 7);
            var locked = await SeedStructureAsync(ctx, misc["LOCKED"], companyId: 1, divisionId: 8);

            var repo = CreateQueryRepo();
            (await repo.IsStructureLockedAsync(draft)).Should().BeFalse();
            (await repo.IsStructureLockedAsync(locked)).Should().BeTrue();
        }

        // ---- GET STRUCTURE ---------------------------------------------------

        [Fact]
        public async Task GetStructure_Returns_Sections_NestedLines_And_Names()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"], companyId: 1, divisionId: 7);
            var section = await SeedSectionAsync(ctx, structureId, misc["BS"], misc["ASSET"], "Non-Current Assets", 1);
            var ppe = await SeedLineAsync(ctx, structureId, section, "PPE", "Property, Plant and Equipment", 1);
            await SeedLineAsync(ctx, structureId, section, "PPE-TAN", "Tangible assets", 1, parentId: ppe);

            var dto = await CreateQueryRepo().GetStructureAsync(1, 7);

            dto.Should().NotBeNull();
            dto!.CompanyName.Should().Be("Acme Mills");
            dto.DivisionName.Should().Be("Spinning Division");
            dto.StructureStatusName.Should().Be("Draft");
            dto.Sections.Should().HaveCount(1);
            dto.Sections[0].StatementTypeName.Should().Be("Balance Sheet");
            dto.Sections[0].NatureName.Should().Be("Asset");
            dto.Sections[0].LineItems.Should().HaveCount(1);               // PPE is the only top-level line
            dto.Sections[0].LineItems[0].ChildLines.Should().HaveCount(1); // Tangible nested under PPE
        }

        [Fact]
        public async Task GetStructure_Missing_ReturnsNull()
        {
            await _fixture.ClearAllTablesAsync();
            (await CreateQueryRepo().GetStructureAsync(1, 7)).Should().BeNull();
        }

        // ---- 03B PREVIEW -----------------------------------------------------

        [Fact]
        public async Task Get03BPreview_Splits_BS_and_PL_And_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var bs = await SeedSectionAsync(ctx, structureId, misc["BS"], misc["ASSET"], "Current Assets", 1);
            var pl = await SeedSectionAsync(ctx, structureId, misc["PL"], misc["EXPENSE"], "Expenses", 2);
            await SeedLineAsync(ctx, structureId, bs, "INV", "Inventories", 1);
            await SeedLineAsync(ctx, structureId, pl, "COMC", "Cost of Materials Consumed", 1);
            await SeedLineAsync(ctx, structureId, pl, "OLD", "Inactive line", 2, active: Status.Inactive);

            var preview = await CreateQueryRepo().Get03BPreviewAsync(structureId);

            preview.BalanceSheetLeaves.Should().ContainSingle(x => x.LineName == "Inventories");
            preview.ProfitAndLossLeaves.Should().ContainSingle(x => x.LineName == "Cost of Materials Consumed");
            preview.ProfitAndLossLeaves.Should().NotContain(x => x.LineName == "Inactive line");
        }

        // ---- SUB-TOTALS ------------------------------------------------------

        [Fact]
        public async Task GetSubTotals_Returns_Formulas_With_ResolvedOperandNames()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var pl = await SeedSectionAsync(ctx, structureId, misc["PL"], misc["INCOME"], "Income", 1);
            var revenue = await SeedLineAsync(ctx, structureId, pl, "REV", "Revenue", 1);

            var subTotal = new ScheduleIIISubTotal
            {
                StructureId = structureId,
                SubTotalName = "Gross Profit",
                FormulaExpression = "Revenue",
                DisplayOrder = 1
            };
            ctx.ScheduleIIISubTotal.Add(subTotal);
            await ctx.SaveChangesAsync();
            ctx.ScheduleIIISubTotalFormula.Add(new ScheduleIIISubTotalFormula
            {
                SubTotalId = subTotal.Id,
                OperandTypeId = misc["LINEITEM"],
                OperandRefId = revenue,
                OperatorId = misc["PLUS"],
                DisplayOrder = 1
            });
            await ctx.SaveChangesAsync();

            var result = await CreateQueryRepo().GetSubTotalsAsync(structureId);

            result.Should().HaveCount(1);
            result[0].SubTotalName.Should().Be("Gross Profit");
            result[0].Formulas.Should().HaveCount(1);
            result[0].Formulas[0].OperandName.Should().Be("Revenue");
            result[0].Formulas[0].OperatorName.Should().Be("Plus");
        }

        // ---- GET LINE ITEM BY ID --------------------------------------------

        [Fact]
        public async Task GetLineItemById_Returns_Dto()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var misc = await SeedMiscAsync(ctx);
            var structureId = await SeedStructureAsync(ctx, misc["DRAFT"]);
            var section = await SeedSectionAsync(ctx, structureId, misc["BS"], misc["ASSET"], "Current Assets", 1);
            var lineId = await SeedLineAsync(ctx, structureId, section, "INV", "Inventories", 1);

            var dto = await CreateQueryRepo().GetLineItemByIdAsync(lineId);

            dto.Should().NotBeNull();
            dto!.LineCode.Should().Be("INV");
            dto.LineName.Should().Be("Inventories");
        }
    }
}
