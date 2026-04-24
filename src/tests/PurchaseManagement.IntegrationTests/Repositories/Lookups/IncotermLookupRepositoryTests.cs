using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Lookups.Purchase
{
    [Collection("DatabaseCollection")]
    public sealed class IncotermLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IncotermLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private IncotermLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new IncotermLookupRepository(conn);
        }

        private async Task<int> EnsureIncotermMiscTypeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.Description == MiscEnumEntity.Incoterms);
            if (existing != null) return existing.Id;

            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "Incoterms",
                Description = MiscEnumEntity.Incoterms,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            return miscType.Id;
        }

        private async Task SeedMiscMasterAsync(
            int miscTypeId,
            string code,
            string description,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.MiscMaster.Add(new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllIncotermAsync_Returns_Active_NonDeleted_Incoterms()
        {
            await _fixture.ClearAllTablesAsync();
            var typeId = await EnsureIncotermMiscTypeAsync();
            await SeedMiscMasterAsync(typeId, "EXW", "Ex Works");
            await SeedMiscMasterAsync(typeId, "FOB", "Free On Board");

            var result = await CreateRepo().GetAllIncotermAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.Code).Should().BeEquivalentTo(new[] { "EXW", "FOB" });
        }

        [Fact]
        public async Task GetAllIncotermAsync_Orders_By_Description_Asc()
        {
            await _fixture.ClearAllTablesAsync();
            var typeId = await EnsureIncotermMiscTypeAsync();
            await SeedMiscMasterAsync(typeId, "ZZZ", "Zulu term");
            await SeedMiscMasterAsync(typeId, "AAA", "Alpha term");
            await SeedMiscMasterAsync(typeId, "MMM", "Mike term");

            var result = await CreateRepo().GetAllIncotermAsync();

            result.Select(r => r.Description).Should().ContainInOrder("Alpha term", "Mike term", "Zulu term");
        }

        [Fact]
        public async Task GetAllIncotermAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var typeId = await EnsureIncotermMiscTypeAsync();
            await SeedMiscMasterAsync(typeId, "ACTIVE", "Active term");
            await SeedMiscMasterAsync(typeId, "INACTIVE", "Inactive term", isActive: false);

            var result = await CreateRepo().GetAllIncotermAsync();

            result.Should().ContainSingle().Which.Code.Should().Be("ACTIVE");
        }

        [Fact]
        public async Task GetAllIncotermAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var typeId = await EnsureIncotermMiscTypeAsync();
            await SeedMiscMasterAsync(typeId, "KEEP", "Kept term");
            await SeedMiscMasterAsync(typeId, "DEL", "Deleted term", isDeleted: true);

            var result = await CreateRepo().GetAllIncotermAsync();

            result.Should().ContainSingle().Which.Code.Should().Be("KEEP");
        }

        [Fact]
        public async Task GetAllIncotermAsync_Filters_By_MiscType_Description()
        {
            await _fixture.ClearAllTablesAsync();
            var incotermTypeId = await EnsureIncotermMiscTypeAsync();
            await SeedMiscMasterAsync(incotermTypeId, "EXW", "Ex Works");

            // Different MiscType — should be excluded
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var otherType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "Other",
                    Description = "SomeOtherType",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(otherType);
                await ctx.SaveChangesAsync();

                ctx.MiscMaster.Add(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = otherType.Id,
                    Code = "OTHER-CODE",
                    Description = "Other desc",
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().GetAllIncotermAsync();

            result.Should().ContainSingle().Which.Code.Should().Be("EXW");
        }

        [Fact]
        public async Task GetAllIncotermAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllIncotermAsync();

            result.Should().BeEmpty();
        }
    }
}
