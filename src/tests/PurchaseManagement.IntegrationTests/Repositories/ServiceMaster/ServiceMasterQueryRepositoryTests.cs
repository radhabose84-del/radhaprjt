using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.ServiceMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.ServiceMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ServiceMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ServiceMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ServiceQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), _fixture.IpMock.Object);

        private ServiceCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedServiceCategoryAsync(ApplicationDbContext ctx)
        {
            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var mt = await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "SVC001",
                Description = "Service Category Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var misc = await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "SVCAT01",
                Description = "General Service",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            return misc.Id;
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        private async Task<PurchaseManagement.Domain.Entities.ServiceMaster> SeedServiceAsync(
            ApplicationDbContext ctx,
            int serviceCategoryId,
            string description = "Test Service",
            bool active = true)
        {
            var entity = new PurchaseManagement.Domain.Entities.ServiceMaster
            {
                ServiceCode = string.Empty,
                ServiceDescription = description,
                SacId = 1,
                UomId = 1,
                ServiceCategoryId = serviceCategoryId,
                IsActive = active ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };
            return await CreateCommandRepo(ctx).CreateAsync(entity, CancellationToken.None);
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllServiceMasterAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            await SeedServiceAsync(ctx, catId);

            var (items, total) = await CreateQueryRepo().GetAllServiceMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllServiceMasterAsync_Should_Return_Empty_When_NoData()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (items, total) = await CreateQueryRepo().GetAllServiceMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllServiceMasterAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            await SeedServiceAsync(ctx, catId, "Plumbing Service");
            await SeedServiceAsync(ctx, catId, "Electrical Work");

            var (items, total) = await CreateQueryRepo().GetAllServiceMasterAsync(1, 10, "Plumbing");

            items.Should().HaveCount(1);
            items[0].ServiceDescription.Should().Be("Plumbing Service");
        }

        [Fact]
        public async Task GetAllServiceMasterAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            var created = await SeedServiceAsync(ctx, catId);
            ctx.ChangeTracker.Clear();

            var entityToDelete = new PurchaseManagement.Domain.Entities.ServiceMaster { Id = created.Id, IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted };
            await CreateCommandRepo(ctx).SoftDeleteAsync(entityToDelete, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllServiceMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllServiceMasterAsync_Should_Populate_ServiceCategory()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            await SeedServiceAsync(ctx, catId);

            var (items, _) = await CreateQueryRepo().GetAllServiceMasterAsync(1, 10, null);

            items[0].ServiceCategory.Should().Be("SVCAT01");
            items[0].ServiceCategoryDescription.Should().Be("General Service");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetServiceMasterByIdAsync_Should_Return_Correct_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            var created = await SeedServiceAsync(ctx, catId, "Cleaning Service");

            var dto = await CreateQueryRepo().GetServiceMasterByIdAsync(created.Id);

            dto.Should().NotBeNull();
            dto.Id.Should().Be(created.Id);
            dto.ServiceDescription.Should().Be("Cleaning Service");
        }

        [Fact]
        public async Task GetServiceMasterByIdAsync_Should_Return_Default_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var dto = await CreateQueryRepo().GetServiceMasterByIdAsync(9999);

            // Implementation returns new GetServiceMasterDto() when not found (Id = 0)
            dto.Id.Should().Be(0);
        }

        [Fact]
        public async Task GetServiceMasterByIdAsync_Should_Populate_ServiceCategory()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            var created = await SeedServiceAsync(ctx, catId);

            var dto = await CreateQueryRepo().GetServiceMasterByIdAsync(created.Id);

            dto.ServiceCategory.Should().Be("SVCAT01");
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task ServiceMasterAuotoComplete_Should_Return_Active_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            await SeedServiceAsync(ctx, catId, "Active Service", active: true);

            var results = await CreateQueryRepo().ServiceMasterAuotoComplete(null);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task ServiceMasterAuotoComplete_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            await SeedServiceAsync(ctx, catId, "Inactive Service", active: false);

            var results = await CreateQueryRepo().ServiceMasterAuotoComplete(null);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task ServiceMasterAuotoComplete_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            await SeedServiceAsync(ctx, catId, "Network Setup");
            await SeedServiceAsync(ctx, catId, "Security Audit");

            var results = await CreateQueryRepo().ServiceMasterAuotoComplete("Network");

            results.Should().HaveCount(1);
            results[0].ServiceDescription.Should().Be("Network Setup");
        }

        [Fact]
        public async Task ServiceMasterAuotoComplete_Should_Return_Empty_When_NoMatch()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedServiceCategoryAsync(ctx);
            await SeedServiceAsync(ctx, catId);

            var results = await CreateQueryRepo().ServiceMasterAuotoComplete("ZZZNOMATCH");

            results.Should().BeEmpty();
        }
    }
}
