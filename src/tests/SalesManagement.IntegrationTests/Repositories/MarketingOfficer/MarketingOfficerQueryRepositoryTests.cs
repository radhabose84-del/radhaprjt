using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MarketingOfficer;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.MarketingOfficer
{
    /// <summary>
    /// Integration tests for MarketingOfficerQueryRepository.
    /// Verifies Dapper SQL queries against a real SQL Server database.
    /// MarketingOfficer uses same-module JOINs to SalesOffice and SalesGroup — no cross-module lookups.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MarketingOfficerQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MarketingOfficerQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private MarketingOfficerQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MarketingOfficerQueryRepository(conn);
        }

        private MarketingOfficerCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

        private int _salesOfficeId;
        private int _salesGroupId;

        private async Task EnsurePrerequisitesAsync()
        {
            if (_salesOfficeId > 0 && _salesGroupId > 0) return;

            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.IsDeleted == IsDelete.NotDeleted);
            if (org == null)
            {
                org = new Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "QORG01",
                    SalesOrganisationName = "Query Org",
                    CompanyId = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.IsDeleted == IsDelete.NotDeleted);
            if (office == null)
            {
                office = new Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "Query Office",
                    SalesOrganisationId = org.Id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            _salesOfficeId = office.Id;

            var group = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.IsDeleted == IsDelete.NotDeleted);
            if (group == null)
            {
                group = new Domain.Entities.SalesGroup
                {
                    SalesGroupName = "Query Group",
                    SalesOfficeId = office.Id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(group);
                await ctx.SaveChangesAsync();
            }
            _salesGroupId = group.Id;
        }

        private async Task ClearMarketingOfficerTablesAsync()
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.OfficerSalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.MarketingOfficer");
        }

        private async Task<int> SeedEntityAsync(
            string employeeNo = "QRY001",
            string employeeName = "Query Officer",
            bool isActive = true,
            bool withChild = false)
        {
            await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = new Domain.Entities.MarketingOfficer
            {
                EmployeeNo = employeeNo,
                EmployeeName = employeeName,
                MobileNo = "9876543210",
                Email = "query@example.com",
                Unit = "Unit Q",
                Department = "Dept Q",
                Designation = "Analyst",
                SalesOfficeId = _salesOfficeId,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted,
                OfficerSalesGroups = withChild
                    ? new List<Domain.Entities.OfficerSalesGroup>
                    {
                        new()
                        {
                            SalesGroupId = _salesGroupId,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        }
                    }
                    : new List<Domain.Entities.OfficerSalesGroup>()
            };

            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("QGA001", "Alpha Officer");
            await SeedEntityAsync("QGA002", "Beta Officer");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            totalCount.Should().Be(2);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_SalesOfficeName()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("QGA003", "Lookup Officer");

            var (data, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].SalesOfficeName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("QGA004", "Deleted Officer");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("QGA005", "Alpha Manager");
            await SeedEntityAsync("QGA006", "Beta Manager");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            totalCount.Should().Be(1);
            data[0].EmployeeName.Should().Be("Alpha Manager");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_EmployeeNo()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("UNIQUE99", "Searchable Officer");
            await SeedEntityAsync("QGA007", "Other Officer");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "UNIQUE99");

            totalCount.Should().Be(1);
            data[0].EmployeeNo.Should().Be("UNIQUE99");
        }

        // ── GetByIdAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("QBI001", "ById Officer");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.EmployeeNo.Should().Be("QBI001");
            dto.EmployeeName.Should().Be("ById Officer");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Include_ChildSalesGroups()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("QBI002", "WithChild Officer", withChild: true);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.SalesGroups.Should().NotBeNull();
            dto.SalesGroups.Should().HaveCount(1);
            dto.SalesGroups[0].SalesGroupId.Should().Be(_salesGroupId);
            dto.SalesGroups[0].SalesGroupName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearMarketingOfficerTablesAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("QBI003", "Soft Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── AlreadyExistsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenDuplicate()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("EXIST01", "Existing Officer");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("EXIST01");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForSoftDeleted()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("EXIST02", "Deleted Officer");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("EXIST02");

            exists.Should().BeFalse();
        }

        // ── NotFoundAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("QNF001", "Found Officer");

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenNotExists()
        {
            await ClearMarketingOfficerTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("QNF002", "Soft Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── SalesOfficeExistsAsync ──────────────────────────────────────────

        [Fact]
        public async Task SalesOfficeExistsAsync_Should_Return_True_WhenExists()
        {
            await EnsurePrerequisitesAsync();

            var result = await CreateQueryRepo().SalesOfficeExistsAsync(_salesOfficeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesOfficeExistsAsync_Should_Return_False_WhenNotExists()
        {
            var result = await CreateQueryRepo().SalesOfficeExistsAsync(99999);

            result.Should().BeFalse();
        }

        // ── SalesGroupsAllExistAsync ────────────────────────────────────────

        [Fact]
        public async Task SalesGroupsAllExistAsync_Should_Return_True_WhenAllExist()
        {
            await EnsurePrerequisitesAsync();

            var result = await CreateQueryRepo().SalesGroupsAllExistAsync(new List<int> { _salesGroupId });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesGroupsAllExistAsync_Should_Return_False_WhenSomeMissing()
        {
            await EnsurePrerequisitesAsync();

            var result = await CreateQueryRepo().SalesGroupsAllExistAsync(new List<int> { _salesGroupId, 99999 });

            result.Should().BeFalse();
        }

        // ── AutocompleteAsync ───────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("AUTO01", "Acme Officer");
            await SeedEntityAsync("AUTO02", "Acme Manager");
            await SeedEntityAsync("AUTO03", "Zeta Officer");

            var results = await CreateQueryRepo().AutocompleteAsync("Acme", CancellationToken.None);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("AUTO04", "Active Person", isActive: true);
            await SeedEntityAsync("AUTO05", "Inactive Person", isActive: false);

            var results = await CreateQueryRepo().AutocompleteAsync("Person", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].EmployeeName.Should().Be("Active Person");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted()
        {
            await ClearMarketingOfficerTablesAsync();
            var id = await SeedEntityAsync("AUTO06", "Deleted Person");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearMarketingOfficerTablesAsync();
            await SeedEntityAsync("AUTO07", "Some Officer");

            var results = await CreateQueryRepo().AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
