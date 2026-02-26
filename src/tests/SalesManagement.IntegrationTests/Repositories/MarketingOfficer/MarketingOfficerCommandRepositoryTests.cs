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
    /// Integration tests for MarketingOfficerCommandRepository.
    /// Verifies EF Core Create (with children), Update (full replacement), and SoftDelete (cascading).
    /// Requires seeded SalesOrganisation → SalesOffice → SalesGroup prerequisite chain.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MarketingOfficerCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MarketingOfficerCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MarketingOfficerCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        // ── Prerequisite FK Seeding ─────────────────────────────────────────

        private async Task<int> EnsureSalesOfficeAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.IsDeleted == IsDelete.NotDeleted);
            if (existing != null)
                return existing.Id;

            var org = new Domain.Entities.SalesOrganisation
            {
                SalesOrganisationCode = "INTORG01",
                SalesOrganisationName = "Integration Org",
                CompanyId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.SalesOrganisation.AddAsync(org);
            await ctx.SaveChangesAsync();

            var office = new Domain.Entities.SalesOffice
            {
                SalesOfficeName = "Integration Office",
                SalesOrganisationId = org.Id,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.SalesOffice.AddAsync(office);
            await ctx.SaveChangesAsync();

            return office.Id;
        }

        private async Task<int> EnsureSalesGroupAsync(ApplicationDbContext ctx, int salesOfficeId)
        {
            var existing = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.IsDeleted == IsDelete.NotDeleted);
            if (existing != null)
                return existing.Id;

            var group = new Domain.Entities.SalesGroup
            {
                SalesGroupName = "Integration Group",
                SalesOfficeId = salesOfficeId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.SalesGroup.AddAsync(group);
            await ctx.SaveChangesAsync();

            return group.Id;
        }

        private Domain.Entities.MarketingOfficer BuildEntity(
            string employeeNo,
            string employeeName,
            int salesOfficeId,
            List<Domain.Entities.OfficerSalesGroup> children = null) =>
            new()
            {
                EmployeeNo = employeeNo,
                EmployeeName = employeeName,
                MobileNo = "9876543210",
                Email = "test@example.com",
                Unit = "Unit A",
                Department = "Sales Dept",
                Designation = "Manager",
                SalesOfficeId = salesOfficeId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                OfficerSalesGroups = children ?? new List<Domain.Entities.OfficerSalesGroup>()
            };

        private async Task ClearMarketingOfficerTablesAsync()
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.OfficerSalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.MarketingOfficer");
        }

        // ── CreateAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            await ClearMarketingOfficerTablesAsync();

            var entity = BuildEntity("CMD001", "Create Test", officeId);
            var newId = await CreateRepository(ctx).CreateAsync(entity);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_ParentFields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            await ClearMarketingOfficerTablesAsync();

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("CMD002", "Alpha Officer", officeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.EmployeeNo.Should().Be("CMD002");
            saved.EmployeeName.Should().Be("Alpha Officer");
            saved.Unit.Should().Be("Unit A");
            saved.Department.Should().Be("Sales Dept");
            saved.Designation.Should().Be("Manager");
            saved.SalesOfficeId.Should().Be(officeId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Children_ViaGraphPersistence()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            var groupId = await EnsureSalesGroupAsync(ctx, officeId);
            await ClearMarketingOfficerTablesAsync();

            var children = new List<Domain.Entities.OfficerSalesGroup>
            {
                new()
                {
                    SalesGroupId = groupId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }
            };

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("CMD003", "Graph Officer", officeId, children));
            ctx.ChangeTracker.Clear();

            var savedChildren = await ctx.OfficerSalesGroup
                .Where(x => x.MarketingOfficerId == newId)
                .ToListAsync();

            savedChildren.Should().HaveCount(1);
            savedChildren[0].SalesGroupId.Should().Be(groupId);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_AuditFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            await ClearMarketingOfficerTablesAsync();

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("CMD004", "Audit Officer", officeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_MutableFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            await ClearMarketingOfficerTablesAsync();

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity("UPD001", "Original Name", officeId));
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.MarketingOfficer
            {
                Id = id,
                EmployeeName = "Updated Name",
                MobileNo = "9111222333",
                Email = "updated@example.com",
                Unit = "Unit B",
                Department = "Marketing Dept",
                Designation = "Director",
                SalesOfficeId = officeId,
                IsActive = Status.Inactive,
                OfficerSalesGroups = new List<Domain.Entities.OfficerSalesGroup>()
            };

            var result = await CreateRepository(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.Id == id);
            saved!.EmployeeName.Should().Be("Updated Name");
            saved.MobileNo.Should().Be("9111222333");
            saved.Email.Should().Be("updated@example.com");
            saved.Unit.Should().Be("Unit B");
            saved.Department.Should().Be("Marketing Dept");
            saved.Designation.Should().Be("Director");
            saved.IsActive.Should().Be(Status.Inactive);
            result.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_FullReplace_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            var groupId = await EnsureSalesGroupAsync(ctx, officeId);
            await ClearMarketingOfficerTablesAsync();

            // Create with one child
            var children = new List<Domain.Entities.OfficerSalesGroup>
            {
                new() { SalesGroupId = groupId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity("UPD002", "Child Test", officeId, children));
            ctx.ChangeTracker.Clear();

            // Update with no children (full replacement removes old)
            var updateEntity = new Domain.Entities.MarketingOfficer
            {
                Id = id,
                EmployeeName = "Child Test",
                Unit = "U", Department = "D", Designation = "Mgr",
                SalesOfficeId = officeId,
                IsActive = Status.Active,
                OfficerSalesGroups = new List<Domain.Entities.OfficerSalesGroup>()
            };

            await CreateRepository(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            var remaining = await ctx.OfficerSalesGroup
                .Where(x => x.MarketingOfficerId == id)
                .ToListAsync();

            remaining.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var updateEntity = new Domain.Entities.MarketingOfficer
            {
                Id = 99999,
                EmployeeName = "Ghost",
                Unit = "U", Department = "D", Designation = "Mgr",
                SalesOfficeId = 1,
                IsActive = Status.Active,
                OfficerSalesGroups = new List<Domain.Entities.OfficerSalesGroup>()
            };

            var result = await CreateRepository(ctx).UpdateAsync(updateEntity);

            result.Should().Be(0);
        }

        // ── SoftDeleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            await ClearMarketingOfficerTablesAsync();

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity("DEL001", "Delete Test", officeId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_OnParentAndChildren()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            var groupId = await EnsureSalesGroupAsync(ctx, officeId);
            await ClearMarketingOfficerTablesAsync();

            var children = new List<Domain.Entities.OfficerSalesGroup>
            {
                new() { SalesGroupId = groupId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity("DEL002", "Cascade Delete", officeId, children));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var parent = await ctx.MarketingOfficer
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
            parent!.IsDeleted.Should().Be(IsDelete.Deleted);

            var childRecords = await ctx.OfficerSalesGroup
                .IgnoreQueryFilters()
                .Where(x => x.MarketingOfficerId == id)
                .ToListAsync();
            childRecords.Should().AllSatisfy(c => c.IsDeleted.Should().Be(IsDelete.Deleted));
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var officeId = await EnsureSalesOfficeAsync(ctx);
            await ClearMarketingOfficerTablesAsync();

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity("DEL003", "Double Delete", officeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
