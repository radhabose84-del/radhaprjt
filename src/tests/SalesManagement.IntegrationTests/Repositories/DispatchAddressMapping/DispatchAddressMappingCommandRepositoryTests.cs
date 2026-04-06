using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMapping;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAddressMapping
{
    [Collection("DatabaseCollection")]
    public sealed class DispatchAddressMappingCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        // Seeded FK IDs — lazily populated once per test class via the fixture's shared DB
        private int _dispatchAddressId;
        private int _usageTypeId;

        public DispatchAddressMappingCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DispatchAddressMappingCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new DispatchAddressMappingCommandRepository(ctx);

        private async Task<(int daId, int utId)> EnsureFkDependenciesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var daId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.DispatchAddressMaster WHERE IsDeleted = 0");
            if (daId == 0)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                var da = new Domain.Entities.DispatchAddressMaster
                {
                    DispatchAddressName = "Test Dispatch Address",
                    AddressLine1 = "Line 1",
                    CityId = 1, StateId = 1, CountryId = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.DispatchAddressMaster.Add(da);
                await ctx.SaveChangesAsync();
                daId = da.Id;
            }

            var utId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.MiscMaster WHERE IsDeleted = 0 AND IsActive = 1");
            if (utId == 0)
            {
                var miscTypeId = await conn.ExecuteScalarAsync<int>(
                    "SELECT TOP 1 Id FROM Sales.MiscTypeMaster WHERE IsDeleted = 0");
                if (miscTypeId == 0)
                {
                    await using var ctx = _fixture.CreateFreshDbContext();
                    var mt = new Domain.Entities.MiscTypeMaster
                    {
                        MiscTypeCode = "USAGETYPE",
                        Description = "Usage Type",
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    };
                    ctx.MiscTypeMaster.Add(mt);
                    await ctx.SaveChangesAsync();
                    miscTypeId = mt.Id;
                }

                await using var ctx2 = _fixture.CreateFreshDbContext();
                var mm = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = "USAGE001",
                    Description = "Usage Type 1",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx2.MiscMaster.Add(mm);
                await ctx2.SaveChangesAsync();
                utId = mm.Id;
            }

            return (daId, utId);
        }

        private Domain.Entities.DispatchAddressMapping BuildEntity(
            int partyId = 1,
            int dispatchAddressId = 0,
            int usageTypeId = 0,
            bool isDefault = false)
            => new Domain.Entities.DispatchAddressMapping
            {
                PartyId = partyId,
                DispatchAddressId = dispatchAddressId,
                UsageTypeId = usageTypeId,
                IsDefault = isDefault,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.DispatchAddressMapping");
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (daId, utId) = await EnsureFkDependenciesAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(dispatchAddressId: daId, usageTypeId: utId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (daId, utId) = await EnsureFkDependenciesAsync();

            var entity = BuildEntity(partyId: 5, dispatchAddressId: daId, usageTypeId: utId, isDefault: true);
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMapping.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PartyId.Should().Be(5);
            saved.DispatchAddressId.Should().Be(daId);
            saved.UsageTypeId.Should().Be(utId);
            saved.IsDefault.Should().BeTrue();
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (daId, utId) = await EnsureFkDependenciesAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(dispatchAddressId: daId, usageTypeId: utId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMapping.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (daId, utId) = await EnsureFkDependenciesAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(dispatchAddressId: daId, usageTypeId: utId, isDefault: false));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.DispatchAddressMapping
            {
                Id = id,
                PartyId = 5,
                DispatchAddressId = daId,
                UsageTypeId = utId,
                IsDefault = true,
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);
            var saved = await ctx.DispatchAddressMapping.FirstOrDefaultAsync(x => x.Id == id);
            saved!.IsDefault.Should().BeTrue();
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var updated = new Domain.Entities.DispatchAddressMapping
            {
                Id = 99999,
                PartyId = 1,
                DispatchAddressId = 1,
                UsageTypeId = 1,
                IsDefault = false,
                IsActive = Status.Active
            };

            var resultId = await CreateRepository(ctx).UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        // ── SoftDeleteAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (daId, utId) = await EnsureFkDependenciesAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(dispatchAddressId: daId, usageTypeId: utId));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (daId, utId) = await EnsureFkDependenciesAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(dispatchAddressId: daId, usageTypeId: utId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMapping
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
