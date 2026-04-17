using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
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
    public sealed class UnitLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UnitLookupRepositoryTests(DbFixture fixture)
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

        private UnitLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UnitLookupRepository(conn);
        }

        private async Task<int> EnsureCompanyAsync()
        {
            await using var ctx = CreateDbContext();
            var existing = await ctx.Companies.FirstOrDefaultAsync(c => c.CompanyName == "UL Co" && c.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var company = new Company
            {
                CompanyName = "UL Co", LegalName = "UL Pvt", GstNumber = "GSTUL",
                TIN = "T", TAN = "T", CSTNo = "C", YearOfEstablishment = 2020,
                Website = "https://x.com", Logo = "l", EntityId = 1, PanNumber = "PANUL",
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted,
                CompanyAddress = new CompanyAddress { AddressLine1 = "X", AddressLine2 = "Y", PinCode = "600001", AlternatePhone = "1", Phone = "2", CountryId = 1, StateId = 1, CityId = 1 },
                CompanyContact = new CompanyContact { Name = "X", Designation = "X", Email = "x@x.com", Phone = "1", Remarks = "X" }
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();
            return company.Id;
        }

        private async Task<int> SeedDivisionAsync(int companyId)
        {
            await using var ctx = CreateDbContext();
            var div = new UserManagement.Domain.Entities.Division
            {
                ShortName = "UL", Name = "UL Div", CompanyId = companyId,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Divisions.AddAsync(div);
            await ctx.SaveChangesAsync();
            return div.Id;
        }

        private async Task<(int miscTypeId, int unitTypeId)> EnsureUnitTypeAsync()
        {
            await using var ctx = CreateDbContext();
            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "UNITTYPE");
            if (miscType == null)
            {
                miscType = new UserManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "UNITTYPE", Description = "Unit Type",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var miscMaster = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "PLANT");
            if (miscMaster == null)
            {
                miscMaster = new UserManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = "PLANT", Description = "Plant", SortOrder = 1,
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(miscMaster);
                await ctx.SaveChangesAsync();
            }
            return (miscType.Id, miscMaster.Id);
        }

        private async Task<int> SeedUnitAsync(int companyId, int divisionId, int unitTypeId, string unitName = "UL Unit")
        {
            await using var ctx = CreateDbContext();
            var unit = new UserManagement.Domain.Entities.Unit
            {
                UnitName = unitName, ShortName = "UL", CompanyId = companyId, DivisionId = divisionId,
                UnitHeadName = "Head", CINNO = "C", OldUnitId = "OLD001", IsMaintenanceStopStart = false,
                SpindlesCapacity = 100, UnitTypeId = unitTypeId,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted,
                UnitAddress = new UnitAddress
                { CountryId = 1, StateId = 1, CityId = 1, AddressLine1 = "A", AddressLine2 = "B", PinCode = 600001, ContactNumber = "1", AlternateNumber = "2" },
                UnitContacts = new UnitContacts { Name = "X", Designation = "X", Email = "x@x.com", PhoneNo = "1", Remarks = "X" }
            };
            await ctx.Unit.AddAsync(unit);
            await ctx.SaveChangesAsync();
            return unit.Id;
        }

        private async Task<int> EnsureDepartmentAsync()
        {
            await using var ctx = CreateDbContext();
            var existing = await ctx.DepartmentGroup.FirstOrDefaultAsync(g => g.DepartmentGroupCode == "ULGRP");
            int groupId;
            if (existing != null) groupId = existing.Id;
            else
            {
                var grp = new UserManagement.Domain.Entities.DepartmentGroup
                {
                    DepartmentGroupCode = "ULGRP", DepartmentGroupName = "UL Group",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.DepartmentGroup.AddAsync(grp);
                await ctx.SaveChangesAsync();
                groupId = grp.Id;
            }

            var dept = await ctx.Department.FirstOrDefaultAsync(d => d.ShortName == "ULD");
            if (dept != null) return dept.Id;

            dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = "ULD", DeptName = "UL Dept", CompanyId = 1, DepartmentGroupId = groupId,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Department.AddAsync(dept);
            await ctx.SaveChangesAsync();
            return dept.Id;
        }

        private async Task<int> SeedUserAsync(string userName = "testuser")
        {
            var deptId = await EnsureDepartmentAsync();
            await using var ctx = CreateDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test", LastName = "User", UserName = userName,
                EmailId = $"{userName}@test.com", Mobile = "9999999999",
                DepartmentId = deptId,
                IsActive = Enums.Status.Active, IsDeleted = (Enums.IsDelete)0
            };
            await ctx.User.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user.UserId;
        }

        private async Task SeedUserUnitAsync(int userId, int unitId, byte isActive = 1)
        {
            await using var ctx = CreateDbContext();
            var uu = new UserUnit { UserId = userId, UnitId = unitId, IsActive = isActive };
            await ctx.UserUnit.AddAsync(uu);
            await ctx.SaveChangesAsync();
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Unit()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var unitId = await SeedUnitAsync(companyId, divId, unitTypeId, "Main Unit");

            var result = await CreateLookupRepo().GetByIdAsync(unitId);

            result.Should().NotBeNull();
            result!.UnitId.Should().Be(unitId);
            result.UnitName.Should().Be("Main Unit");
            result.UnitTypeName.Should().Be("Plant");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var unitId = await SeedUnitAsync(companyId, divId, unitTypeId);

            await using var ctx = CreateDbContext();
            var u = await ctx.Unit.FirstAsync(x => x.Id == unitId);
            u.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(unitId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateLookupRepo().GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Units()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var id1 = await SeedUnitAsync(companyId, divId, unitTypeId, "Unit A");
            var id2 = await SeedUnitAsync(companyId, divId, unitTypeId, "Unit B");
            await SeedUnitAsync(companyId, divId, unitTypeId, "Unit C");

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
        public async Task GetByIdsAsync_Should_Ignore_NonPositive_Ids()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var unitId = await SeedUnitAsync(companyId, divId, unitTypeId);

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { unitId, 0, -1 });

            results.Should().HaveCount(1);
        }

        // --- GetAllUnitAsync ---

        [Fact]
        public async Task GetAllUnitAsync_Should_Return_Seeded_Units()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            await SeedUnitAsync(companyId, divId, unitTypeId, "All Unit 1");
            await SeedUnitAsync(companyId, divId, unitTypeId, "All Unit 2");

            var results = await CreateLookupRepo().GetAllUnitAsync();

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllUnitAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var id1 = await SeedUnitAsync(companyId, divId, unitTypeId, "Keep");
            var id2 = await SeedUnitAsync(companyId, divId, unitTypeId, "Drop");

            await using var ctx = CreateDbContext();
            var u = await ctx.Unit.FirstAsync(x => x.Id == id2);
            u.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllUnitAsync();

            results.Should().HaveCount(1);
            results[0].UnitName.Should().Be("Keep");
        }

        // --- GetUserUnitAsync ---

        [Fact]
        public async Task GetUserUnitAsync_Should_Return_User_Units()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var unitId = await SeedUnitAsync(companyId, divId, unitTypeId, "UU Unit");
            var userId = await SeedUserAsync("uuuser");
            await SeedUserUnitAsync(userId, unitId);

            var results = await CreateLookupRepo().GetUserUnitAsync(userId);

            results.Should().HaveCount(1);
            results[0].UnitName.Should().Be("UU Unit");
        }

        [Fact]
        public async Task GetUserUnitAsync_Should_Exclude_InactiveUserUnit()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var unitId = await SeedUnitAsync(companyId, divId, unitTypeId);
            var userId = await SeedUserAsync("inactiveuser");
            await SeedUserUnitAsync(userId, unitId, isActive: 0);

            var results = await CreateLookupRepo().GetUserUnitAsync(userId);

            results.Should().BeEmpty();
        }

        // --- GetUserUnitByUserNameAsync ---

        [Fact]
        public async Task GetUserUnitByUserNameAsync_Should_Return_Matching_Units()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var (_, unitTypeId) = await EnsureUnitTypeAsync();
            var unitId = await SeedUnitAsync(companyId, divId, unitTypeId, "Named Unit");
            var userId = await SeedUserAsync("nameduser");
            await SeedUserUnitAsync(userId, unitId);

            var results = await CreateLookupRepo().GetUserUnitByUserNameAsync("nameduser");

            results.Should().HaveCount(1);
            results[0].UnitName.Should().Be("Named Unit");
        }

        [Fact]
        public async Task GetUserUnitByUserNameAsync_Should_Return_Empty_For_Unknown_User()
        {
            await ClearAsync();

            var results = await CreateLookupRepo().GetUserUnitByUserNameAsync("ghost");

            results.Should().BeEmpty();
        }
    }
}
