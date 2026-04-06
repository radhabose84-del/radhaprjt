using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMapping;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAddressMapping
{
    [Collection("DatabaseCollection")]
    public sealed class DispatchAddressMappingQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DispatchAddressMappingQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DispatchAddressMappingQueryRepository CreateQueryRepo(Mock<IPartyLookup>? partyLookup = null)
        {
            var mockParty = partyLookup ?? BuildDefaultPartyLookup();
            return new DispatchAddressMappingQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                mockParty.Object);
        }

        private Mock<IPartyLookup> BuildDefaultPartyLookup(int partyId = 1, string partyName = "Test Party")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            var dto = new Contracts.Dtos.Lookups.Party.PartyLookupDto
            {
                Id = partyId,
                PartyCode = "PARTY001",
                PartyName = partyName
            };
            mock.Setup(x => x.GetByIdAsync(partyId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto> { dto });
            return mock;
        }

        private DispatchAddressMappingCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new DispatchAddressMappingCommandRepository(ctx);

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.DispatchAddressMapping");
        }

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
                    DispatchAddressName = "Query Test Dispatch Address",
                    AddressLine1 = "Address Line 1",
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
                        MiscTypeCode = "QUSAGETYPE",
                        Description = "Query Usage Type",
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
                    Code = "QUSAGE001",
                    Description = "Query Usage 1",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx2.MiscMaster.Add(mm);
                await ctx2.SaveChangesAsync();
                utId = mm.Id;
            }

            return (daId, utId);
        }

        private async Task<int> SeedAsync(int partyId, int dispatchAddressId, int usageTypeId, bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(new Domain.Entities.DispatchAddressMapping
            {
                PartyId = partyId,
                DispatchAddressId = dispatchAddressId,
                UsageTypeId = usageTypeId,
                IsDefault = false,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_SeededRecord()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            await SeedAsync(1, daId, utId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            var id = await SeedAsync(1, daId, utId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_ByPartyId()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            await SeedAsync(partyId: 10, daId, utId);
            await SeedAsync(partyId: 11, daId, utId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, partyId: 10);

            total.Should().Be(1);
            items[0].PartyId.Should().Be(10);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_PartyName_From_Lookup()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            await SeedAsync(partyId: 1, daId, utId);

            var mockParty = BuildDefaultPartyLookup(1, "Acme Party");
            var (items, _) = await CreateQueryRepo(mockParty).GetAllAsync(1, 10, null);

            items[0].PartyName.Should().Be("Acme Party");
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            var id = await SeedAsync(partyId: 1, daId, utId);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.PartyId.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_AfterSoftDelete()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            var id = await SeedAsync(1, daId, utId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── NotFoundAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            var id = await SeedAsync(1, daId, utId);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            var id = await SeedAsync(1, daId, utId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── CompositeKeyExistsAsync ──────────────────────────────────────────

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_WhenExists()
        {
            await ClearTableAsync();
            var (daId, utId) = await EnsureFkDependenciesAsync();
            await SeedAsync(partyId: 1, daId, utId);

            var result = await CreateQueryRepo().CompositeKeyExistsAsync(1, daId, utId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_WhenNotExists()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().CompositeKeyExistsAsync(99, 99, 99);

            result.Should().BeFalse();
        }
    }
}
