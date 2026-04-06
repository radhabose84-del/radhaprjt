using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.SalesLead;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesLead
{
    [Collection("DatabaseCollection")]
    public sealed class SalesLeadQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesLeadQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesLeadQueryRepository CreateQueryRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<ICityLookup>? cityLookup = null,
            Mock<IItemLookup>? itemLookup = null)
        {
            partyLookup ??= BuildDefaultPartyLookup();
            cityLookup ??= BuildDefaultCityLookup();
            itemLookup ??= BuildDefaultItemLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesLeadQueryRepository(conn, partyLookup.Object, cityLookup.Object, itemLookup.Object);
        }

        private static Mock<IPartyLookup> BuildDefaultPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyLookupDto?)null);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>());
            return mock;
        }

        private static Mock<ICityLookup> BuildDefaultCityLookup()
        {
            var mock = new Mock<ICityLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CityLookupDto?)null);
            mock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityLookupDto>());
            return mock;
        }

        private static Mock<IItemLookup> BuildDefaultItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>());
            return mock;
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await Dapper.SqlMapper.ExecuteAsync(conn, "DELETE FROM Sales.SalesEnquiryDetail");
            await Dapper.SqlMapper.ExecuteAsync(conn, "DELETE FROM Sales.SalesEnquiryHeader");
            await Dapper.SqlMapper.ExecuteAsync(conn, "DELETE FROM Sales.SalesLead");
        }

        private async Task<int> EnsureMarketingOfficerAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var existingId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(conn,
                "SELECT TOP 1 Id FROM Sales.MarketingOfficer WHERE IsDeleted = 0 AND IsActive = 1");
            if (existingId > 0) return existingId;

            var salesOfficeId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(conn,
                "SELECT TOP 1 Id FROM Sales.SalesOffice WHERE IsDeleted = 0");
            if (salesOfficeId == 0)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                var so = new Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "Test Sales Office SLQ",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesOffice.Add(so);
                await ctx.SaveChangesAsync();
                salesOfficeId = so.Id;
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var mo = new Domain.Entities.MarketingOfficer
            {
                EmployeeNo = "EMP_SLQ01",
                EmployeeName = "Test Officer SLQ",
                MobileNo = "9876543211",
                SalesOfficeId = salesOfficeId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx2.MarketingOfficer.Add(mo);
            await ctx2.SaveChangesAsync();
            return mo.Id;
        }

        private async Task<int> SeedLeadAsync(
            string contactName = "Query Lead",
            string mobileNumber = "9000000001",
            int? marketingOfficerId = null)
        {
            var moId = marketingOfficerId ?? await EnsureMarketingOfficerAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.SalesLead
            {
                PartyId = null,
                ProspectCompanyName = "Test Corp",
                CityId = null,
                ContactName = contactName,
                MobileNumber = mobileNumber,
                EmailId = "query@test.com",
                ContactId = null,
                ItemId = null,
                RequirementQty = 50m,
                ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Remarks = "Query test",
                LeadSourceId = null,
                MarketingOfficerId = moId,
                InteractionDate = DateTimeOffset.UtcNow,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.SalesLead.Add(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_SeededRecord()
        {
            await ClearTableAsync();
            await SeedLeadAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_CorrectContactName()
        {
            await ClearTableAsync();
            await SeedLeadAsync(contactName: "Unique Query Person");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].ContactName.Should().Be("Unique Query Person");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new SalesLeadCommandRepository(ctx);
            await repo.SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm_ContactName()
        {
            await ClearTableAsync();
            await SeedLeadAsync(contactName: "Alpha Contact", mobileNumber: "9100000001");
            await SeedLeadAsync(contactName: "Beta Contact", mobileNumber: "9100000002");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].ContactName.Should().Be("Alpha Contact");
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm_MobileNumber()
        {
            await ClearTableAsync();
            await SeedLeadAsync(contactName: "Mobile Search Lead", mobileNumber: "9199999999");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "9199999999");

            items.Should().HaveCount(1);
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
            var moId = await EnsureMarketingOfficerAsync();
            for (int i = 1; i <= 5; i++)
                await SeedLeadAsync($"Lead {i}", $"900000{i:D4}", moId);

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);

            page1.Should().HaveCount(3);
            total.Should().Be(5);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_CorrectRecord()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync(contactName: "GetById Lead", mobileNumber: "9200000001");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.ContactName.Should().Be("GetById Lead");
            dto.MobileNumber.Should().Be("9200000001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesLeadCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_ActiveRecords()
        {
            await ClearTableAsync();
            await SeedLeadAsync(contactName: "Active Lead", mobileNumber: "9300000001");

            var results = await CreateQueryRepo().AutocompleteAsync("Active", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].ContactName.Should().Be("Active Lead");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync(contactName: "Deleted Lead", mobileNumber: "9300000002");
            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesLeadCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync(contactName: "Inactive Lead", mobileNumber: "9300000003");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.SalesLead.FindAsync(id);
            entity!.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Inactive", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Match_ByMobileNumber()
        {
            await ClearTableAsync();
            await SeedLeadAsync(contactName: "Mobile Match", mobileNumber: "9312345678");

            var results = await CreateQueryRepo().AutocompleteAsync("9312345678", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        // ── MobileNumberExistsForProspectAsync ────────────────────────────────

        [Fact]
        public async Task MobileNumberExistsForProspectAsync_Should_Return_True_WhenExists()
        {
            await ClearTableAsync();
            await SeedLeadAsync(mobileNumber: "9400000001");

            var exists = await CreateQueryRepo().MobileNumberExistsForProspectAsync("9400000001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MobileNumberExistsForProspectAsync_Should_Return_False_WhenNotExists()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().MobileNumberExistsForProspectAsync("9400000099");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task MobileNumberExistsForProspectAsync_Should_Return_False_ForSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync(mobileNumber: "9400000002");
            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesLeadCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().MobileNumberExistsForProspectAsync("9400000002");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task MobileNumberExistsForProspectAsync_Should_ExcludeId_WhenProvided()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync(mobileNumber: "9400000003");

            var exists = await CreateQueryRepo().MobileNumberExistsForProspectAsync("9400000003", excludeId: id);

            exists.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenRecordExists()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenRecordDoesNotExist()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedLeadAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesLeadCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeTrue();
        }

        // ── MarketingOfficerExistsAsync ───────────────────────────────────────

        [Fact]
        public async Task MarketingOfficerExistsAsync_Should_Return_True_WhenExists()
        {
            var moId = await EnsureMarketingOfficerAsync();

            var exists = await CreateQueryRepo().MarketingOfficerExistsAsync(moId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MarketingOfficerExistsAsync_Should_Return_False_WhenNotExists()
        {
            var exists = await CreateQueryRepo().MarketingOfficerExistsAsync(99999);

            exists.Should().BeFalse();
        }
    }
}
