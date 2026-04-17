using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAddressMaster
{
    [Collection("DatabaseCollection")]
    public sealed class DispatchAddressMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DispatchAddressMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DispatchAddressMasterQueryRepository CreateRepo(
            Mock<ICityLookup>? city = null,
            Mock<IStateLookup>? state = null,
            Mock<ICountryLookup>? country = null,
            Mock<IFreightMasterLookup>? freight = null)
        {
            if (city == null)
            {
                city = new Mock<ICityLookup>(MockBehavior.Loose);
                city.Setup(c => c.GetAllCityAsync(default))
                    .ReturnsAsync(new List<CityLookupDto>
                    {
                        new() { CityId = 1, CityName = "Bangalore" }
                    });
            }
            if (state == null)
            {
                state = new Mock<IStateLookup>(MockBehavior.Loose);
                state.Setup(s => s.GetAllStatesAsync(default))
                    .ReturnsAsync(new List<StateLookupDto>
                    {
                        new() { StateId = 1, StateName = "Karnataka" }
                    });
            }
            if (country == null)
            {
                country = new Mock<ICountryLookup>(MockBehavior.Loose);
                country.Setup(c => c.GetAllCountriesAsync(default))
                    .ReturnsAsync(new List<CountryLookupDto>
                    {
                        new() { CountryId = 1, CountryName = "India" }
                    });
            }
            if (freight == null)
            {
                freight = new Mock<IFreightMasterLookup>(MockBehavior.Loose);
                freight.Setup(f => f.GetAllFreightMasterAsync())
                    .ReturnsAsync(new List<Contracts.Dtos.Lookups.Logistics.FreightMasterLookupDto>());
            }

            return new DispatchAddressMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                city.Object, state.Object, country.Object, freight.Object);
        }

        private async Task<int> SeedAsync(
            string name, string pinCode = "560001", int cityId = 1,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var d = new SalesManagement.Domain.Entities.DispatchAddressMaster
            {
                DispatchAddressName = name,
                AddressLine1 = "L1",
                CityId = cityId, StateId = 1, CountryId = 1,
                PinCode = pinCode,
                FreightId = 1,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.DispatchAddressMaster.AddAsync(d);
            await ctx.SaveChangesAsync();
            return d.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("DQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("DQ_UNIQ");
            await SeedAsync("DQ_OTHER");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "DQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].DispatchAddressName.Should().Be("DQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DispatchAddressName.Should().Be("DQ_GBI");
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
            await SeedAsync("DQ_AC1");
            await SeedAsync("DQ_AC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("DQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("DQ_CK", "560002");

            var result = await CreateRepo().CompositeKeyExistsAsync("DQ_CK", 1, "560002");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_SELF", "560003");

            var result = await CreateRepo().CompositeKeyExistsAsync("DQ_SELF", 1, "560003", excludeId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CityExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().CityExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CityExistsAsync_Should_Return_False_When_NotIn_Lookup()
        {
            var result = await CreateRepo().CityExistsAsync(9999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task StateExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().StateExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CountryExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().CountryExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_SDV");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsDispatchAddressMasterLinkedAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("DQ_LK");

            var result = await CreateRepo().IsDispatchAddressMasterLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
