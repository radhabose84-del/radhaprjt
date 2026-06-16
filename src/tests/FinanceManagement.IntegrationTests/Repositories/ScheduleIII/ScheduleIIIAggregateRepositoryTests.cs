using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.ScheduleIII;
using FinanceManagement.IntegrationTests.Common;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.ScheduleIII
{
    [Collection("DatabaseCollection")]
    public sealed class ScheduleIIIAggregateRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ScheduleIIIAggregateRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static ScheduleIIICommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

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

        private static ScheduleIIIInput BuildInput(Dictionary<string, int> m, int companyId = 1, int divisionId = 7) => new()
        {
            CompanyId = companyId,
            DivisionId = divisionId,
            StructureStatusId = m["DRAFT"],
            TextileSplitEnabled = 0,
            VersionNo = 1,
            Sections = new List<SectionInput>
            {
                new()
                {
                    SectionName = "Non-Current Assets", StatementTypeId = m["BS"], NatureId = m["ASSET"], DisplayOrder = 1,
                    LineItems = new List<LineItemInput>
                    {
                        new() { LineCode = "PPE",     LineName = "Property, Plant and Equipment", DisplayOrder = 1, IsSplitLine = 0 },
                        new() { LineCode = "PPE-TAN", LineName = "Tangible assets", ParentLineCode = "PPE", DisplayOrder = 1, IsSplitLine = 0 },
                    }
                },
                new()
                {
                    SectionName = "Income", StatementTypeId = m["PL"], NatureId = m["INCOME"], DisplayOrder = 2,
                    LineItems = new List<LineItemInput>
                    {
                        new() { LineCode = "REV",  LineName = "Revenue", DisplayOrder = 1, IsSplitLine = 0 },
                        new() { LineCode = "COGS", LineName = "Cost of Goods Sold", DisplayOrder = 2, IsSplitLine = 0 },
                    }
                }
            },
            SubTotals = new List<SubTotalInput>
            {
                new()
                {
                    SubTotalName = "Gross Profit", IncludeOtherIncome = 0, DisplayOrder = 1,
                    Formulas = new List<FormulaInput>
                    {
                        new() { OperandTypeId = m["LINEITEM"], OperandCode = "REV",  OperatorId = m["PLUS"],  DisplayOrder = 1 },
                        new() { OperandTypeId = m["LINEITEM"], OperandCode = "COGS", OperatorId = m["MINUS"], DisplayOrder = 2 },
                    }
                },
                new()
                {
                    SubTotalName = "EBITDA", IncludeOtherIncome = 1, DisplayOrder = 2,
                    Formulas = new List<FormulaInput>
                    {
                        new() { OperandTypeId = m["SUBTOTAL"], OperandCode = "Gross Profit", OperatorId = m["PLUS"], DisplayOrder = 1 },
                    }
                }
            }
        };

        [Fact]
        public async Task CreateAggregate_Persists_All_Five_Tables_With_Expressions_And_ParentLinks()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = await SeedMiscAsync(ctx);

            var structureId = await CreateCommandRepo(ctx).CreateAggregateAsync(BuildInput(m));

            structureId.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();

            (await ctx.ScheduleIIISection.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
            (await ctx.ScheduleIIILineItem.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(4);
            (await ctx.ScheduleIIISubTotal.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);

            var subIds = await ctx.ScheduleIIISubTotal.Where(x => x.StructureId == structureId).Select(x => x.Id).ToListAsync();
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => subIds.Contains(f.SubTotalId) && f.IsDeleted == IsDelete.NotDeleted)).Should().Be(3);

            var gp = await ctx.ScheduleIIISubTotal.FirstAsync(x => x.StructureId == structureId && x.SubTotalName == "Gross Profit");
            gp.FormulaExpression.Should().Be("Revenue - Cost of Goods Sold");

            // Parent/child link resolved by LineCode
            var tangible = await ctx.ScheduleIIILineItem.FirstAsync(x => x.StructureId == structureId && x.LineCode == "PPE-TAN");
            var ppe = await ctx.ScheduleIIILineItem.FirstAsync(x => x.StructureId == structureId && x.LineCode == "PPE");
            tangible.ParentLineId.Should().Be(ppe.Id);
        }

        [Fact]
        public async Task GetById_Returns_Full_Tree()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = await SeedMiscAsync(ctx);
            var structureId = await CreateCommandRepo(ctx).CreateAggregateAsync(BuildInput(m));

            var dto = await CreateQueryRepo().GetByIdAsync(structureId);

            dto.Should().NotBeNull();
            dto!.CompanyName.Should().Be("Acme Mills");
            dto.Sections.Should().HaveCount(2);
            dto.SubTotals.Should().HaveCount(2);
            var income = dto.Sections.First(s => s.SectionName == "Income");
            income.LineItems.Should().HaveCount(2);
            var gp = dto.SubTotals.First(s => s.SubTotalName == "Gross Profit");
            gp.Formulas.Should().HaveCount(2);
            gp.Formulas[0].OperandName.Should().Be("Revenue");
        }

        [Fact]
        public async Task UpdateAggregate_Upserts_InPlace_And_Does_Not_Delete()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = await SeedMiscAsync(ctx);
            var structureId = await CreateCommandRepo(ctx).CreateAggregateAsync(BuildInput(m));
            ctx.ChangeTracker.Clear();

            // Target existing rows by Id to update them in place.
            var section = await ctx.ScheduleIIISection.FirstAsync(x => x.StructureId == structureId && x.SectionName == "Income");
            var line = await ctx.ScheduleIIILineItem.FirstAsync(x => x.StructureId == structureId && x.LineCode == "REV");

            var update = new ScheduleIIIInput
            {
                Id = structureId,
                CompanyId = 1,
                DivisionId = 7,
                StructureStatusId = m["DRAFT"],
                TextileSplitEnabled = 1,
                VersionNo = 2,
                IsActive = 1,
                Sections = new List<SectionInput>
                {
                    new()
                    {
                        Id = section.Id,
                        SectionName = "Income (revised)",
                        StatementTypeId = m["PL"], NatureId = m["INCOME"], DisplayOrder = 2,
                        LineItems = new List<LineItemInput>
                        {
                            new() { Id = line.Id, LineCode = "REV", LineName = "Revenue (revised)", DisplayOrder = 1, IsSplitLine = 0 }
                        }
                    }
                },
                SubTotals = new List<SubTotalInput>()   // omitted -> untouched (not deleted)
            };

            await CreateCommandRepo(ctx).UpdateAggregateAsync(update);
            ctx.ChangeTracker.Clear();

            // structure scalars updated
            var structure = await ctx.ScheduleIIIStructure.FirstAsync(x => x.Id == structureId);
            structure.TextileSplitEnabled.Should().BeTrue();
            structure.VersionNo.Should().Be(2);

            // targeted rows updated in place (same Ids)
            (await ctx.ScheduleIIISection.FirstAsync(x => x.Id == section.Id)).SectionName.Should().Be("Income (revised)");
            (await ctx.ScheduleIIILineItem.FirstAsync(x => x.Id == line.Id)).LineName.Should().Be("Revenue (revised)");

            // nothing deleted or soft-deleted — counts unchanged from create
            (await ctx.ScheduleIIISection.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
            (await ctx.ScheduleIIILineItem.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(4);
            (await ctx.ScheduleIIISubTotal.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
        }

        [Fact]
        public async Task UpdateAggregate_WithoutChildIds_MatchesByNaturalKey_NoDuplicates()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = await SeedMiscAsync(ctx);
            var structureId = await CreateCommandRepo(ctx).CreateAggregateAsync(BuildInput(m));
            ctx.ChangeTracker.Clear();

            // Re-send the SAME shape (same section names / line codes / sub-total names) with NO child ids,
            // changing one line's name — must update in place, not duplicate.
            var update = BuildInput(m);
            update.Id = structureId;
            update.Sections.SelectMany(s => s.LineItems).First(l => l.LineCode == "REV").LineName = "Revenue (v2)";

            await CreateCommandRepo(ctx).UpdateAggregateAsync(update);
            ctx.ChangeTracker.Clear();

            // counts unchanged — no duplicate rows inserted
            (await ctx.ScheduleIIISection.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
            (await ctx.ScheduleIIILineItem.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(4);
            (await ctx.ScheduleIIISubTotal.CountAsync(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(2);
            var subIds = await ctx.ScheduleIIISubTotal.Where(x => x.StructureId == structureId).Select(x => x.Id).ToListAsync();
            (await ctx.ScheduleIIISubTotalFormula.CountAsync(f => subIds.Contains(f.SubTotalId) && f.IsDeleted == IsDelete.NotDeleted)).Should().Be(3);

            // the matched line was updated in place (single REV row, new name)
            (await ctx.ScheduleIIILineItem.CountAsync(x => x.StructureId == structureId && x.LineCode == "REV" && x.IsDeleted == IsDelete.NotDeleted)).Should().Be(1);
            (await ctx.ScheduleIIILineItem.FirstAsync(x => x.StructureId == structureId && x.LineCode == "REV")).LineName.Should().Be("Revenue (v2)");
        }
    }
}
