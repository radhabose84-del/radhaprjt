using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PriceMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PriceMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PriceMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IMiscMasterQueryRepository> _miscQueryRepo = new(MockBehavior.Loose);

        public PriceMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PriceMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object, _miscQueryRepo.Object);

        private PriceMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ctx = _fixture.CreateFreshDbContext();
            return new PriceMasterQueryRepository(ctx, _fixture.IpMock.Object, conn, _miscQueryRepo.Object);
        }

        private static PriceMasterHeader BuildHeader(
            int itemId = 1,
            int vendorId = 1,
            int statusId = 1,
            int sourceFromId = 1) =>
            new()
            {
                ItemId = itemId,
                VendorId = vendorId,
                UnitId = 1,
                UomId = 1,
                ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                StatusId = statusId,
                SourceFromId = sourceFromId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = new List<PriceMasterDetail>
                {
                    new()
                    {
                        ScaleQtyFrom = 1,
                        ScaleQtyTo = 100,
                        UnitPrice = 100m,
                        CurrencyId = 1,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };

        private async Task<int> SeedHeaderAsync(
            int itemId = 1,
            int vendorId = 1,
            int statusId = 1,
            int sourceFromId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateCommandRepo(ctx);
            var header = BuildHeader(itemId, vendorId, statusId, sourceFromId);
            await repo.AddAsync(header, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);
            return header.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterDetails");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterHeader");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var id = await SeedHeaderAsync(itemId: 100, vendorId: 200);

            var dto = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.ItemId.Should().Be(100);
            dto.VendorId.Should().Be(200);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999, CancellationToken.None);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedHeaderAsync(itemId: 101, vendorId: 201);

            // Soft delete
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).DeleteAsync(id);
            ctx.ChangeTracker.Clear();

            var dto = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Include_Details()
        {
            await ClearTablesAsync();
            var id = await SeedHeaderAsync(itemId: 102, vendorId: 202);

            var dto = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Details.Should().NotBeEmpty();
            dto.Details.First().UnitPrice.Should().Be(100m);
        }

        // --- GET FOR EDIT ---

        [Fact]
        public async Task GetForEditAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var id = await SeedHeaderAsync(itemId: 110, vendorId: 210);

            var dto = await CreateQueryRepo().GetForEditAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.ItemId.Should().Be(110);
            dto.VendorId.Should().Be(210);
        }

        [Fact]
        public async Task GetForEditAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var dto = await CreateQueryRepo().GetForEditAsync(9999, CancellationToken.None);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetForEditAsync_Should_Include_Details()
        {
            await ClearTablesAsync();
            var id = await SeedHeaderAsync(itemId: 111, vendorId: 211);

            var dto = await CreateQueryRepo().GetForEditAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Details.Should().NotBeEmpty();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await ClearTablesAsync();
            await SeedHeaderAsync(itemId: 120, vendorId: 220);

            var result = await CreateQueryRepo().GetAllAsync(
                1, 15, null, null, null, null, null, false, CancellationToken.None);

            result.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedHeaderAsync(itemId: 121, vendorId: 221);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).DeleteAsync(id);
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().GetAllAsync(
                1, 15, null, null, null, null, null, false, CancellationToken.None);

            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_EmptyTable_ReturnsEmptyResult()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetAllAsync(
                1, 15, null, null, null, null, null, false, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.Total.Should().Be(0);
        }
    }
}
