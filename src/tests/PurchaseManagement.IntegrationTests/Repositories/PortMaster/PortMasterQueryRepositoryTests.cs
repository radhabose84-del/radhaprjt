using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.Port;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PortMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PortMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PortMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PortMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private PortMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task<int> SeedPortTypeAsync(ApplicationDbContext ctx)
        {
            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var mt = await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "PRT001",
                Description = "Port Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var misc = await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "SEA",
                Description = "Sea Port",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            return misc.Id;
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PortMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.DutyMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterDetail");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterHeader");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscTypeMaster");
        }

        private async Task<PurchaseManagement.Domain.Entities.PortMaster> SeedPortWithTypeAsync(
            ApplicationDbContext ctx,
            int portTypeId,
            string portCode = "PORT001",
            string portName = "Test Port",
            int countryId = 1) =>
            await CreateCommandRepo(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.PortMaster
                {
                    PortCode = portCode,
                    PortName = portName,
                    CountryId = countryId,
                    PortTypeId = portTypeId,
                    TypeId = null,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);

        private async Task<PurchaseManagement.Domain.Entities.PortMaster> SeedPortWithoutTypeAsync(
            ApplicationDbContext ctx,
            string portCode = "PORT001",
            string portName = "Test Port",
            int countryId = 1) =>
            await CreateCommandRepo(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.PortMaster
                {
                    PortCode = portCode,
                    PortName = portName,
                    CountryId = countryId,
                    PortTypeId = null,
                    TypeId = null,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var portTypeId = await SeedPortTypeAsync(ctx);
            var created = await SeedPortWithTypeAsync(ctx, portTypeId, "PORT001", "Mumbai Port");

            var dto = await CreateQueryRepo().GetByIdAsync(created.Id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(created.Id);
            dto.PortCode.Should().Be("PORT001");
            dto.PortName.Should().Be("Mumbai Port");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var dto = await CreateQueryRepo().GetByIdAsync(9999, CancellationToken.None);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_PortType()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var portTypeId = await SeedPortTypeAsync(ctx);
            var created = await SeedPortWithTypeAsync(ctx, portTypeId);

            var dto = await CreateQueryRepo().GetByIdAsync(created.Id, CancellationToken.None);

            dto!.PortType.Should().Be("SEA");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var portTypeId = await SeedPortTypeAsync(ctx);
            var created = await SeedPortWithTypeAsync(ctx, portTypeId);
            ctx.ChangeTracker.Clear();

            await CreateCommandRepo(ctx).SoftDeleteAsync(created.Id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(created.Id, CancellationToken.None);

            dto.Should().BeNull();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var portTypeId = await SeedPortTypeAsync(ctx);
            await SeedPortWithTypeAsync(ctx, portTypeId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null, null, CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_NoData()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null, null, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var portTypeId = await SeedPortTypeAsync(ctx);
            await SeedPortWithTypeAsync(ctx, portTypeId, "PORT001", "Mumbai Port");
            await SeedPortWithTypeAsync(ctx, portTypeId, "PORT002", "Chennai Port");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Mumbai", null, null, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].PortName.Should().Be("Mumbai Port");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_PortTypeId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var portTypeId = await SeedPortTypeAsync(ctx);
            await SeedPortWithTypeAsync(ctx, portTypeId, "PORT001", "Matching Port");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null, portTypeId, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].PortTypeId.Should().Be(portTypeId);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var portTypeId = await SeedPortTypeAsync(ctx);
            var created = await SeedPortWithTypeAsync(ctx, portTypeId);
            ctx.ChangeTracker.Clear();

            await CreateCommandRepo(ctx).SoftDeleteAsync(created.Id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null, null, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            // Autocomplete uses LEFT JOIN — no need to seed MiscMaster
            await SeedPortWithoutTypeAsync(ctx, "PAUTO1", "Auto Port");

            var results = await CreateQueryRepo().AutocompleteAsync(string.Empty, CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].portCode.Should().Be("PAUTO1");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_Term()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPortWithoutTypeAsync(ctx, "PAUTO2", "Kolkata Port");
            await SeedPortWithoutTypeAsync(ctx, "PAUTO3", "Delhi Dry Port");

            var results = await CreateQueryRepo().AutocompleteAsync("Kolkata", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].portname.Should().Be("Kolkata Port");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_When_NoMatch()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPortWithoutTypeAsync(ctx, "PAUTO4", "Test Port");

            var results = await CreateQueryRepo().AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var created = await SeedPortWithoutTypeAsync(ctx, "PAUTO5", "Deleted Port");
            ctx.ChangeTracker.Clear();

            await CreateCommandRepo(ctx).SoftDeleteAsync(created.Id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync(string.Empty, CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
