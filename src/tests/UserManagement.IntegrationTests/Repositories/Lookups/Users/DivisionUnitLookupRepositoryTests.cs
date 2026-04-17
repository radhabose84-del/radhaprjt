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
    public sealed class DivisionUnitLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DivisionUnitLookupRepositoryTests(DbFixture fixture)
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

        private DivisionUnitLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DivisionUnitLookupRepository(conn);
        }

        private async Task<int> EnsureCompanyAsync()
        {
            await using var ctx = CreateDbContext();
            var existing = await ctx.Companies.FirstOrDefaultAsync(c => c.CompanyName == "DUL Co" && c.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var company = new Company
            {
                CompanyName = "DUL Co", LegalName = "DUL Pvt", GstNumber = "GSTDUL",
                TIN = "T", TAN = "T", CSTNo = "C", YearOfEstablishment = 2020,
                Website = "https://x.com", Logo = "l", EntityId = 1, PanNumber = "PANDUL",
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted,
                CompanyAddress = new CompanyAddress
                { AddressLine1 = "X", AddressLine2 = "Y", PinCode = "600001",
                  AlternatePhone = "1", Phone = "2", CountryId = 1, StateId = 1, CityId = 1 },
                CompanyContact = new CompanyContact
                { Name = "X", Designation = "X", Email = "x@x.com", Phone = "1", Remarks = "X" }
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();
            return company.Id;
        }

        private async Task<int> SeedDivisionAsync(int companyId, string name = "DUL Div")
        {
            await using var ctx = CreateDbContext();
            var div = new UserManagement.Domain.Entities.Division
            {
                ShortName = "DUL", Name = name, CompanyId = companyId,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Divisions.AddAsync(div);
            await ctx.SaveChangesAsync();
            return div.Id;
        }

        private async Task<int> SeedUnitAsync(int companyId, int divisionId, string unitName = "DUL Unit")
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

            var unit = new UserManagement.Domain.Entities.Unit
            {
                UnitName = unitName, ShortName = "DU", CompanyId = companyId, DivisionId = divisionId,
                UnitHeadName = "H", CINNO = "C", OldUnitId = "O", IsMaintenanceStopStart = false,
                SpindlesCapacity = 100, UnitTypeId = miscMaster.Id,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted,
                UnitAddress = new UnitAddress
                { CountryId = 1, StateId = 1, CityId = 1, AddressLine1 = "A", AddressLine2 = "B",
                  PinCode = 600001, ContactNumber = "1", AlternateNumber = "2" },
                UnitContacts = new UnitContacts
                { Name = "X", Designation = "X", Email = "x@x.com", PhoneNo = "1", Remarks = "X" }
            };
            await ctx.Unit.AddAsync(unit);
            await ctx.SaveChangesAsync();
            return unit.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetUnitsByDivisionAsync ---

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Return_Matching_Units()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            await SeedUnitAsync(companyId, divId, "Unit A");
            await SeedUnitAsync(companyId, divId, "Unit B");

            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(companyId, divId);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Map_Columns_Correctly()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId, "Map Div");
            var unitId = await SeedUnitAsync(companyId, divId, "Map Unit");

            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(companyId, divId);

            results.Should().HaveCount(1);
            var dto = results[0];
            dto.UnitId.Should().Be(unitId);
            dto.UnitName.Should().Be("Map Unit");
            dto.DivisionId.Should().Be(divId);
            dto.DivisionName.Should().Be("Map Div");
            dto.CompanyId.Should().Be(companyId);
            dto.CompanyName.Should().Be("DUL Co");
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Return_Empty_When_InvalidCompanyId()
        {
            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(0, 1);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Return_Empty_When_InvalidDivisionId()
        {
            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(1, 0);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Exclude_SoftDeleted_Unit()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divId = await SeedDivisionAsync(companyId);
            var unitId = await SeedUnitAsync(companyId, divId, "Active Unit");
            await SeedUnitAsync(companyId, divId, "Keep Unit");

            await using var ctx = CreateDbContext();
            var unit = await ctx.Unit.FirstAsync(u => u.Id == unitId);
            unit.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(companyId, divId);

            results.Should().HaveCount(1);
            results[0].UnitName.Should().Be("Keep Unit");
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_Should_Filter_By_DivisionId()
        {
            await ClearAsync();
            var companyId = await EnsureCompanyAsync();
            var divA = await SeedDivisionAsync(companyId, "Div A");
            var divB = await SeedDivisionAsync(companyId, "Div B");
            await SeedUnitAsync(companyId, divA, "Unit A");
            await SeedUnitAsync(companyId, divB, "Unit B");

            var results = await CreateLookupRepo().GetUnitsByDivisionAsync(companyId, divA);

            results.Should().HaveCount(1);
            results[0].UnitName.Should().Be("Unit A");
        }
    }
}
