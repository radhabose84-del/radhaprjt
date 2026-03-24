using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.ServiceMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.ServiceMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ServiceMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ServiceMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ServiceCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static PurchaseManagement.Domain.Entities.ServiceMaster BuildEntity(
            string description = "Test Service",
            int sacId = 1,
            int uomId = 1,
            int serviceCategoryId = 1) =>
            new()
            {
                ServiceCode = string.Empty,
                ServiceDescription = description,
                SacId = sacId,
                UomId = uomId,
                ServiceCategoryId = serviceCategoryId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.ServiceMaster");

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(
                BuildEntity(), CancellationToken.None);

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_ServiceCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(
                BuildEntity(), CancellationToken.None);

            result.ServiceCode.Should().StartWith("SRV");
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_ServiceDescription()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(
                BuildEntity("Custom Service Description"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.ServiceMaster>()
                .FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.ServiceDescription.Should().Be("Custom Service Description");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(
                BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.ServiceMaster>()
                .FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Entity_With_Updated_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity("Updated Service Description");
            var result = await CreateRepository(ctx).UpdateAsync(
                created.Id, updated, CancellationToken.None);

            result.Should().NotBeNull();
            result.ServiceDescription.Should().Be("Updated Service Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_ServiceCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            var originalCode = created.ServiceCode;
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity("Updated Description");
            await CreateRepository(ctx).UpdateAsync(created.Id, updated, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.ServiceMaster>()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            saved!.ServiceCode.Should().Be(originalCode);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entityToDelete = new PurchaseManagement.Domain.Entities.ServiceMaster { Id = created.Id };
            var result = await CreateRepository(ctx).SoftDeleteAsync(entityToDelete, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entityToDelete = new PurchaseManagement.Domain.Entities.ServiceMaster { Id = created.Id };
            await CreateRepository(ctx).SoftDeleteAsync(entityToDelete, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Set<PurchaseManagement.Domain.Entities.ServiceMaster>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
