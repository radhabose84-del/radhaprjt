using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Repositories.TripSheet;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.TripSheet
{
    [Collection("DatabaseCollection")]
    public sealed class TripSheetQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public TripSheetQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private TripSheetQueryRepository CreateRepo()
        {
            var unit = new Mock<IUnitLookup>(MockBehavior.Loose);
            unit.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>());
            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            return new TripSheetQueryRepository(new SqlConnection(_fixture.ConnectionString), unit.Object, party.Object);
        }

        private async Task<int> SeedAsync(string tripSheetNo, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new SalesManagement.Domain.Entities.TripSheetHeader
            {
                TripSheetNo = tripSheetNo,
                TripDate = new DateOnly(2026, 1, 15),
                VehicleNo = "KA01AB1234",
                UnitId = 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.TripSheetHeader.AddAsync(h);
            await ctx.SaveChangesAsync();

            // IsActive default-value gotcha: Status.Inactive is CLR default → force via follow-up update.
            if (active == Status.Inactive)
            {
                h.IsActive = Status.Inactive;
                await ctx.SaveChangesAsync();
            }
            return h.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("TSQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("TSQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("TSQ_UNIQ");
            await SeedAsync("TSQ_OTHER");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "TSQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].TripSheetNo.Should().Be("TSQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("TSQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.TripSheetNo.Should().Be("TSQ_GBI");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("TSQ_AC1");
            await SeedAsync("TSQ_AC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("TSQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].TripSheetNo.Should().Be("TSQ_AC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("TSQ_DUP");

            var result = await CreateRepo().AlreadyExistsAsync("TSQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("TSQ_SELF");

            var result = await CreateRepo().AlreadyExistsAsync("TSQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DispatchExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().DispatchExistsAsync(9999999);
            result.Should().BeFalse();
        }
    }
}
