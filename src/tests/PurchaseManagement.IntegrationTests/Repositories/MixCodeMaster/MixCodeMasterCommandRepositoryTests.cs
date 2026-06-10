using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MixCodeMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.MixCodeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MixCodeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MixCodeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static MixCodeMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static PurchaseManagement.Domain.Entities.MixCodeMaster BuildEntity(
            string code = "MIX001",
            string desc = "Test Mix") =>
            new()
            {
                MixCode = code,
                MixCodeDesc = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("MIX002", "Cotton 60/40"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MixCodeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.MixCode.Should().Be("MIX002");
            saved.MixCodeDesc.Should().Be("Cotton 60/40");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MixCodeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Description_Change()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("MIX003", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new PurchaseManagement.Domain.Entities.MixCodeMaster
            {
                Id = id,
                MixCodeDesc = "Changed",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MixCodeMaster.FirstAsync(x => x.Id == id);
            updated.MixCodeDesc.Should().Be("Changed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_MixCode()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("ORIG01", "Original"));
            ctx.ChangeTracker.Clear();

            // Even if a different code is supplied, the immutable MixCode must be preserved.
            await CreateRepository(ctx).UpdateAsync(new PurchaseManagement.Domain.Entities.MixCodeMaster
            {
                Id = id,
                MixCode = "HACKED",
                MixCodeDesc = "Changed",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MixCodeMaster.FirstAsync(x => x.Id == id);
            updated.MixCode.Should().Be("ORIG01");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MixCodeMaster.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
