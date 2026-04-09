using Dapper;
using Microsoft.Data.SqlClient;
using LogisticsManagement.Domain.Common;
using LogisticsManagement.Infrastructure.Data;
using LogisticsManagement.Infrastructure.Repositories.MiscTypeMaster;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static MiscTypeMasterCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx);

        private static Domain.Entities.MiscTypeMaster BuildEntity(
            string code = "MTCODE01",
            string description = "Test MiscType") =>
            new()
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscTypeMaster");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("MTCODE01", "Test Description"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MiscTypeCode.Should().Be("MTCODE01");
            saved.Description.Should().Be("Test Description");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Multiple_Should_Return_Unique_Ids()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id1 = await CreateRepository(ctx).CreateAsync(BuildEntity("CODE001"));
            var id2 = await CreateRepository(ctx).CreateAsync(BuildEntity("CODE002"));

            id1.Should().NotBe(id2);
            id1.Should().BeGreaterThan(0);
            id2.Should().BeGreaterThan(0);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.MiscTypeMaster.FirstAsync(x => x.Id == id);
            entity.Description = "Updated Description";
            var result = await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.MiscTypeMaster.FirstAsync(x => x.Id == id);
            updated.Description.Should().Be("Updated Description");
            result.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("ORIG001"));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.MiscTypeMaster.FirstAsync(x => x.Id == id);
            entity.Description = "Changed Name Only";
            await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.MiscTypeMaster.FirstAsync(x => x.Id == id);
            updated.MiscTypeCode.Should().Be("ORIG001");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity();
            entity.Id = 9999;
            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_Modified_Audit_Fields()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.MiscTypeMaster.FirstAsync(x => x.Id == id);
            entity.Description = "Modified";
            await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.MiscTypeMaster.FirstAsync(x => x.Id == id);
            updated.ModifiedBy.Should().Be(1);
            updated.ModifiedByName.Should().Be("test-user");
            updated.ModifiedIP.Should().Be("127.0.0.1");
            updated.ModifiedDate.Should().NotBeNull();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var deleted = await ctx3.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_Already_Deleted()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx3).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
