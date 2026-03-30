using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.BankMaster;

namespace PartyManagement.IntegrationTests.Repositories.BankMaster
{
    [Collection("DatabaseCollection")]
    public sealed class BankMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BankMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BankMasterCommandRepository CreateRepository(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static PartyManagement.Domain.Entities.BankMaster BuildEntity(
            string code = "BNK001",
            string name = "Test Bank") =>
            new PartyManagement.Domain.Entities.BankMaster
            {
                BankCode = code,
                BankName = name,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Party].[BankAccount]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Party].[BankMaster]");
        }

        // --- ADD (CREATE) ---

        [Fact]
        public async Task AddAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).AddAsync(BuildEntity(), CancellationToken.None);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).AddAsync(BuildEntity("HDFC001", "HDFC Bank"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BankMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.BankCode.Should().Be("HDFC001");
            saved.BankName.Should().Be("HDFC Bank");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task AddAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).AddAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BankMaster.FirstOrDefaultAsync(x => x.Id == newId);

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
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).AddAsync(BuildEntity("SBI001", "State Bank"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.BankMaster.FirstAsync(x => x.Id == newId);
            entity.BankName = "Updated Bank Name";
            await CreateRepository(ctx).UpdateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BankMaster.FirstAsync(x => x.Id == newId);
            updated.BankName.Should().Be("Updated Bank Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_BankCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).AddAsync(BuildEntity("AXIS001", "Axis Bank"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.BankMaster.FirstAsync(x => x.Id == newId);
            entity.BankName = "Different Name";
            await CreateRepository(ctx).UpdateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BankMaster.FirstAsync(x => x.Id == newId);
            updated.BankCode.Should().Be("AXIS001");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).AddAsync(BuildEntity("DEL001", "Delete Bank"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.BankMaster.FirstAsync(x => x.Id == newId);
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            await CreateRepository(ctx).SoftDeleteAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.BankMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
