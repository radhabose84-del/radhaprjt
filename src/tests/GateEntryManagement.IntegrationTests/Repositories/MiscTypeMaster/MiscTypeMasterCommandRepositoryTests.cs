using GateEntryManagement.Infrastructure.Repositories.MiscTypeMaster;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscTypeMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static GateEntryManagement.Domain.Entities.MiscTypeMaster BuildEntity(
            string miscTypeCode = "TYPE001", string description = "Test Type") =>
            new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("TYPE001", "Test Type"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MiscTypeCode.Should().Be("TYPE001");
            saved.Description.Should().Be("Test Type");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

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
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var entityToUpdate = new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                Description = "Updated Description",
                IsActive = Status.Active
            };
            var result = await CreateRepository(ctx).UpdateAsync(entityToUpdate);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == id);
            updated.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entityToUpdate = new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                Id = 9999,
                Description = "Does Not Exist",
                IsActive = Status.Active
            };
            var result = await CreateRepository(ctx).UpdateAsync(entityToUpdate);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscTypeMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
