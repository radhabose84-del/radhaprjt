using Contracts.Dtos.Lookups.Logistics;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        public DispatchAddressMappingQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DispatchAddressMappingQueryRepository CreateRepo(
            Mock<IPartyLookup>? party = null,
            Mock<IFreightMasterLookup>? freight = null)
        {
            if (party == null)
            {
                party = new Mock<IPartyLookup>(MockBehavior.Loose);
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>());
            }
            if (freight == null)
            {
                freight = new Mock<IFreightMasterLookup>(MockBehavior.Loose);
                freight.Setup(f => f.GetAllFreightMasterAsync())
                    .ReturnsAsync(new List<FreightMasterLookupDto>());
            }

            var cityMock = new Mock<ICityLookup>(MockBehavior.Loose);
            cityMock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CityLookupDto>)new List<CityLookupDto>());
            cityMock.Setup(c => c.GetAllCityAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityLookupDto>());

            var stateMock = new Mock<IStateLookup>(MockBehavior.Loose);
            stateMock.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<StateLookupDto>)new List<StateLookupDto>());
            stateMock.Setup(s => s.GetAllStatesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateLookupDto>());

            var countryMock = new Mock<ICountryLookup>(MockBehavior.Loose);
            countryMock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CountryLookupDto>)new List<CountryLookupDto>());
            countryMock.Setup(c => c.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryLookupDto>());

            return new DispatchAddressMappingQueryRepository(
                new SqlConnection(_fixture.ConnectionString), party.Object, freight.Object,
                cityMock.Object, stateMock.Object, countryMock.Object);
        }

        private async Task<int> EnsureMiscIdAsync(string code = "DAMQ_USG")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DAMQ_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DAMQ_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> EnsureDispatchAddressAsync(string name = "DAMMQ_ADDR")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.DispatchAddressMaster.FirstOrDefaultAsync(x => x.DispatchAddressName == name);
            if (existing != null) return existing.Id;
            var d = new SalesManagement.Domain.Entities.DispatchAddressMaster
            {
                DispatchAddressName = name,
                AddressLine1 = "L1",
                CityId = 1, StateId = 1, CountryId = 1,
                PinCode = "560001",
                FreightId = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.DispatchAddressMaster.AddAsync(d);
            await ctx.SaveChangesAsync();
            return d.Id;
        }

        private async Task<int> SeedAsync(
            int partyId, bool isDefault = false,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var addrId = await EnsureDispatchAddressAsync();
            var usageId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = new SalesManagement.Domain.Entities.DispatchAddressMapping
            {
                PartyId = partyId,
                DispatchAddressId = addrId,
                UsageTypeId = usageId,
                IsDefault = isDefault,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.DispatchAddressMapping.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync(partyId: 1);

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_PartyId()
        {
            await ClearAsync();
            await SeedAsync(partyId: 1);
            await SeedAsync(partyId: 2);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null, partyId: 1);

            rows.Should().HaveCount(1);
            rows[0].PartyId.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync(partyId: 1, deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync(partyId: 5);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.PartyId.Should().Be(5);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync(partyId: 1);

            var result = await CreateRepo().AutocompleteAsync("DAMMQ", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync(partyId: 10);
            await using var ctx = _fixture.CreateFreshDbContext();
            var addrId = await ctx.DispatchAddressMaster.Where(d => d.DispatchAddressName == "DAMMQ_ADDR").Select(d => d.Id).FirstAsync();
            var usageId = await ctx.MiscMaster.Where(m => m.Code == "DAMQ_USG").Select(m => m.Id).FirstAsync();

            var result = await CreateRepo().CompositeKeyExistsAsync(10, addrId, usageId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync(partyId: 11);
            await using var ctx = _fixture.CreateFreshDbContext();
            var addrId = await ctx.DispatchAddressMaster.Where(d => d.DispatchAddressName == "DAMMQ_ADDR").Select(d => d.Id).FirstAsync();
            var usageId = await ctx.MiscMaster.Where(m => m.Code == "DAMQ_USG").Select(m => m.Id).FirstAsync();

            var result = await CreateRepo().CompositeKeyExistsAsync(11, addrId, usageId, excludeId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DispatchAddressExistsAsync_Should_Return_True_For_Active()
        {
            var addrId = await EnsureDispatchAddressAsync();

            var result = await CreateRepo().DispatchAddressExistsAsync(addrId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_For_Active()
        {
            var miscId = await EnsureMiscIdAsync();

            var result = await CreateRepo().MiscMasterExistsAsync(miscId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DefaultAlreadyExistsAsync_Should_Return_True_When_Default_Exists()
        {
            await ClearAsync();
            await SeedAsync(partyId: 20, isDefault: true);
            await using var ctx = _fixture.CreateFreshDbContext();
            var usageId = await ctx.MiscMaster.Where(m => m.Code == "DAMQ_USG").Select(m => m.Id).FirstAsync();

            var result = await CreateRepo().DefaultAlreadyExistsAsync(20, usageId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DefaultAlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync(partyId: 21, isDefault: true);
            await using var ctx = _fixture.CreateFreshDbContext();
            var usageId = await ctx.MiscMaster.Where(m => m.Code == "DAMQ_USG").Select(m => m.Id).FirstAsync();

            var result = await CreateRepo().DefaultAlreadyExistsAsync(21, usageId, excludeId: id);

            result.Should().BeFalse();
        }
    }
}
