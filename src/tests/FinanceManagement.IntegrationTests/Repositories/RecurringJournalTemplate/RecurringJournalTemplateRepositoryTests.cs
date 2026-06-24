using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.RecurringJournalTemplate
{
    [Collection("DatabaseCollection")]
    public sealed class RecurringJournalTemplateRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RecurringJournalTemplateRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private RecurringJournalTemplateCommandRepository CreateCommandRepo(FinanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);
        private RecurringJournalTemplateQueryRepository CreateQueryRepo() => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Lines()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);

            int newId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                newId = await CreateCommandRepo(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            newId.Should().BeGreaterThan(0);

            await using var verify = _fixture.CreateFreshDbContext();
            var lines = await verify.RecurringJournalTemplateDetail.Where(d => d.TemplateId == newId).ToListAsync();
            lines.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Lines_And_Header()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateCommandRepo(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = RecurringTemplateSeed.BuildTemplate(ids, "Monthly Rent — Edited");
                entity.Id = id;
                entity.AutoPost = false;
                await CreateCommandRepo(ctx).UpdateAsync(entity);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var header = await verify.RecurringJournalTemplateHeader.FirstAsync(h => h.Id == id);
            header.TemplateName.Should().Be("Monthly Rent — Edited");
            header.AutoPost.Should().BeFalse();
            (await verify.RecurringJournalTemplateDetail.CountAsync(d => d.TemplateId == id)).Should().Be(2);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateCommandRepo(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            await using (var ctx = _fixture.CreateFreshDbContext())
                (await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None)).Should().BeTrue();

            await using var verify = _fixture.CreateFreshDbContext();
            var deleted = await verify.RecurringJournalTemplateHeader.IgnoreQueryFilters().FirstAsync(h => h.Id == id);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Lines_And_Names()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateCommandRepo(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Lines.Should().HaveCount(2);
            dto.VoucherTypeCode.Should().Be("JV");
            dto.FrequencyName.Should().Be("Monthly");
            dto.AmountAdjustmentRuleName.Should().Be("Fixed");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Template()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task AlreadyExistsByNameAsync_Should_Return_True_For_Duplicate()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids, "Monthly Rent — Silvassa"));

            (await CreateQueryRepo().AlreadyExistsByNameAsync("Monthly Rent — Silvassa")).Should().BeTrue();
        }

        [Fact]
        public async Task FkValidators_Should_Resolve_Seeded_Misc_And_Masters()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            var repo = CreateQueryRepo();

            (await repo.FrequencyExistsAsync(ids.FrequencyId)).Should().BeTrue();
            (await repo.AmountAdjustmentRuleExistsAsync(ids.AmountAdjustmentRuleId)).Should().BeTrue();
            (await repo.VoucherTypeExistsAsync(ids.VoucherTypeId, 1)).Should().BeTrue();
            (await repo.GlAccountExistsAsync(ids.GlAccountDrId, 1)).Should().BeTrue();
            (await repo.CostCentreExistsAsync(ids.CostCentreId)).Should().BeTrue();
            (await repo.ProfitCentreExistsAsync(ids.ProfitCentreId)).Should().BeTrue();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Template()
        {
            await ClearTableAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids, "Monthly Rent — Silvassa"));

            var results = await CreateQueryRepo().AutocompleteAsync("Rent", CancellationToken.None);

            results.Should().ContainSingle(r => r.TemplateName == "Monthly Rent — Silvassa");
        }
    }
}
