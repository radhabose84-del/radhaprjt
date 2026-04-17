using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOffice;
using SalesManagement.Infrastructure.Repositories.SalesOrganisation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOffice
{
    /// <summary>
    /// Integration tests for SalesOfficeCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    /// SalesOffice has a same-module FK to SalesOrganisation — a parent org is seeded before each test.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesOfficeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesOfficeCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesOfficeCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesOfficeCommandRepository(ctx);

        private Domain.Entities.SalesOffice BuildEntity(
            int salesOrganisationId,
            string name = "Test Sales Office",
            int cityId = 1,
            string pincode = "110001",
            string phone = "9876543210",
            string email = "office@test.com",
            string responsibleManager = "Manager A",
            string regionTerritory = "North",
            string address = "123 Test Street")
            => new Domain.Entities.SalesOffice
            {
                SalesOfficeName = name,
                SalesOrganisationId = salesOrganisationId,
                CityId = cityId,
                Pincode = pincode,
                Phone = phone,
                Email = email,
                ResponsibleManager = responsibleManager,
                RegionTerritory = regionTerritory,
                Address = address,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync() =>
            await _fixture.ClearTablesAsync("Sales.SalesOffice");

        private async Task<int> SeedSalesOrganisationAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var orgRepo = new SalesOrganisationCommandRepository(ctx);
            return await orgRepo.CreateAsync(new Domain.Entities.SalesOrganisation
            {
                SalesOrganisationCode = "ORG" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                SalesOrganisationName = "Test Org",
                CompanyId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var newId = await repo.CreateAsync(BuildEntity(orgId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_Correct_Fields()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var entity = BuildEntity(orgId, name: "Alpha Office", cityId: 5, pincode: "400001",
                phone: "9999999999", email: "alpha@test.com", responsibleManager: "Mgr Alpha",
                regionTerritory: "West", address: "Alpha Street 1");

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.SalesOfficeName.Should().Be("Alpha Office");
            saved.SalesOrganisationId.Should().Be(orgId);
            saved.CityId.Should().Be(5);
            saved.Pincode.Should().Be("400001");
            saved.Phone.Should().Be("9999999999");
            saved.Email.Should().Be("alpha@test.com");
            saved.ResponsibleManager.Should().Be("Mgr Alpha");
            saved.RegionTerritory.Should().Be("West");
            saved.Address.Should().Be("Alpha Street 1");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var newId = await repo.CreateAsync(BuildEntity(orgId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Update_Mutable_Fields()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId, name: "Original Office"));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesOffice
            {
                Id = id,
                SalesOfficeName = "Updated Office",
                SalesOrganisationId = orgId,
                CityId = 10,
                Pincode = "500001",
                Phone = "8888888888",
                Email = "updated@test.com",
                ResponsibleManager = "Mgr Updated",
                RegionTerritory = "South",
                Address = "Updated Street",
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.Id == id);
            saved!.SalesOfficeName.Should().Be("Updated Office");
            saved.CityId.Should().Be(10);
            saved.Pincode.Should().Be("500001");
            saved.Phone.Should().Be("8888888888");
            saved.Email.Should().Be("updated@test.com");
            saved.ResponsibleManager.Should().Be("Mgr Updated");
            saved.RegionTerritory.Should().Be("South");
            saved.Address.Should().Be("Updated Street");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var updated = new Domain.Entities.SalesOffice
            {
                Id = 99999,
                SalesOfficeName = "Ghost Office"
            };

            var resultId = await repo.UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesOffice
            {
                Id = id,
                SalesOfficeName = "Updated Office",
                SalesOrganisationId = orgId,
                CityId = 1,
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.Id == id);

            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOffice
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var result = await repo.SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
