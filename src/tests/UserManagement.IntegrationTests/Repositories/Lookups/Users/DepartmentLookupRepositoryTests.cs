using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class DepartmentLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepartmentLookupRepositoryTests(DbFixture fixture)
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

        private DepartmentLookupRepository CreateLookupRepo(int companyId = 1)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(companyId);
            return new DepartmentLookupRepository(conn, ipMock.Object);
        }

        private async Task<int> EnsureDepartmentGroupAsync()
        {
            await using var ctx = CreateDbContext();
            var existing = await ctx.DepartmentGroup
                .FirstOrDefaultAsync(dg => dg.DepartmentGroupCode == "LKPGRP");

            if (existing != null)
                return existing.Id;

            var group = new UserManagement.Domain.Entities.DepartmentGroup
            {
                DepartmentGroupCode = "LKPGRP",
                DepartmentGroupName = "Lookup Group",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.DepartmentGroup.AddAsync(group);
            await ctx.SaveChangesAsync();
            return group.Id;
        }

        private async Task<int> SeedDepartmentAsync(
            int groupId,
            string shortName = "LKP",
            string deptName = "Lookup Dept",
            int companyId = 1)
        {
            await using var ctx = CreateDbContext();
            var dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = shortName,
                DeptName = deptName,
                CompanyId = companyId,
                DepartmentGroupId = groupId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Department.AddAsync(dept);
            await ctx.SaveChangesAsync();
            return dept.Id;
        }

        private async Task ClearTestDepartmentsAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllDepartmentAsync ---

        [Fact]
        public async Task GetAllDepartmentAsync_Should_Return_Seeded_Departments_For_Company()
        {
            await ClearTestDepartmentsAsync();
            var groupId = await EnsureDepartmentGroupAsync();
            await SeedDepartmentAsync(groupId, "LKP1", "Lookup D1", companyId: 1);
            await SeedDepartmentAsync(groupId, "LKP2", "Lookup D2", companyId: 1);

            var results = await CreateLookupRepo(companyId: 1).GetAllDepartmentAsync();

            results.Should().Contain(d => d.ShortName == "LKP1");
            results.Should().Contain(d => d.ShortName == "LKP2");
        }

        [Fact]
        public async Task GetAllDepartmentAsync_Should_Filter_By_CompanyId()
        {
            await ClearTestDepartmentsAsync();
            var groupId = await EnsureDepartmentGroupAsync();
            await SeedDepartmentAsync(groupId, "LKP1", "Lookup D1", companyId: 1);
            await SeedDepartmentAsync(groupId, "LKP2", "Lookup D2", companyId: 2);

            var results = await CreateLookupRepo(companyId: 1).GetAllDepartmentAsync();

            results.Should().Contain(d => d.ShortName == "LKP1");
            results.Should().NotContain(d => d.ShortName == "LKP2");
        }

        [Fact]
        public async Task GetAllDepartmentAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTestDepartmentsAsync();
            var groupId = await EnsureDepartmentGroupAsync();
            var id = await SeedDepartmentAsync(groupId, "LKPDEL", "Lookup Del");

            await using var ctx = CreateDbContext();
            var dept = await ctx.Department.FirstAsync(d => d.Id == id);
            dept.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllDepartmentAsync();

            results.Should().NotContain(d => d.ShortName == "LKPDEL");
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Department()
        {
            await ClearTestDepartmentsAsync();
            var groupId = await EnsureDepartmentGroupAsync();
            var id = await SeedDepartmentAsync(groupId, "LKPID", "ById Dept");

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DepartmentId.Should().Be(id);
            result.DepartmentName.Should().Be("ById Dept");
            result.ShortName.Should().Be("LKPID");
            result.Departmentgroupid.Should().Be(groupId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateLookupRepo().GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTestDepartmentsAsync();
            var groupId = await EnsureDepartmentGroupAsync();
            var id = await SeedDepartmentAsync(groupId, "LKPSD", "SoftDel Dept");

            await using var ctx = CreateDbContext();
            var dept = await ctx.Department.FirstAsync(d => d.Id == id);
            dept.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Departments()
        {
            await ClearTestDepartmentsAsync();
            var groupId = await EnsureDepartmentGroupAsync();
            var id1 = await SeedDepartmentAsync(groupId, "LKPA", "A");
            var id2 = await SeedDepartmentAsync(groupId, "LKPB", "B");
            await SeedDepartmentAsync(groupId, "LKPC", "C");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(Array.Empty<int>());

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTestDepartmentsAsync();
            var groupId = await EnsureDepartmentGroupAsync();
            var id1 = await SeedDepartmentAsync(groupId, "LKPX", "X");
            var id2 = await SeedDepartmentAsync(groupId, "LKPY", "Y");

            await using var ctx = CreateDbContext();
            var dept = await ctx.Department.FirstAsync(d => d.Id == id1);
            dept.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(1);
            results[0].DepartmentId.Should().Be(id2);
        }
    }
}
