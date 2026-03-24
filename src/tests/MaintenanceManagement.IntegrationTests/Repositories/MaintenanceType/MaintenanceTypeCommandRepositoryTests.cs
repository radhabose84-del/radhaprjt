using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceType;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceType
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceTypeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceTypeCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceTypeCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static MaintenanceManagement.Domain.Entities.MaintenanceType BuildEntity(
            string typeName = "Corrective") =>
            new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                TypeName = typeName,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MaintenanceType]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Preventive"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MaintenanceType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.TypeName.Should().Be("Preventive");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MaintenanceType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Corrective"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                TypeName = "Updated Type",
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(newId, toUpdate);

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Corrective"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                TypeName = "Updated Type",
                IsActive = BaseEntity.Status.Active
            };

            await CreateRepository(ctx).UpdateAsync(newId, toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MaintenanceType.FirstOrDefaultAsync(x => x.Id == newId);
            updated.TypeName.Should().Be("Updated Type");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_NegativeOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var toUpdate = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                TypeName = "Not Exists",
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(9999, toUpdate);

            result.Should().Be(-1);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(newId, toDelete);

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            await CreateRepository(ctx).DeleteAsync(newId, toDelete);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MaintenanceType
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_NegativeOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var toDelete = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(9999, toDelete);

            result.Should().Be(-1);
        }

        // --- EXISTS BY CODE ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await CreateRepository(ctx).CreateAsync(BuildEntity("Corrective"));

            var exists = await CreateRepository(ctx).ExistsByCodeAsync("Corrective");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var exists = await CreateRepository(ctx).ExistsByCodeAsync("NonExistent");

            exists.Should().BeFalse();
        }
    }
}
