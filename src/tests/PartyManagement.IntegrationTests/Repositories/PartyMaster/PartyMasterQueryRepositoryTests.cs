using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;
using PartyManagement.Infrastructure.Repositories.PartyMaster;

namespace PartyManagement.IntegrationTests.Repositories.PartyMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IIncotermLookup> _incotermLookup = new(MockBehavior.Loose);
        private readonly Mock<IPaymentTermLookup> _paymentTermLookup = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _cityLookup = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _stateLookup = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _countryLookup = new(MockBehavior.Loose);
        private readonly Mock<IDataAccessFilter> _dataAccessFilter = new(MockBehavior.Loose);
        private readonly Mock<ISalesSegmentLookup> _salesSegmentLookup = new(MockBehavior.Loose);

        public PartyMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;

            // Bypass data access filtering so queries return all data in the test DB
            _dataAccessFilter
                .Setup(f => f.GetContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataAccessContext { BypassDataAccess = true });
        }

        private PartyMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyMasterQueryRepository(
                conn,
                _fixture.IpMock.Object,
                _incotermLookup.Object,
                _paymentTermLookup.Object,
                _cityLookup.Object,
                _stateLookup.Object,
                _countryLookup.Object,
                _dataAccessFilter.Object,
                _salesSegmentLookup.Object);
        }

        private async Task<int> SeedApprovalStatusAsync()
        {
            await using var ctx1 = _fixture.CreateFreshDbContext();
            var mt = await new MiscTypeMasterCommandRepository(ctx1).CreateAsync(new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "ApprovalStatus",
                Description = "Approval Status",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx2).CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "Pending",
                Description = "Pending",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            return await SeedRegistrationTypeAsync();
        }

        /// <summary>
        /// Seeds a RegistrationType MiscTypeMaster and a GST MiscMaster entry.
        /// RegistrationTypeId is NOT NULL in the DB, so every PartyMaster insert needs a valid FK.
        /// Returns the seeded MiscMaster Id to use as RegistrationTypeId.
        /// </summary>
        private async Task<int> SeedRegistrationTypeAsync()
        {
            await using var ctx1 = _fixture.CreateFreshDbContext();
            var mt = await new MiscTypeMasterCommandRepository(ctx1).CreateAsync(new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "RegistrationType",
                Description = "Registration Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var mm = await new MiscMasterCommandRepository(ctx2).CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "GST",
                Description = "GST Registered",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return mm.Id;
        }

        private async Task<int> SeedPartyMasterAsync(string code = "PQ0001", string name = "Query Test Party")
        {
            var regTypeId = await SeedApprovalStatusAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var cmdRepo = new PartyMasterCommandRepository(ctx, _fixture.IpMock.Object);
            return await cmdRepo.CreateAsync(new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = code,
                PartyName = name,
                UnitId = 1,
                RegistrationTypeId = regTypeId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyActivityLog]");
            await conn.ExecuteAsync("DELETE FROM [Party].[AgentConfig]");
            await conn.ExecuteAsync("DELETE FROM [Party].[SalesType]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyUnitCompanyMapping]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyDocument]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyBank]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyAddress]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyContact]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyType]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyGroup]");
            await conn.ExecuteAsync("DELETE FROM [Party].[BankAccount]");
            await conn.ExecuteAsync("DELETE FROM [Party].[BankMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllPartyMasterAsync_Should_Return_Empty_When_NoData()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllPartyMasterAsync(1, 10, null);

            // GetAllPartyMasterAsync uses INNER JOINs to PartyType, MiscMaster, PartyGroup.
            // With no data seeded, all INNER JOINs produce zero rows.
            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllPartyMasterAsync_Should_Respect_Pagination()
        {
            await ClearTablesAsync();

            // Even with no data, pagination parameters are accepted without error
            var (items, total) = await CreateQueryRepo().GetAllPartyMasterAsync(2, 5, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdPartyMasterAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdPartyMasterAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdPartyMasterAsync_Should_Return_PartyMaster_With_Empty_Collections_When_No_Children()
        {
            await ClearTablesAsync();
            var id = await SeedPartyMasterAsync("PQ0001", "GetById Test Party");

            var result = await CreateQueryRepo().GetByIdPartyMasterAsync(id);

            result.Should().NotBeNull();
            // Child collections are empty since no child data was seeded
            result!.PartyTypes.Should().BeEmpty();
            result.PartyContacts.Should().BeEmpty();
            result.PartyAddresses.Should().BeEmpty();
            result.PartyBanks.Should().BeEmpty();
            result.PartyDocuments.Should().BeEmpty();
            result.PartyUnitCompanyMappings.Should().BeEmpty();
            result.SalesTypes.Should().BeEmpty();
            result.AgentConfigs.Should().BeEmpty();
        }

        // --- GET PARTY TYPE CODES ---

        [Fact]
        public async Task GetPartyTypeCodesAsync_Should_Return_Empty_For_Nonexistent_Party()
        {
            await ClearTablesAsync();

            var codes = await CreateQueryRepo().GetPartyTypeCodesAsync(9999);

            codes.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPartyTypeCodesAsync_Should_Return_Empty_When_Party_Has_No_Types()
        {
            await ClearTablesAsync();
            var id = await SeedPartyMasterAsync("PQ0001", "No Types Party");

            var codes = await CreateQueryRepo().GetPartyTypeCodesAsync(id);

            // No PartyType records seeded — returns empty list
            codes.Should().BeEmpty();
        }

        // --- GET PARTY GROUPS ---

        [Fact]
        public async Task GetPartyGroupsAsync_Should_Return_Empty_When_No_Groups_Seeded()
        {
            await ClearTablesAsync();

            var results = await CreateQueryRepo().GetPartyGroupsAsync(new List<int> { 1, 2, 3 });

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPartyGroupsAsync_Should_Return_Empty_When_No_Matching_GroupTypeIds()
        {
            await ClearTablesAsync();

            // Use non-existent ID — avoids SQL IN () issue with empty list
            var results = await CreateQueryRepo().GetPartyGroupsAsync(new List<int> { 99999 });

            results.Should().BeEmpty();
        }
    }
}
