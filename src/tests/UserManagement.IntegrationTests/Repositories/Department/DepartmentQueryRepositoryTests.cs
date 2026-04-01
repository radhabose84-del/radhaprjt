using Contracts.Interfaces;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Contracts.Interfaces.Validations.BudgetManagement;
using Contracts.Interfaces.Validations.ProjectManagement;
using Contracts.Interfaces.Validations.WarehouseManagement;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Departments;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Department
{
    [Collection("DatabaseCollection")]
    public sealed class DepartmentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepartmentQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private DepartmentQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");

            var maintenanceVal = new Mock<IMaintenanceDepartmentValidation>(MockBehavior.Loose);
            var budgetVal = new Mock<IBudgetDepartmentValidation>(MockBehavior.Loose);
            var projectVal = new Mock<IProjectDepartmentValidation>(MockBehavior.Loose);
            var warehouseVal = new Mock<IWarehouseDepartmentValidation>(MockBehavior.Loose);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DepartmentQueryRepository(conn, ipMock.Object, maintenanceVal.Object, budgetVal.Object, projectVal.Object, warehouseVal.Object);
        }

        private async Task<int> EnsureDepartmentGroupAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.DepartmentGroup
                .FirstOrDefaultAsync(dg => dg.DepartmentGroupCode == "QRYGRP");

            if (existing != null)
                return existing.Id;

            var group = new DepartmentGroup
            {
                DepartmentGroupCode = "QRYGRP",
                DepartmentGroupName = "Query Test Group",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.DepartmentGroup.AddAsync(group);
            await ctx.SaveChangesAsync();
            return group.Id;
        }

        private async Task<int> SeedDepartmentAsync(
            ApplicationDbContext ctx,
            int groupId,
            string shortName = "TST",
            string deptName = "Test Department",
            Enums.Status isActive = Enums.Status.Active)
        {
            var dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = 1,
                DepartmentGroupId = groupId,
                IsActive = isActive,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var repo = new DepartmentCommandRepository(ctx);
            var created = await repo.CreateAsync(dept);
            ctx.ChangeTracker.Clear();
            return created.Id;
        }

        /// <summary>
        /// Safe cleanup: soft-delete our test data instead of physical DELETE.
        /// Physical DELETE would conflict with FK_Users_Department_DepartmentId
        /// from Users seeded by other integration tests in the same collection.
        /// </summary>
        private async Task ClearTestDepartmentsAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE AppData.Department SET IsDeleted = 1 WHERE ShortName LIKE 'TST%' OR ShortName LIKE 'UPD%' OR ShortName LIKE 'A0%' OR ShortName LIKE 'DEL%' OR ShortName LIKE 'BID%' OR ShortName LIKE 'GRP%' OR ShortName LIKE 'ACT%' OR ShortName LIKE 'INA%'");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllDepartmentAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            await SeedDepartmentAsync(ctx, groupId, "A01", "Alpha Dept");
            await SeedDepartmentAsync(ctx, groupId, "B02", "Beta Dept");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllDepartmentAsync(1, 100, null);

            // Use >= instead of exact count — other tests in the collection may seed data
            items.Should().Contain(d => d.ShortName == "A01");
            items.Should().Contain(d => d.ShortName == "B02");
            total.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetAllDepartmentAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            await SeedDepartmentAsync(ctx, groupId, "A01", "Alpha Dept");
            await SeedDepartmentAsync(ctx, groupId, "B02", "Beta Dept");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllDepartmentAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].DeptName.Should().Be("Alpha Dept");
        }

        [Fact]
        public async Task GetAllDepartmentAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var id = await SeedDepartmentAsync(ctx, groupId, "DEL", "Deleted Dept");

            // Soft-delete via command repo
            await using var ctx2 = CreateDbContext();
            var cmdRepo = new DepartmentCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new UserManagement.Domain.Entities.Department
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllDepartmentAsync(1, 100, "Deleted Dept");

            // Soft-deleted record should not appear in filtered results
            items.Should().NotContain(d => d.ShortName == "DEL" && d.DeptName == "Deleted Dept");
        }

        [Fact]
        public async Task GetAllDepartmentAsync_Should_Include_DepartmentGroupName()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            await SeedDepartmentAsync(ctx, groupId, "GRP", "Group Test Dept");

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllDepartmentAsync(1, 100, "Group Test");

            items.Should().Contain(d => d.ShortName == "GRP");
            var grpDept = items.First(d => d.ShortName == "GRP");
            grpDept.DepartmentGroupName.Should().NotBeNullOrEmpty();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Department()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var id = await SeedDepartmentAsync(ctx, groupId, "BID", "ById Dept");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ShortName.Should().Be("BID");
            result.DeptName.Should().Be("ById Dept");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var id = await SeedDepartmentAsync(ctx, groupId, "DEL", "Deleted Dept");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new DepartmentCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new UserManagement.Domain.Entities.Department
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent_Id()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(99999);

            result.Should().BeNull();
        }

        // --- FK COLUMN EXIST VALIDATION ---

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_True_For_Active_Department()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var id = await SeedDepartmentAsync(ctx, groupId, "ACT", "Active Dept", Enums.Status.Active);

            var repo = CreateQueryRepo();
            var result = await repo.FKColumnExistValidation(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_For_Inactive_Department()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var id = await SeedDepartmentAsync(ctx, groupId, "INA", "Inactive Dept", Enums.Status.Inactive);

            var repo = CreateQueryRepo();
            var result = await repo.FKColumnExistValidation(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.FKColumnExistValidation(99999);

            result.Should().BeFalse();
        }
    }
}
