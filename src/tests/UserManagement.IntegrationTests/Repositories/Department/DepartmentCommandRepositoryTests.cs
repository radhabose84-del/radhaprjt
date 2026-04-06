using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Departments;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Department
{
    [Collection("DatabaseCollection")]
    public sealed class DepartmentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepartmentCommandRepositoryTests(DbFixture fixture)
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

        private DepartmentCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new DepartmentCommandRepository(ctx);

        private async Task<int> EnsureDepartmentGroupAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.DepartmentGroup
                .FirstOrDefaultAsync(dg => dg.DepartmentGroupCode == "TESTGRP");

            if (existing != null)
                return existing.Id;

            var group = new UserManagement.Domain.Entities.DepartmentGroup
            {
                DepartmentGroupCode = "TESTGRP",
                DepartmentGroupName = "Test Group",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.DepartmentGroup.AddAsync(group);
            await ctx.SaveChangesAsync();
            return group.Id;
        }

        private UserManagement.Domain.Entities.Department BuildDepartment(
            int departmentGroupId,
            string shortName = "TST",
            string deptName = "Test Department",
            int companyId = 1)
        {
            return new UserManagement.Domain.Entities.Department
            {
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = companyId,
                DepartmentGroupId = departmentGroupId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
        }

        /// <summary>
        /// Safe cleanup: soft-delete our test data instead of physical DELETE.
        /// Physical DELETE would conflict with FK_Users_Department_DepartmentId
        /// from Users seeded by other integration tests in the same collection.
        /// </summary>
        private async Task ClearTestDepartmentsAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE AppData.Department SET IsDeleted = 1 WHERE ShortName LIKE 'TST%' OR ShortName LIKE 'UPD%'");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var repo = CreateRepository(ctx);
            var dept = BuildDepartment(groupId);

            var result = await repo.CreateAsync(dept);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var repo = CreateRepository(ctx);
            var dept = BuildDepartment(groupId, "ENG", "Engineering", 1);

            var result = await repo.CreateAsync(dept);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Department.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.ShortName.Should().Be("ENG");
            saved.DeptName.Should().Be("Engineering");
            saved.CompanyId.Should().Be(1);
            saved.DepartmentGroupId.Should().Be(groupId);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var repo = CreateRepository(ctx);
            var dept = BuildDepartment(groupId);

            var result = await repo.CreateAsync(dept);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Department.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
            saved.CreatedByName.Should().NotBeNullOrEmpty();
            saved.CreatedIP.Should().NotBeNullOrEmpty();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var repo = CreateRepository(ctx);
            var dept = BuildDepartment(groupId, "OLD", "Old Name");
            var created = await repo.CreateAsync(dept);
            ctx.ChangeTracker.Clear();

            var updateDept = new UserManagement.Domain.Entities.Department
            {
                ShortName = "NEW",
                DeptName = "New Name",
                CompanyId = 1,
                DepartmentGroupId = groupId,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(created.Id, updateDept);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.Department.FirstOrDefaultAsync(x => x.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.ShortName.Should().Be("NEW");
            updated.DeptName.Should().Be("New Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var updateDept = new UserManagement.Domain.Entities.Department
            {
                ShortName = "X",
                DeptName = "X",
                CompanyId = 1,
                DepartmentGroupId = 1,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(99999, updateDept);

            result.Should().Be(0);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Department()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var repo = CreateRepository(ctx);
            var dept = BuildDepartment(groupId);
            var created = await repo.CreateAsync(dept);
            ctx.ChangeTracker.Clear();

            var deleteModel = new UserManagement.Domain.Entities.Department
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.Department.FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var deleteModel = new UserManagement.Domain.Entities.Department
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(99999, deleteModel);

            result.Should().Be(0);
        }

        // --- ExistsByCodeAsync ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_For_Existing_DeptName()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDepartmentsAsync(ctx);
            var groupId = await EnsureDepartmentGroupAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildDepartment(groupId, "HR", "Human Resources"));

            var exists = await repo.ExistsByCodeAsync("Human Resources");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_For_NonExisting()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var exists = await repo.ExistsByCodeAsync("NonExistentDeptName_XYZ");

            exists.Should().BeFalse();
        }
    }
}
