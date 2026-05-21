using Contracts.Interfaces;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    // Verifies DepartmentUserLookupRepository against a real SQL Server.
    // Mirrors sp_EvaluateApproval Block 4:
    //   ApprovalStepDetail → MiscMaster (TargetType.Code) → ApprovalStepDepartmentMapping
    //   → AppSecurity.Users (u.DepartmentId = dm.DepartmentId AND u.IsActive = 1)
    // The two AppData workflow tables are not in the UserManagement EF model,
    // so they are created with the minimal columns the query touches.
    [Collection("DatabaseCollection")]
    public sealed class DepartmentUserLookupRepositoryTests
    {
        private const string QcCode = "COMPLAINT_QC_REVIEWER_USER";
        private readonly DbFixture _fixture;

        public DepartmentUserLookupRepositoryTests(DbFixture fixture)
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

        private DepartmentUserLookupRepository CreateLookupRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task EnsureWorkflowTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                IF OBJECT_ID('AppData.ApprovalStepDetail') IS NULL
                    CREATE TABLE AppData.ApprovalStepDetail (Id INT NOT NULL, TargetTypeId INT NOT NULL);
                IF OBJECT_ID('AppData.ApprovalStepDepartmentMapping') IS NULL
                    CREATE TABLE AppData.ApprovalStepDepartmentMapping
                        (Id INT NOT NULL, ApprovalStepDetailId INT NOT NULL, DepartmentId INT NOT NULL);
                DELETE FROM AppData.ApprovalStepDepartmentMapping;
                DELETE FROM AppData.ApprovalStepDetail;");
        }

        private async Task<int> SeedDepartmentAsync(string shortName, string deptName)
        {
            await using var ctx = CreateDbContext();
            var grp = await ctx.DepartmentGroup.FirstOrDefaultAsync(g => g.DepartmentGroupCode == "QCGRP");
            if (grp == null)
            {
                grp = new UserManagement.Domain.Entities.DepartmentGroup
                {
                    DepartmentGroupCode = "QCGRP", DepartmentGroupName = "QC Group",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.DepartmentGroup.AddAsync(grp);
                await ctx.SaveChangesAsync();
            }

            var dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = shortName, DeptName = deptName, CompanyId = 1, DepartmentGroupId = grp.Id,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Department.AddAsync(dept);
            await ctx.SaveChangesAsync();
            return dept.Id;
        }

        private async Task<int> SeedMiscMasterAsync(string code)
        {
            await using var ctx = CreateDbContext();

            // MiscMaster.MiscTypeId is a real FK → ensure a MiscTypeMaster row first
            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "TARGETTYPE");
            if (miscType == null)
            {
                miscType = new UserManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "TARGETTYPE", Description = "Approval Target Type",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var mm = new UserManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id, Code = code, Description = code,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(mm);
            await ctx.SaveChangesAsync();
            return mm.Id;
        }

        private async Task<int> SeedUserAsync(
            string userName, int deptId, Enums.Status status = Enums.Status.Active)
        {
            await using var ctx = CreateDbContext();
            var user = new UserManagement.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "QC", LastName = userName, UserName = userName,
                EmailId = $"{userName}@test.com", Mobile = "9999999999",
                DepartmentId = deptId,
                IsActive = status, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.User.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user.UserId;
        }

        private async Task MapStepToDepartmentAsync(int targetTypeId, int departmentId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                INSERT INTO AppData.ApprovalStepDetail (Id, TargetTypeId) VALUES (1001, @TargetTypeId);
                INSERT INTO AppData.ApprovalStepDepartmentMapping (Id, ApprovalStepDetailId, DepartmentId)
                    VALUES (2001, 1001, @DepartmentId);",
                new { TargetTypeId = targetTypeId, DepartmentId = departmentId });
        }

        [Fact]
        public async Task Returns_Only_Active_Users_In_Mapped_QC_Department()
        {
            await _fixture.ClearAllTablesAsync();
            await EnsureWorkflowTablesAsync();

            var qcDeptId = await SeedDepartmentAsync("QCD", "Quality Control");
            var otherDeptId = await SeedDepartmentAsync("OTH", "Other Dept");
            var targetTypeId = await SeedMiscMasterAsync(QcCode);
            await MapStepToDepartmentAsync(targetTypeId, qcDeptId);

            var qcActive1 = await SeedUserAsync("qcactive1", qcDeptId);
            var qcActive2 = await SeedUserAsync("qcactive2", qcDeptId);
            await SeedUserAsync("qcinactive", qcDeptId, Enums.Status.Inactive);
            await SeedUserAsync("otherdept", otherDeptId);

            var result = await CreateLookupRepo()
                .GetActiveUserIdsByApprovalStepTargetTypeAsync(QcCode);

            result.Should().BeEquivalentTo(new[] { qcActive1, qcActive2 });
        }

        [Fact]
        public async Task Is_CaseInsensitive_And_Trims_TargetTypeCode()
        {
            await _fixture.ClearAllTablesAsync();
            await EnsureWorkflowTablesAsync();

            var qcDeptId = await SeedDepartmentAsync("QCD", "Quality Control");
            var targetTypeId = await SeedMiscMasterAsync(QcCode);
            await MapStepToDepartmentAsync(targetTypeId, qcDeptId);
            var qcUser = await SeedUserAsync("qcuser", qcDeptId);

            var result = await CreateLookupRepo()
                .GetActiveUserIdsByApprovalStepTargetTypeAsync("  complaint_qc_reviewer_user  ");

            result.Should().ContainSingle().Which.Should().Be(qcUser);
        }

        [Fact]
        public async Task Returns_Empty_When_No_Mapping_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await EnsureWorkflowTablesAsync();

            var qcDeptId = await SeedDepartmentAsync("QCD", "Quality Control");
            await SeedMiscMasterAsync(QcCode);
            await SeedUserAsync("qcuser", qcDeptId); // user exists but no step→dept mapping

            var result = await CreateLookupRepo()
                .GetActiveUserIdsByApprovalStepTargetTypeAsync(QcCode);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Returns_Empty_For_Blank_TargetTypeCode()
        {
            var result = await CreateLookupRepo()
                .GetActiveUserIdsByApprovalStepTargetTypeAsync("   ");

            result.Should().BeEmpty();
        }
    }
}
