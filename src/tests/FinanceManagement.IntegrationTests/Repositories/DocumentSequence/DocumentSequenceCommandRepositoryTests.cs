using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.DocumentSequence;
using FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.DocumentSequence
{
    [Collection("DatabaseCollection")]
    public sealed class DocumentSequenceCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DocumentSequenceCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DocumentSequenceCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedTransactionTypeAsync(
            string typeName = "Invoice",
            string shortName = "INV",
            int unitId = 1,
            int moduleId = 1,
            int menuId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new TransactionTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.TransactionTypeMaster
            {
                UnitId = unitId,
                ModuleId = moduleId,
                MenuId = menuId,
                TypeName = typeName,
                ShortName = shortName,
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private static Domain.Entities.DocumentSequence BuildEntity(
            int transactionTypeId,
            int financialYearId = 1,
            int docNo = 1) =>
            new()
            {
                TransactionTypeId = transactionTypeId,
                FinancialYearId = financialYearId,
                DocNo = docNo,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedTransactionTypeAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedTransactionTypeAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, financialYearId: 2, docNo: 100));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DocumentSequence.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.TransactionTypeId.Should().Be(typeId);
            saved.FinancialYearId.Should().Be(2);
            saved.DocNo.Should().Be(100);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedTransactionTypeAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DocumentSequence.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedTransactionTypeAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, financialYearId: 1, docNo: 1));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.DocumentSequence.FirstAsync(x => x.Id == id);
            entity.DocNo = 999;
            entity.FinancialYearId = 5;
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.DocumentSequence.FirstAsync(x => x.Id == id);
            updated.DocNo.Should().Be(999);
            updated.FinancialYearId.Should().Be(5);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedTransactionTypeAsync();

            var entity = BuildEntity(typeId);
            entity.Id = 9999;

            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedTransactionTypeAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedTransactionTypeAsync();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.DocumentSequence
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
