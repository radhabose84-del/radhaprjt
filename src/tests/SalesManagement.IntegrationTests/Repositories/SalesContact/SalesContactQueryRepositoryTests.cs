using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesContact;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesContact
{
    [Collection("DatabaseCollection")]
    public sealed class SalesContactQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesContactQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesContactQueryRepository CreateQueryRepo(Mock<IPartyLookup>? partyLookup = null)
        {
            var mockParty = partyLookup ?? BuildEmptyPartyLookup();
            return new SalesContactQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                mockParty.Object);
        }

        private Mock<IPartyLookup> BuildEmptyPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Party.PartyLookupDto?)null);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto>());
            return mock;
        }

        private Mock<IPartyLookup> BuildPartyLookup(int partyId = 1, string partyName = "Test Party")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            var dto = new Contracts.Dtos.Lookups.Party.PartyLookupDto
            {
                Id = partyId,
                PartyCode = "P001",
                PartyName = partyName
            };
            mock.Setup(x => x.GetByIdAsync(partyId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto> { dto });
            return mock;
        }

        private SalesContactCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new SalesContactCommandRepository(ctx);

        private Domain.Entities.SalesContact BuildEntity(
            string name = "Test Contact",
            string mobile = "9876543210",
            int contactTypeId = 1,
            int? partyId = null,
            bool isActive = true)
            => new Domain.Entities.SalesContact
            {
                ContactName = name,
                MobileNumber = mobile,
                ContactTypeId = contactTypeId,
                PartyId = partyId,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.SalesLead");
            await conn.ExecuteAsync("DELETE FROM Sales.SalesContact");
        }

        private async Task<int> SeedAsync(Domain.Entities.SalesContact entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_SeededRecord()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("John Doe", "9876543210"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items[0].ContactName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("Deleted Contact", "9100000001"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().NotContain(x => x.ContactName == "Deleted Contact");
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnContactName()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("Alpha Contact", "9200000001"));
            await SeedAsync(BuildEntity("Beta Contact", "9200000002"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            total.Should().Be(1);
            items[0].ContactName.Should().Be("Alpha Contact");
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
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            for (int i = 0; i < 5; i++)
                await SeedAsync(BuildEntity($"Contact {i}", $"910000000{i}"));

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_PartyName_FromLookup()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("Contact With Party", "9300000001", partyId: 1));

            var mockParty = BuildPartyLookup(1, "Party Corp");
            var (items, _) = await CreateQueryRepo(mockParty).GetAllAsync(1, 10, null);

            items[0].PartyName.Should().Be("Party Corp");
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("ById Contact", "9400000001"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.ContactName.Should().Be("ById Contact");
            dto.MobileNumber.Should().Be("9400000001");
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
            var id = await SeedAsync(BuildEntity("SoftDel Contact", "9500000001"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── MobileAlreadyExistsAsync ─────────────────────────────────────────

        [Fact]
        public async Task MobileAlreadyExistsAsync_Should_Return_True_WhenExists()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("Contact A", "9600000001"));

            var result = await CreateQueryRepo().MobileAlreadyExistsAsync("9600000001");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MobileAlreadyExistsAsync_Should_Return_False_WhenNotExists()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().MobileAlreadyExistsAsync("9999999999");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task MobileAlreadyExistsAsync_Should_Return_False_ForSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("Deleted Mobile Contact", "9700000001"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().MobileAlreadyExistsAsync("9700000001");

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity());

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

        // ── AutocompleteAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_MatchingResults()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("Autocomplete Match", "9800000001", isActive: true));
            await SeedAsync(BuildEntity("Other Contact", "9800000002", isActive: true));

            var results = await CreateQueryRepo().AutocompleteAsync("Autocomplete", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].ContactName.Should().Be("Autocomplete Match");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("Active Contact", "9900000001", isActive: true));
            await SeedAsync(BuildEntity("Inactive Contact", "9900000002", isActive: false));

            var results = await CreateQueryRepo().AutocompleteAsync("Contact", CancellationToken.None);

            results.Should().NotContain(r => r.ContactName == "Inactive Contact");
            results.Should().Contain(r => r.ContactName == "Active Contact");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("Deleted Auto Contact", "9900000009", isActive: true));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted Auto", CancellationToken.None);

            results.Should().NotContain(r => r.ContactName == "Deleted Auto Contact");
        }
    }
}
